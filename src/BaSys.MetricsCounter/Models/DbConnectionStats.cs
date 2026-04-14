namespace BaSys.MetricsCounter.Models;

public sealed record DbConnectionStats(
    int Total,
    int Active,
    int Idle,
    int IdleInTransaction);
