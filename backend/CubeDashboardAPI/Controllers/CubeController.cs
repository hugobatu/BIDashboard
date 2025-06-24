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
    public IActionResult GetDashboard()
    {
        string connStr = $"Data Source={_config.DataSource};Catalog={_config.Catalog}";
        string cube = _config.CubeName;

        var result = new
        {
            byMonth = QueryCube(connStr, $@"
                SELECT 
                    {{[Measures].[Fact Incident Count]}} ON COLUMNS,
                    FILTER(
                        [Dim Date].[Full Date].MEMBERS,
                        NOT ISEMPTY([Measures].[Fact Incident Count])
                    ) ON ROWS
                FROM [{cube}]")
        };

        return Ok(result);
    }

    private List<object> QueryCube(string connStr, string mdx)
    {
        var data = new List<object>();

        using (var conn = new AdomdConnection(connStr))
        {
            conn.Open();
            using (var cmd = new AdomdCommand(mdx, conn))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var label = reader.IsDBNull(0) ? null : reader[0].ToString();
                    var value = reader.IsDBNull(1) ? null : reader[1];

                    data.Add(new
                    {
                        Label = label,
                        Value = value
                    });
                }
            }
        }

        return data;
    }
}
