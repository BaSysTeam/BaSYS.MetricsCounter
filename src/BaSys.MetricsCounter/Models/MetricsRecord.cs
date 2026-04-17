namespace BaSys.MetricsCounter.Models;

public sealed record MetricsRecord(
    DateTime Timestamp,
    long FromStartMs,
    double CpuPercent,
    double CpuPercentTotal,
    double MemoryMB,
    DbConnectionStats? DbConnections = null);
