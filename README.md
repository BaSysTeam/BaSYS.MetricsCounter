# BaSys.MetricsCounter

A lightweight .NET 8 console utility that monitors CPU and memory usage of a running process in real time, with a live-updating terminal UI powered by [Spectre.Console](https://spectreconsole.net/). Optionally monitors the number of database connections (PostgreSQL or MS SQL). When monitoring stops, all collected data is exported to a CSV file.

## Features

- Real-time CPU % and memory (MB) monitoring for any process by PID
- Optional database connection count monitoring (total, active, idle, idle in transaction)
- Supports PostgreSQL (`pg_stat_activity`) and MS SQL Server (`sys.dm_exec_sessions`)
- Configurable sampling interval (default: 1 second)
- Live-updating table in the terminal with color-coded values
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
BaSys.MetricsCounter --pid <PID> [--interval <seconds>] [--db-type <pgsql|mssql> --db-connection <string>]
BaSys.MetricsCounter --help
```

### Arguments

| Argument                     | Required | Default | Description                                     |
|------------------------------|----------|---------|-------------------------------------------------|
| `--pid <PID>`                | Yes      | —       | Process ID to monitor                           |
| `--interval <seconds>`       | No       | `1`     | Sampling interval in seconds                    |
| `--db-type <pgsql\|mssql>`   | No       | —       | Database type (`pgsql` or `mssql`)              |
| `--db-connection <string>`   | No       | —       | Database connection string                      |
| `--help`                     | No       | —       | Display usage instructions                      |

> `--db-type` and `--db-connection` must be specified together. The database name for filtering is extracted from the connection string automatically (`Database` for PostgreSQL, `Initial Catalog` / `Database` for MS SQL).

### Examples

Monitor process with PID 1234 at the default 1-second interval:

```bash
BaSys.MetricsCounter --pid 1234
```

Monitor with a 500ms interval:

```bash
BaSys.MetricsCounter --pid 1234 --interval 0.5
```

Monitor process and PostgreSQL database connections:

```bash
BaSys.MetricsCounter --pid 1234 --db-type pgsql --db-connection "Host=localhost;Database=elevator;Username=postgres;Password=secret"
```

Monitor process and MS SQL database connections:

```bash
BaSys.MetricsCounter --pid 1234 --db-type mssql --db-connection "Server=.;Database=elevator;Trusted_Connection=True;TrustServerCertificate=True"
```

Show help:

```bash
BaSys.MetricsCounter --help
```

### Stopping

Press **Ctrl+C** to stop monitoring. The utility will gracefully shut down and export all collected samples to a CSV file in the `Results/` folder.

If the target process exits on its own during monitoring, the utility detects this automatically and exports the data collected up to that point.

## Console Output

While running, the utility displays a live-updating table:

```
╭──────────────────────── Process Metrics Monitor (42 samples) ────────────────────────╮
│  #  │ Timestamp      │ From Start (ms) │ CPU (%) │ Memory (MB) │                     │
│  40 │ 14:30:42.123   │           39010 │    3.25 │      128.45 │                     │
│  41 │ 14:30:43.125   │           40020 │   12.50 │      129.01 │                     │
│  42 │ 14:30:44.127   │           41030 │    5.10 │      128.88 │                     │
╰──────────────────────────────────────────────────────────────────────────────────────╯
```

When database monitoring is enabled, additional columns are shown:

```
│ ... │ Memory (MB) │ DB Total │ DB Active │ DB Idle │ DB Idle in Tx │
│ ... │      128.45 │       12 │         3 │       8 │             1 │
```

CPU values are color-coded:
- **Green**: < 50%
- **Yellow**: 50% -- 80%
- **Red**: > 80%

DB Active connections are color-coded:
- **Green**: < 20
- **Yellow**: 20 -- 49
- **Red**: >= 50

DB Idle in Tx is highlighted in yellow when > 0.

## CSV Output

On exit, a file named `metrics_<pid>_<yyyyMMdd_HHmmss>.csv` is created in the `Results/` subdirectory of the current working directory. The folder is created automatically if it does not exist.

### Format

Without database monitoring:

```csv
timestamp,from_start_ms,cpu_percent,memory_mb
2026-04-14 14:30:03.456,0,0.00,128.32
2026-04-14 14:30:04.458,1002,3.25,128.45
```

With database monitoring enabled:

```csv
timestamp,from_start_ms,cpu_percent,memory_mb,db_total,db_active,db_idle,db_idle_in_tx
2026-04-14 14:30:03.456,0,0.00,128.32,12,3,8,1
2026-04-14 14:30:04.458,1002,3.25,128.45,14,5,8,1
```

| Column              | Description                                      |
|---------------------|--------------------------------------------------|
| `timestamp`         | Wall-clock time of the sample (local time)       |
| `from_start_ms`     | Milliseconds elapsed since monitoring started    |
| `cpu_percent`       | CPU usage percentage (across all cores)           |
| `memory_mb`         | Working set memory in megabytes                  |
| `db_total`          | Total database connections (when DB enabled)     |
| `db_active`         | Active connections                               |
| `db_idle`           | Idle connections                                 |
| `db_idle_in_tx`     | Connections idle in transaction                  |

## License

See [LICENSE](LICENSE) for details.
