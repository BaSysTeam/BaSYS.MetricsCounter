using System.Diagnostics;
using BaSys.MetricsCounter.Models;

namespace BaSys.MetricsCounter.Services;

public sealed class ProcessMonitor : IDisposable
{
    private readonly int _pid;
    private readonly double _intervalSeconds;
    private readonly List<MetricsRecord> _records = new();
    private readonly DateTime _startTime;
    private readonly Stopwatch _stopwatch;

    private Process? _process;
    private TimeSpan _previousCpuTime;
    private DateTime _previousSampleTime;

    public ProcessMonitor(int pid, double intervalSeconds)
    {
        _pid = pid;
        _intervalSeconds = intervalSeconds;
        _startTime = DateTime.Now;
        _stopwatch = Stopwatch.StartNew();
    }

    public IReadOnlyList<MetricsRecord> Records => _records;

    public void ReplaceLastRecord(MetricsRecord record)
    {
        if (_records.Count > 0)
            _records[^1] = record;
    }

    public int Pid => _pid;

    public DateTime StartTime => _startTime;

    /// <summary>
    /// Attaches to the target process. Throws if process not found or access denied.
    /// </summary>
    public void Attach()
    {
        _process = Process.GetProcessById(_pid);
        _process.Refresh();
        _previousCpuTime = _process.TotalProcessorTime;
        _previousSampleTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Takes a single metrics sample. Returns null if the process has exited.
    /// </summary>
    public MetricsRecord? Sample()
    {
        if (_process is null)
            throw new InvalidOperationException("Call Attach() before sampling.");

        try
        {
            _process.Refresh();

            if (_process.HasExited)
                return null;

            var now = DateTime.UtcNow;
            var currentCpuTime = _process.TotalProcessorTime;

            var cpuElapsed = (currentCpuTime - _previousCpuTime).TotalMilliseconds;
            var wallElapsed = (now - _previousSampleTime).TotalMilliseconds;

            double cpuPercent = wallElapsed > 0
                ? cpuElapsed / wallElapsed / Environment.ProcessorCount * 100.0
                : 0;

            _previousCpuTime = currentCpuTime;
            _previousSampleTime = now;

            double memoryMB = _process.WorkingSet64 / (1024.0 * 1024.0);

            var record = new MetricsRecord(
                Timestamp: DateTime.Now,
                FromStartMs: _stopwatch.ElapsedMilliseconds,
                CpuPercent: Math.Round(Math.Max(cpuPercent, 0), 2),
                MemoryMB: Math.Round(memoryMB, 2));

            _records.Add(record);
            return record;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public void Dispose()
    {
        _process?.Dispose();
    }
}
