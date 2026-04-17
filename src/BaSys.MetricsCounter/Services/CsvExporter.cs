using System.Globalization;
using System.Text;
using BaSys.MetricsCounter.Models;

namespace BaSys.MetricsCounter.Services;

public static class CsvExporter
{
    private const string BaseHeader = "timestamp,from_start_ms,cpu_percent,cpu_percent_total,memory_mb";
    private const string DbHeader = ",db_total,db_active,db_idle,db_idle_in_tx";

    private const string ResultsFolder = "Results";

    public static string Export(IReadOnlyList<MetricsRecord> records, int pid)
    {
        var resultsDir = Path.Combine(Directory.GetCurrentDirectory(), ResultsFolder);
        Directory.CreateDirectory(resultsDir);

        var fileName = $"metrics_{pid}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = Path.Combine(resultsDir, fileName);

        bool hasDb = records.Any(r => r.DbConnections is not null);

        var sb = new StringBuilder();
        sb.AppendLine(hasDb ? BaseHeader + DbHeader : BaseHeader);

        foreach (var r in records)
        {
            sb.Append(string.Join(",",
                r.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                r.FromStartMs,
                r.CpuPercent.ToString("F2", CultureInfo.InvariantCulture),
                r.CpuPercentTotal.ToString("F2", CultureInfo.InvariantCulture),
                r.MemoryMB.ToString("F2", CultureInfo.InvariantCulture)));

            if (hasDb)
            {
                var db = r.DbConnections;
                sb.Append(db is not null
                    ? $",{db.Total},{db.Active},{db.Idle},{db.IdleInTransaction}"
                    : ",,,,");
            }

            sb.AppendLine();
        }

        File.WriteAllText(filePath, sb.ToString());
        return filePath;
    }
}
