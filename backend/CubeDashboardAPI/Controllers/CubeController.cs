using Microsoft.AspNetCore.Mvc;
using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Net.Http.Headers;
using System.Xml.XPath;

[ApiController]
[Route("")]
public class CubeController : ControllerBase
{
    private readonly SqlQueryService _sqlService;
    private readonly CubeConfig _config;
    private readonly SqlDbConfig _sqlConfig;

    public CubeController(CubeConfig config, SqlDbConfig sqlConfig, SqlQueryService sqlService)
    {
        _sqlService = sqlService;
        _config = config;
        _sqlConfig = sqlConfig;
    }

    // 1.1 Tổng số lượng Incident gần nhất trong 1 năm (trả về tháng gần nhất trong năm đó)
    [HttpGet("latestIncidentInYear")]
    public IActionResult GetLatestIncidentInYear(int year)
    {
        string sql = $@"
            SELECT MAX(D.Month)
            FROM [dw].[DimDate] D
            JOIN [dw].[FactIncident] F
            ON F.DateKey = D.DateKey
            WHERE D.Year = {year}
        ";

        object? latestMonthIncident = _sqlService.QueryScalar(sql);
        if (latestMonthIncident == null)
        {
            return NotFound($"No available data found for year {year}.");
        }

        string month = latestMonthIncident.ToString()!;

        string connStr = $"Data Source={_config.DataSource};Catalog={_config.Catalog}";
        string cube = _config.CubeName;

        string mdx = $@"
            SELECT
                {{[Measures].[Fact Incident Count]}} ON COLUMNS
            FROM [{cube}]
            WHERE (
                [Dim Date].[Year].&[{year}],
                [Dim Date].[Month].&[{month}]
            )
        ";

        var headers = new List<string> { $"Incident Count (latest month {month} in {year})" };
        var result = QueryCube(connStr, mdx, headers);

        var response = new
        {
            latestMonth = int.Parse(month),
            year,
            data = result
        };

        return Ok(response);
    }
    // 1.2 Tổng số lượng Incident gần nhất trong 1 tháng (trả về ngày gần nhất của tháng đó)
    [HttpGet("latestIncidentInMonthYear")]
    public IActionResult GetLatestIncidentInMonthYear(int year, int month)
    {
        string sql = $@"
            SELECT MAX(D.Day)
            FROM [dw].[DimDate] D
            JOIN [dw].[FactIncident] F
            ON F.DateKey = D.DateKey
            WHERE D.Year = {year} AND D.Month = {month}
        ";

        object? latestMonthIncident = _sqlService.QueryScalar(sql);
        if (latestMonthIncident == null)
        {
            return NotFound($"No available data found for year {year}.");
        }

        string day = latestMonthIncident.ToString()!;

        string connStr = $"Data Source={_config.DataSource};Catalog={_config.Catalog}";
        string cube = _config.CubeName;

        string mdx = $@"
            SELECT
                {{[Measures].[Fact Incident Count]}} ON COLUMNS
            FROM [{cube}]
            WHERE (
                [Dim Date].[Year].&[{year}],
                [Dim Date].[Month].&[{month}],
                [Dim Date].[Day].&[{day}]
            )
        ";

        var headers = new List<string> { $"Incident Count (latest day {day} in month {month} year {year})" };
        var result = QueryCube(connStr, mdx, headers);

        var response = new
        {
            latestDay = int.Parse(day),
            month = month,
            year,
            data = result
        };

        return Ok(response);
    }
    // 2.1 Tổng số incident (ra 1 con số, filter theo năm)
    [HttpGet("totalIncidentByYear")]
    public IActionResult GetTotalIncidentByYear(int year)
    {
        string cube = _config.CubeName;
        string connStr = $"Data Source={_config.DataSource};Catalog={_config.Catalog}";
        string mdx = $@"
            SELECT 
                {{[Measures].[Fact Incident Count]}} ON COLUMNS
            FROM [{cube}]
            WHERE (
                [Dim Date].[Year].&[{year}]
            )
        ";
        var headers = new List<string> { $"Total Incident in year {year}" };
        var result = QueryCube(connStr, mdx, headers);

        return Ok(result);
    }
    // 2.2 Tổng số incident (ra 1 con số, filter theo tháng + năm)
    [HttpGet("totalIncidentByMonthYear")]
    public IActionResult GetTotalIncidentByMonthYear(int year, int month)
    {
        string cube = _config.CubeName;
        string connStr = $"Data Source={_config.DataSource};Catalog={_config.Catalog}";
        string mdx = $@"
            SELECT
                {{[Measures].[Fact Incident Count]}} ON COLUMNS
            FROM [{cube}]
            WHERE (
                [Dim Date].[Year].&[{year}],
                [Dim Date].[Month].&[{month}]
            )
        ";
        var headers = new List<string> { $"Total Incident in month {month} year {year}" };
        var result = QueryCube(connStr, mdx, headers);

        return Ok(result);
    }
    // 3.1 Tổng số incident có độ ưu tiên cao (filter theo năm)
    [HttpGet("highPrioritizedIncidentByYear")]
    public IActionResult GetPrioritizedIncidentByYear(int year)
    {
        string cube = _config.CubeName;
        string connStr = $"Data Source={_config.DataSource};Catalog={_config.Catalog}";
        string mdx = $@"
            SELECT
                {{[Measures].[Fact Incident Count]}} ON COLUMNS
            FROM [BI Do An]
            WHERE (
                    {{[Dim Priority].[Priority Code].&[1C]}},
                    [Dim Date].[Year].&[{year}]
            )";
        var headers = new List<string> { $"Total Incident in year {year}" };
        var result = QueryCube(connStr, mdx, headers);

        return Ok(result);
    }
    // 3.2 Tổng số incident có độ ưu tiên cao (filter theo tháng + năm)
    [HttpGet("highPrioritizedIncidentByMonthYear")]
    public IActionResult GetPrioritizedIncidentByMonthYear(int year, int month)
    {
        string cube = _config.CubeName;
        string connStr = $"Data Source={_config.DataSource};Catalog={_config.Catalog}";
        string mdx = $@"
            SELECT
                {{[Measures].[Fact Incident Count]}} ON COLUMNS
            FROM [BI Do An]
            WHERE (
                    {{[Dim Priority].[Priority Code].&[1C]}},
                    [Dim Date].[Year].&[{year}],
                    [Dim Date].[Month].&[{month}]
            )";
        var headers = new List<string> { $"Total Incident in month {month} year {year}" };
        var result = QueryCube(connStr, mdx, headers);
        
        return Ok(result);
    }
    // 4.1 Số lượng Incident theo từng service (filter theo năm)
    [HttpGet("totalIncidentByServiceYear")]
    public IActionResult GetTotalIncidentByServiceYear(int year)
    {
        string cube = _config.CubeName;
        string connStr = $"Data Source={_config.DataSource};Catalog={_config.Catalog}";
        string mdx = $@"
            SELECT
                {{[Measures].[Fact Incident Count]}} ON COLUMNS,
                [Dim Business Service].[Business Service Name].[Business Service Name] ON ROWS
            FROM [BI Do An]
            WHERE (
                [Dim Date].[Year].&[{year}]
            )";
        var headers = new List<string> { $"Service", $"Total Incident in year {year}"};
        var result = QueryCube(connStr, mdx, headers);
        
        return Ok(result);
    }
    // 4.2 Số lượng Incident theo từng service (filter theo tháng + năm)
    [HttpGet("totalIncidentByServiceMonthYear")]
    public IActionResult GetTotalIncidentByServiceMonthYear(int year, int month)
    {
        string cube = _config.CubeName;
        string connStr = $"Data Source={_config.DataSource};Catalog={_config.Catalog}";
        string mdx = $@"
            SELECT
                {{[Measures].[Fact Incident Count]}} ON COLUMNS,
                [Dim Business Service].[Business Service Name].[Business Service Name] ON ROWS
            FROM [BI Do An]
            WHERE (
                [Dim Date].[Year].&[{year}],
                [Dim Date].[Month].&[{month}]
            )";
        var headers = new List<string> { $"Service", $"Total Incident in year {year}"};
        var result = QueryCube(connStr, mdx, headers);
        
        return Ok(result);
    }
    // 5.1 Số lượng incident của từng service theo từng tháng trong 1 năm (filter theo theo năm)
    [HttpGet("incidentByServiceYear")]
    public IActionResult GetIncidentByServiceYear(int year)
    {
        string cube = _config.CubeName;
        string connStr = $"Data Source={_config.DataSource};Catalog={_config.Catalog}";
        string mdx = $@"
            SELECT 
                NON EMPTY {{[Measures].[Fact Incident Count]}} ON COLUMNS,
                NON EMPTY 
                (
                    [Dim Business Service].[Business Service Name].MEMBERS *
                    [Dim Date].[Month].MEMBERS
                ) ON ROWS
            FROM [BI Do An]
            WHERE (
            [Dim Date].[Year].&[{year}]
            )";
        var headers = new List<string> { $"Service", $"Month", $"Total Incident"};
        var result = QueryCube(connStr, mdx, headers);
        
        return Ok(result);
    }
    // 5.2 Số lượng incident của từng service theo từng ngày trong 1 tháng (filter theo theo tháng + năm)
    [HttpGet("incidentByServiceMonthYear")]
    public IActionResult GetIncidentByServiceMonthYear(int year, int month)
    {
        string cube = _config.CubeName;
        string connStr = $"Data Source={_config.DataSource};Catalog={_config.Catalog}";
        string mdx = $@"
            SELECT 
                NON EMPTY {{[Measures].[Fact Incident Count]}} ON COLUMNS,
                NON EMPTY 
                (
                    [Dim Business Service].[Business Service Name].MEMBERS *
                    [Dim Date].[Day].MEMBERS
                ) ON ROWS
            FROM [BI Do An]
            WHERE (
                [Dim Date].[Year].&[{year}], 
                [Dim Date].[Month].&[{month}]
            )";
        var headers = new List<string> { $"Service", $"Day", $"Total Incident"};
        var result = QueryCube(connStr, mdx, headers);
        
        return Ok(result);
    }
    //  MDX query
    private List<Dictionary<string, object?>> QueryCube(string connStr, string mdx, List<string>? customHeaders = null)
    {
        var data = new List<Dictionary<string, object?>>();
        using var conn = new AdomdConnection(connStr);
        conn.Open();

        using var cmd = new AdomdCommand(mdx, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var row = new Dictionary<string, object?>();
            bool hasNullDimension = false;

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string header = (customHeaders != null && i < customHeaders.Count)
                    ? customHeaders[i]
                    : string.IsNullOrWhiteSpace(reader.GetName(i)) ? $"Column{i}" : reader.GetName(i);

                object? value = reader.IsDBNull(i) ? null : reader[i];

                // Nếu dimension mà null (và không phải chỉ cột measure), thì bỏ qua dòng này
                // Giả định: measure luôn là cột cuối cùng (hoặc tự cấu hình tên cột để phân biệt)
                if (i < reader.FieldCount - 1 && value == null)
                {
                    hasNullDimension = true;
                    break;
                }

                row[header] = value;
            }

            if (!hasNullDimension)
                data.Add(row);
        }

        return data;
    }
}