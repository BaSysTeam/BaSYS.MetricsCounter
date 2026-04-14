# BaSys.MetricsCounter

A lightweight .NET 8 console utility that monitors CPU and memory usage of a running process in real time, with a live-updating terminal UI powered by [Spectre.Console](https://spectreconsole.net/). When monitoring stops, all collected data is exported to a CSV file.

## Features

- Real-time CPU % and memory (MB) monitoring for any process by PID
- Configurable sampling interval (default: 1 second)
- Live-updating table in the terminal with color-coded CPU values
- Automatic CSV export on exit with timestamps and elapsed time
- Graceful shutdown via Ctrl+C

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

## Build

```bash
cd src/BaSys.MetricsCounter
dotnet build
```

To publish a self-contained executable:

```bash
dotnet publish -c Release -r win-x64
```

## Usage

```
BaSys.MetricsCounter --pid <PID> [--interval <seconds>]
BaSys.MetricsCounter --help
```

### Arguments

| Argument               | Required | Default | Description                          |
|------------------------|----------|---------|--------------------------------------|
| `--pid <PID>`          | Yes      | —       | Process ID to monitor                |
| `--interval <seconds>` | No       | `1`     | Sampling interval in seconds         |
| `--help`               | No       | —       | Display usage instructions           |

### Examples

Monitor process with PID 1234 at the default 1-second interval:

```bash
BaSys.MetricsCounter --pid 1234
```

Monitor with a 500ms interval:

```bash
BaSys.MetricsCounter --pid 1234 --interval 0.5
```

Show help:

```bash
BaSys.MetricsCounter --help
```

### Stopping

Press **Ctrl+C** to stop monitoring. The utility will gracefully shut down and export all collected samples to a CSV file in the current directory.

If the target process exits on its own during monitoring, the utility detects this automatically and exports the data collected up to that point.

## Console Output

While running, the utility displays a live-updating table:

```
╭────────────────── Process Metrics Monitor (42 samples) ──────────────────╮
│  #  │ Timestamp      │ From Start (s) │  CPU (%) │ Memory (MB) │
│  40 │ 14:30:42.123   │          39.01 │     3.25 │      128.45 │
│  41 │ 14:30:43.125   │          40.02 │    12.50 │      129.01 │
│  42 │ 14:30:44.127   │          41.03 │     5.10 │      128.88 │
╰──────────────────────────────────────────────────────────────────────────╯
```

CPU values are color-coded:
- **Green**: < 50%
- **Yellow**: 50% – 80%
- **Red**: > 80%

## CSV Output

On exit, a file named `metrics_<pid>_<yyyyMMdd_HHmmss>.csv` is created in the current working directory.

### Format

```csv
timestamp,from_start_seconds,cpu_percent,memory_mb
2026-04-14 14:30:03.456,0.00,0.00,128.32
2026-04-14 14:30:04.458,1.00,3.25,128.45
2026-04-14 14:30:05.460,2.01,12.50,129.01
```

| Column              | Description                                      |
|---------------------|--------------------------------------------------|
| `timestamp`         | Wall-clock time of the sample (local time)       |
| `from_start_seconds`| Seconds elapsed since monitoring started         |
| `cpu_percent`       | CPU usage percentage (across all cores)           |
| `memory_mb`         | Working set memory in megabytes                  |

## License

See [LICENSE](LICENSE) for details.
