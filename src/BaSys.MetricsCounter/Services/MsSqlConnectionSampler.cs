using BaSys.MetricsCounter.Models;
using Microsoft.Data.SqlClient;

namespace BaSys.MetricsCounter.Services;

public sealed class MsSqlConnectionSampler : IDbConnectionSampler
{
    private const string Query = @"
SELECT COUNT(*) AS total,
       SUM(CASE WHEN r.request_id IS NOT NULL THEN 1 ELSE 0 END) AS active,
       SUM(CASE WHEN r.request_id IS NULL AND s.open_transaction_count = 0 THEN 1 ELSE 0 END) AS idle,
       SUM(CASE WHEN r.request_id IS NULL AND s.open_transaction_count > 0 THEN 1 ELSE 0 END) AS idle_in_tx
FROM sys.dm_exec_sessions s
LEFT JOIN sys.dm_exec_requests r ON s.session_id = r.session_id
WHERE s.database_id = DB_ID(@dbName)
  AND s.is_user_process = 1";

    private readonly string _connectionString;
    private SqlConnection? _connection;

    public string DatabaseName { get; }

    public MsSqlConnectionSampler(string connectionString)
    {
        _connectionString = connectionString;
        var builder = new SqlConnectionStringBuilder(connectionString);
        DatabaseName = builder.InitialCatalog;

        if (string.IsNullOrEmpty(DatabaseName))
            throw new ArgumentException(
                "Connection string must contain an Initial Catalog (Database) parameter.");
    }

    public void Connect()
    {
        _connection = new SqlConnection(_connectionString);
        _connection.Open();
    }

    public DbConnectionStats? Sample()
    {
        if (_connection is null)
            throw new InvalidOperationException("Call Connect() before sampling.");

        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Dispose();
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
            }

            using var cmd = new SqlCommand(Query, _connection);
            cmd.Parameters.AddWithValue("@dbName", DatabaseName);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new DbConnectionStats(
                    Total: reader.GetInt32(0),
                    Active: reader.GetInt32(1),
                    Idle: reader.GetInt32(2),
                    IdleInTransaction: reader.GetInt32(3));
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
