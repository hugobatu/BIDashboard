using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;

public class SqlQueryService
{
    private readonly string _connStr;

    public SqlQueryService(SqlDbConfig config)
    {
        _connStr = config.ConnectionString;
    }


    public List<Dictionary<string, object?>> Query(string sql)
    {
        var result = new List<Dictionary<string, object?>>();

        using var conn = new SqlConnection(_connStr);
        conn.Open();

        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        
        while (reader.Read())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader[i];
                row[columnName] = value;
            }
            result.Add(row);
        }

        return result;
    }

    public object? QueryScalar(string sql)
    {
        using var conn = new SqlConnection(_connStr);
        conn.Open();

        using var cmd = new SqlCommand(sql, conn);
        return cmd.ExecuteScalar();
    }
}
