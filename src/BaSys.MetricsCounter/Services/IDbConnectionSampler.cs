using BaSys.MetricsCounter.Models;

namespace BaSys.MetricsCounter.Services;

public interface IDbConnectionSampler : IDisposable
{
    string DatabaseName { get; }

    void Connect();

    DbConnectionStats? Sample();
}
