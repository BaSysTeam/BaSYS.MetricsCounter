using System.Globalization;
using System.Text;
using BaSys.MetricsCounter.Models;

namespace BaSys.MetricsCounter.Services;

public static class CsvExporter
{
    private const string Header = "timestamp,from_start_seconds,cpu_percent,memory_mb";

    public static string Export(IReadOnlyList<MetricsRecord> records, int pid)
    {
        var fileName = $"metrics_{pid}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

        var sb = new StringBuilder();
        sb.AppendLine(Header);

        foreach (var r in records)
        {
            sb.AppendLine(string.Join(",",
                r.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                r.FromStartSeconds.ToString("F2", CultureInfo.InvariantCulture),
                r.CpuPercent.ToString("F2", CultureInfo.InvariantCulture),
                r.MemoryMB.ToString("F2", CultureInfo.InvariantCulture)));
        }

        File.WriteAllText(filePath, sb.ToString());
        return filePath;
    }
}
