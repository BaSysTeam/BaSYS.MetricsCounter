namespace BaSys.MetricsCounter.Models;

public sealed record MetricsRecord(
    DateTime Timestamp,
    double FromStartSeconds,
    double CpuPercent,
    double MemoryMB);
