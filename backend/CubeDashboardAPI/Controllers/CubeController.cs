using Microsoft.AspNetCore.Mvc;
using Microsoft.AnalysisServices.AdomdClient;
using System.Text;

[ApiController]
[Route("")]
public class CubeController : ControllerBase
{
    private readonly CubeConfig _config;
    private readonly SqlQueryService _sqlService;

    public CubeController(CubeConfig config, SqlQueryService sqlService)
    {
        _config = config;
        _sqlService = sqlService;
    }

    [HttpGet("kpis")]
    public IActionResult GetKpis([FromQuery] int year, [FromQuery] int? month, [FromQuery] List<string>? assignmentGroups)
    {
        string cube = _config.CubeName;

        string dateFilter = month.HasValue
            ? $"([Dim Date].[Year].&[{year}], [Dim Date].[Month].&[{month.Value}])"
            : $"[Dim Date].[Year].&[{year}]";

        string groupFilter = BuildGroupFilter(assignmentGroups);

        string mdx = $@"
            WITH
                MEMBER [Measures].[Total Incidents] AS [Measures].[Fact Incident Count]
                MEMBER [Measures].[High Priority Incidents] AS 
                    SUM({{ [Dim Priority].[Priority Code].&[1C], [Dim Priority].[Priority Code].&[2H] }}, [Measures].[Fact Incident Count])
            SELECT
                {{
                    [Measures].[Total Incidents],
                    [Measures].[High Priority Incidents]
                }} ON COLUMNS
            FROM [{cube}]
            WHERE ({dateFilter}{groupFilter})";

        var kpiResult = QueryKpiCube(mdx);
        var latestResult = GetLatestIncidentValue(year, month, assignmentGroups);

        var response = new
        {
            total = kpiResult.GetValueOrDefault("[Measures].[Total Incidents]", 0),
            highPriority = kpiResult.GetValueOrDefault("[Measures].[High Priority Incidents]", 0),
            latestValue = latestResult.latestValue,
            latestTitle = latestResult.latestTitle
        };

        return Ok(response);
    }

    [HttpGet("service-priority-distribution")]
    public IActionResult GetServicePriorityDistribution([FromQuery] int year, [FromQuery] int? month, [FromQuery] List<string>? assignmentGroups)
    {
        string dateFilter = month.HasValue ? $"([Dim Date].[Year].&[{year}], [Dim Date].[Month].&[{month.Value}])" : $"[Dim Date].[Year].&[{year}]";
        string groupFilter = BuildGroupFilter(assignmentGroups);

        string mdx = $@"
            SELECT 
                NON EMPTY {{[Measures].[Fact Incident Count]}} ON COLUMNS,
                NON EMPTY (
                    [Dim Business Service].[Business Service Name].MEMBERS *
                    [Dim Priority].[Priority Name].MEMBERS *
                    [Dim Assignment Group].[Assignment Group Name].MEMBERS
                ) ON ROWS
            FROM [{_config.CubeName}]
            WHERE ({dateFilter}{groupFilter})";

        var headers = new List<string> { "Service", "Priority", "AssignmentGroup", "Count" };
        var result = QueryCube(mdx, headers);
        return Ok(result);
    }

    [HttpGet("trend-by-daeo-group")]
    public IActionResult GetTrendByDaeoGroup([FromQuery] int year, [FromQuery] int? month)
    {
        string cube = _config.CubeName;
        string dateDimension = month.HasValue ? "[Dim Date].[Day]" : "[Dim Date].[Month]";
        string dateFilter = month.HasValue
            ? $"([Dim Date].[Year].&[{year}], [Dim Date].[Month].&[{month.Value}])"
            : $"[Dim Date].[Year].&[{year}]";

        string[] daeoGroups = { "DAEO" };

        var daeoMembers = string.Join(", ", daeoGroups.Select(g => $"[Dim Assignment Group].[Assignment Group Name].&[{g}]"));

        string mdx = $@"
        WITH 
            MEMBER [Dim Assignment Group].[Assignment Group Name].[DAEO] AS 
                SUM( {{ {daeoMembers} }} )
            MEMBER [Dim Assignment Group].[Assignment Group Name].[NON - DAEO] AS 
                ([Dim Assignment Group].[Assignment Group Name].[All] - [Dim Assignment Group].[Assignment Group Name].[DAEO])
        SELECT 
            NON EMPTY {{[Measures].[Fact Incident Count]}} ON COLUMNS,
            NON EMPTY (
                {{ 
                    [Dim Assignment Group].[Assignment Group Name].[DAEO], 
                    [Dim Assignment Group].[Assignment Group Name].[NON - DAEO] 
                }} *
                {dateDimension}.MEMBERS
            ) ON ROWS
        FROM [{cube}]
        WHERE ({dateFilter})";

        var headers = new List<string> { "AssignmentGroup", "Time", "Count" };
        var result = QueryCube(mdx, headers);
        return Ok(result);
    }

    [HttpGet("shift-priority-distribution")]
    public IActionResult GetShiftPriorityDistribution([FromQuery] int year, [FromQuery] int? month, [FromQuery] List<string>? assignmentGroups)
    {
        string dateFilter = month.HasValue ? $"([Dim Date].[Year].&[{year}], [Dim Date].[Month].&[{month.Value}])" : $"[Dim Date].[Year].&[{year}]";
        string groupFilter = BuildGroupFilter(assignmentGroups);

        string mdx = $@"
            SELECT 
                NON EMPTY {{[Measures].[Fact Incident Count]}} ON COLUMNS,
                NON EMPTY (
                    [Dim Shift].[Shift Name].MEMBERS *
                    [Dim Priority].[Priority Name].MEMBERS *
                    [Dim Assignment Group].[Assignment Group Name].MEMBERS
                ) ON ROWS
            FROM [{_config.CubeName}]
            WHERE ({dateFilter}{groupFilter})";

        var headers = new List<string> { "Shift", "Priority", "AssignmentGroup", "Count" };
        var result = QueryCube(mdx, headers);
        return Ok(result);
    }

    private (int latestValue, string latestTitle) GetLatestIncidentValue(int year, int? month, List<string>? assignmentGroups)
    {
        string groupFilterMdx = BuildGroupFilter(assignmentGroups);
        string mdx;
        string dateFilter;
        string latestTitle;

        if (month.HasValue)
        {
            string sql = $@"
                SELECT MAX(D.Day)
                FROM [dw].[FactIncident] F
                JOIN [dw].[DimDate] D ON F.DateKey = D.DateKey
                WHERE D.Year = {year} AND D.Month = {month.Value}";

            var latestDayResult = _sqlService.QueryScalar(sql);
            int latestDay = (latestDayResult != null && latestDayResult != DBNull.Value) ? Convert.ToInt32(latestDayResult) : 0;

            if (latestDay == 0) return (0, $"Incident Mới (Ngày)");

            dateFilter = $"([Dim Date].[Year].&[{year}], [Dim Date].[Month].&[{month.Value}], [Dim Date].[Day].&[{latestDay}])";
            latestTitle = $"Incident Mới (Ngày {latestDay})";
        }
        else
        {
            string sql = $@"
                SELECT MAX(D.Month)
                FROM [dw].[FactIncident] F
                JOIN [dw].[DimDate] D ON F.DateKey = D.DateKey
                WHERE D.Year = {year}";

            var latestMonthResult = _sqlService.QueryScalar(sql);
            int latestMonth = (latestMonthResult != null && latestMonthResult != DBNull.Value) ? Convert.ToInt32(latestMonthResult) : 0;

            if (latestMonth == 0) return (0, $"Incident Mới (Tháng)");

            dateFilter = $"([Dim Date].[Year].&[{year}], [Dim Date].[Month].&[{latestMonth}])";
            latestTitle = $"Incident Mới (Tháng {latestMonth})";
        }

        mdx = $@"
            SELECT {{[Measures].[Fact Incident Count]}} ON COLUMNS
            FROM [{_config.CubeName}]
            WHERE ({dateFilter}{groupFilterMdx})";

        var resultDict = QueryKpiCube(mdx);
        return (resultDict.GetValueOrDefault("[Measures].[Fact Incident Count]", 0), latestTitle);
    }

    private string BuildGroupFilter(List<string>? groups)
    {
        if (groups == null || !groups.Any()) return "";
        var sb = new StringBuilder();
        sb.Append(", {");
        for (int i = 0; i < groups.Count; i++)
        {
            sb.Append($"[Dim Assignment Group].[Assignment Group Name].&[{groups[i]}]");
            if (i < groups.Count - 1) sb.Append(", ");
        }
        sb.Append("}");
        return sb.ToString();
    }

    private Dictionary<string, int> QueryKpiCube(string mdx)
    {
        string connStr = $"Data Source={_config.DataSource};Catalog={_config.Catalog}";
        var result = new Dictionary<string, int>();
        using var conn = new AdomdConnection(connStr);
        conn.Open();
        using var cmd = new AdomdCommand(mdx, conn);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                result[reader.GetName(i)] = reader.IsDBNull(i) ? 0 : Convert.ToInt32(reader[i]);
            }
        }
        return result;
    }

    private List<Dictionary<string, object?>> QueryCube(string mdx, List<string> headers)
    {
        string connStr = $"Data Source={_config.DataSource};Catalog={_config.Catalog}";
        var data = new List<Dictionary<string, object?>>();
        using var conn = new AdomdConnection(connStr);
        conn.Open();
        using var cmd = new AdomdCommand(mdx, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[headers[i]] = reader.IsDBNull(i) ? null : reader[i];
            }
            data.Add(row);
        }
        return data;
    }
}