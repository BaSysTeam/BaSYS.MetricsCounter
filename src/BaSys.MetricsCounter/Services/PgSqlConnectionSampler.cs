using BaSys.MetricsCounter.Models;
using Npgsql;

namespace BaSys.MetricsCounter.Services;

public sealed class PgSqlConnectionSampler : IDbConnectionSampler
{
    private const string Query = @"
SELECT count(*) AS total,
       count(*) FILTER (WHERE state = 'active') AS active,
       count(*) FILTER (WHERE state = 'idle') AS idle,
       count(*) FILTER (WHERE state = 'idle in transaction') AS idle_in_tx
FROM pg_stat_activity
WHERE datname = @dbName";

    private readonly string _connectionString;
    private NpgsqlConnection? _connection;

    public string DatabaseName { get; }

    public PgSqlConnectionSampler(string connectionString)
    {
        _connectionString = connectionString;
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        DatabaseName = builder.Database
            ?? throw new ArgumentException("Connection string must contain a Database parameter.");
    }

    public void Connect()
    {
        _connection = new NpgsqlConnection(_connectionString);
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
                _connection = new NpgsqlConnection(_connectionString);
                _connection.Open();
            }

            using var cmd = new NpgsqlCommand(Query, _connection);
            cmd.Parameters.AddWithValue("dbName", DatabaseName);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new DbConnectionStats(
                    Total: (int)reader.GetInt64(0),
                    Active: (int)reader.GetInt64(1),
                    Idle: (int)reader.GetInt64(2),
                    IdleInTransaction: (int)reader.GetInt64(3));
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
