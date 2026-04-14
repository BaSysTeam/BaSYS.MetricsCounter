using BaSys.MetricsCounter.Models;
using Spectre.Console;

namespace BaSys.MetricsCounter.UI;

public static class ConsoleRenderer
{
    private const int MaxVisibleRows = 30;

    public static Table CreateTable()
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold blue]Process Metrics Monitor[/]")
            .AddColumn(new TableColumn("[bold]#[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Timestamp[/]"))
            .AddColumn(new TableColumn("[bold]From Start (ms)[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]CPU (%)[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Memory (MB)[/]").RightAligned());

        return table;
    }

    public static void UpdateTable(Table table, IReadOnlyList<MetricsRecord> records)
    {
        table.Rows.Clear();

        int totalCount = records.Count;
        int skip = Math.Max(0, totalCount - MaxVisibleRows);

        if (skip > 0)
        {
            table.Caption = new TableTitle(
                $"[dim]Showing last {MaxVisibleRows} of {totalCount} samples[/]");
        }

        for (int i = skip; i < totalCount; i++)
        {
            var r = records[i];
            int rowNum = i + 1;

            table.AddRow(
                new Markup($"[dim]{rowNum}[/]"),
                new Markup(r.Timestamp.ToString("HH:mm:ss.fff")),
                new Markup($"{r.FromStartMs}"),
                new Markup(ColorizeCpu(r.CpuPercent)),
                new Markup($"[cyan]{r.MemoryMB:F2}[/]"));
        }

        table.Title = new TableTitle(
            $"[bold blue]Process Metrics Monitor[/] [dim]({totalCount} samples)[/]");
    }

    private static string ColorizeCpu(double cpuPercent)
    {
        return cpuPercent switch
        {
            >= 80 => $"[bold red]{cpuPercent:F2}[/]",
            >= 50 => $"[bold yellow]{cpuPercent:F2}[/]",
            _ => $"[green]{cpuPercent:F2}[/]"
        };
    }

    public static void PrintHeader(int pid, double intervalSeconds, string processName)
    {
        var panel = new Panel(
                new Rows(
                    new Markup($"[bold]PID:[/]       [cyan]{pid}[/]"),
                    new Markup($"[bold]Process:[/]   [cyan]{processName}[/]"),
                    new Markup($"[bold]Interval:[/]  [cyan]{intervalSeconds}s[/]"),
                    new Markup($"[bold]Started:[/]   [cyan]{DateTime.Now:yyyy-MM-dd HH:mm:ss}[/]"),
                    new Markup(""),
                    new Markup("[dim]Press [bold]Ctrl+C[/] to stop monitoring and export CSV[/]")))
            .Header("[bold green]Monitoring Started[/]")
            .Border(BoxBorder.Double)
            .BorderColor(Color.Green);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public static void PrintSummary(int pid, int sampleCount, TimeSpan duration, string csvPath)
    {
        AnsiConsole.WriteLine();

        var panel = new Panel(
                new Rows(
                    new Markup($"[bold]PID:[/]       [cyan]{pid}[/]"),
                    new Markup($"[bold]Samples:[/]   [cyan]{sampleCount}[/]"),
                    new Markup($"[bold]Duration:[/]  [cyan]{duration:hh\\:mm\\:ss\\.ff}[/]"),
                    new Markup($"[bold]CSV File:[/]  [cyan]{csvPath}[/]")))
            .Header("[bold yellow]Monitoring Complete[/]")
            .Border(BoxBorder.Double)
            .BorderColor(Color.Yellow);

        AnsiConsole.Write(panel);
    }

    public static void PrintError(string message)
    {
        AnsiConsole.MarkupLine($"[bold red]Error:[/] {message}");
    }

    public static void PrintProcessExited()
    {
        AnsiConsole.MarkupLine("\n[bold yellow]Target process has exited.[/] Stopping monitoring...");
    }
}
