using Microsoft.AspNetCore.Mvc;
using Microsoft.AnalysisServices.AdomdClient;
using System.Collections.Generic;

[ApiController]
[Route("")]
public class CubeController : ControllerBase
{
    private readonly CubeConfig _config;

    public CubeController(CubeConfig config)
    {
        _config = config;
    }

    [HttpGet("dashboard")]
    public IActionResult GetDashboard(int year, int month)
    {
        string connStr = $"Data Source={_config.DataSource};Catalog={_config.Catalog}";
        string cube = _config.CubeName;

        var result = new
        {
            // 1. Số lượng incident theo service trong 1 năm và 1 tháng cụ thể ở các ngày (multi-line chart)
            byServiceInMonthYear = QueryCube(connStr, $@"
                SELECT 
                    NON EMPTY 
                        {{[Measures].[Fact Incident Count]}} ON COLUMNS,
                    NON EMPTY 
                        ([Dim Business Service].[Business Service Name].MEMBERS *
                        [Dim Date].[Day].MEMBERS) ON ROWS
                FROM [{cube}]
                WHERE (
                    [Dim Date].[Year].&[{year}], 
                    [Dim Date].[Month].&[{month}])
            "),

            // 2. Incident theo từng service trong 12 tháng của 1 năm cụ thể (multi-line chart)
            byServiceInYear = QueryCube(connStr, $@"
                SELECT 
                    [Measures].[Fact Incident Count] ON COLUMNS,
                    NON EMPTY (
                        [Dim Business Service].[Business Service Name].MEMBERS *
                        [Dim Date].[Month].MEMBERS
                    ) ON ROWS
                FROM [{cube}]
                WHERE ([Dim Date].[Year].&[{year}])
            ")

            // 3. Tổng số lượng incident theo ca trong các service (bar chart)

            
        };

        return Ok(result);
    }

    private List<Dictionary<string, object?>> QueryCube(string connStr, string mdx)
    {
        var data = new List<Dictionary<string, object?>>();

        using (var conn = new AdomdConnection(connStr))
        {
            conn.Open();
            using (var cmd = new AdomdCommand(mdx, conn))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var row = new Dictionary<string, object?>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);
                        object? value = reader.IsDBNull(i) ? null : reader[i];
                        row[columnName] = value;
                    }

                    data.Add(row);
                }
            }
        }

        return data;
    }

}
