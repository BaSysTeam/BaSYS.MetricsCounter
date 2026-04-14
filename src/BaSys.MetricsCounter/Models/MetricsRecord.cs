namespace BaSys.MetricsCounter.Models;

public sealed record MetricsRecord(
    DateTime Timestamp,
    long FromStartMs,
    double CpuPercent,
    double MemoryMB);
