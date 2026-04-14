using System.Diagnostics;
using System.Globalization;
using BaSys.MetricsCounter.Services;
using BaSys.MetricsCounter.UI;
using Spectre.Console;

namespace BaSys.MetricsCounter;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || HasFlag(args, "--help") || HasFlag(args, "-h"))
        {
            HelpPrinter.Print();
            return 0;
        }

        if (!TryParseArgs(args, out int pid, out double interval, out string? error))
        {
            ConsoleRenderer.PrintError(error!);
            AnsiConsole.MarkupLine("[dim]Use [bold]--help[/] for usage information.[/]");
            return 1;
        }

        Process process;
        try
        {
            process = Process.GetProcessById(pid);
        }
        catch (ArgumentException)
        {
            ConsoleRenderer.PrintError($"Process with PID [bold]{pid}[/] not found.");
            return 1;
        }
        catch (Exception ex)
        {
            ConsoleRenderer.PrintError($"Cannot access process [bold]{pid}[/]: {ex.Message}");
            return 1;
        }

        string processName = process.ProcessName;
        process.Dispose();

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        using var monitor = new ProcessMonitor(pid, interval);

        try
        {
            monitor.Attach();
        }
        catch (Exception ex)
        {
            ConsoleRenderer.PrintError($"Failed to attach to process [bold]{pid}[/]: {ex.Message}");
            return 1;
        }

        ConsoleRenderer.PrintHeader(pid, interval, processName);

        var table = ConsoleRenderer.CreateTable();
        int intervalMs = (int)(interval * 1000);
        bool processExited = false;

        try
        {
            await AnsiConsole.Live(table)
                .AutoClear(false)
                .Overflow(VerticalOverflow.Ellipsis)
                .StartAsync(async ctx =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        var record = monitor.Sample();

                        if (record is null)
                        {
                            processExited = true;
                            break;
                        }

                        ConsoleRenderer.UpdateTable(table, monitor.Records);
                        ctx.Refresh();

                        try
                        {
                            await Task.Delay(intervalMs, cts.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                    }
                });
        }
        catch (TaskCanceledException)
        {
            // Expected on Ctrl+C
        }

        if (processExited)
        {
            ConsoleRenderer.PrintProcessExited();
        }

        if (monitor.Records.Count > 0)
        {
            var csvPath = CsvExporter.Export(monitor.Records, pid);
            var duration = TimeSpan.FromSeconds(
                monitor.Records[^1].FromStartSeconds);

            ConsoleRenderer.PrintSummary(pid, monitor.Records.Count, duration, csvPath);
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]No samples were collected.[/]");
        }

        return 0;
    }

    private static bool TryParseArgs(string[] args, out int pid, out double interval, out string? error)
    {
        pid = 0;
        interval = 1.0;
        error = null;
        bool pidFound = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--pid":
                    if (i + 1 >= args.Length)
                    {
                        error = "Missing value for [bold]--pid[/].";
                        return false;
                    }
                    if (!int.TryParse(args[++i], out pid) || pid <= 0)
                    {
                        error = $"Invalid PID: [bold]{args[i]}[/]. Must be a positive integer.";
                        return false;
                    }
                    pidFound = true;
                    break;

                case "--interval":
                    if (i + 1 >= args.Length)
                    {
                        error = "Missing value for [bold]--interval[/].";
                        return false;
                    }
                    if (!double.TryParse(args[++i], NumberStyles.Float, CultureInfo.InvariantCulture, out interval) || interval <= 0)
                    {
                        error = $"Invalid interval: [bold]{args[i]}[/]. Must be a positive number.";
                        return false;
                    }
                    break;

                case "--help":
                case "-h":
                    break;

                default:
                    error = $"Unknown argument: [bold]{args[i]}[/].";
                    return false;
            }
        }

        if (!pidFound)
        {
            error = "Missing required argument [bold]--pid[/].";
            return false;
        }

        return true;
    }

    private static bool HasFlag(string[] args, string flag)
    {
        foreach (var arg in args)
        {
            if (string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
