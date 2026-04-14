using Spectre.Console;

namespace BaSys.MetricsCounter.UI;

public static class HelpPrinter
{
    public static void Print()
    {
        var grid = new Grid()
            .AddColumn(new GridColumn().PadRight(4))
            .AddColumn();

        grid.AddRow("[green]--pid <PID>[/]", "Target process ID to monitor [bold](required)[/]");
        grid.AddRow("[green]--interval <seconds>[/]", "Sampling interval in seconds (default: [cyan]1[/])");
        grid.AddRow("[green]--db-type <pgsql|mssql>[/]", "Database type: [cyan]pgsql[/] or [cyan]mssql[/]");
        grid.AddRow("[green]--db-connection <string>[/]", "Database connection string");
        grid.AddRow("[green]--help[/]", "Show this help message");

        var panel = new Panel(grid)
            .Header("[bold blue]BaSys.MetricsCounter[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Blue);

        AnsiConsole.Write(new Rule("[bold]Process Metrics Monitor[/]").RuleStyle("blue"));
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  Monitors CPU and memory usage of a running process.");
        AnsiConsole.MarkupLine("  Optionally monitors database connection counts (PostgreSQL / MS SQL).");
        AnsiConsole.MarkupLine("  Results are displayed live and exported to CSV on exit.");
        AnsiConsole.WriteLine();

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new Rule("[bold]Usage[/]").RuleStyle("dim"));
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  [dim]$[/] BaSys.MetricsCounter [green]--pid[/] 1234");
        AnsiConsole.MarkupLine("  [dim]$[/] BaSys.MetricsCounter [green]--pid[/] 1234 [green]--interval[/] 0.5");
        AnsiConsole.MarkupLine("  [dim]$[/] BaSys.MetricsCounter [green]--pid[/] 1234 [green]--db-type[/] pgsql [green]--db-connection[/] \"Host=localhost;Database=mydb;Username=user;Password=pass\"");
        AnsiConsole.MarkupLine("  [dim]$[/] BaSys.MetricsCounter [green]--pid[/] 1234 [green]--db-type[/] mssql [green]--db-connection[/] \"Server=.;Database=mydb;Trusted_Connection=True\"");
        AnsiConsole.MarkupLine("  [dim]$[/] BaSys.MetricsCounter [green]--help[/]");
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new Rule("[bold]Output[/]").RuleStyle("dim"));
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  Press [bold]Ctrl+C[/] to stop monitoring.");
        AnsiConsole.MarkupLine("  A CSV file ([cyan]Results/metrics_<pid>_<timestamp>.csv[/]) is saved");
        AnsiConsole.MarkupLine("  in the [cyan]Results/[/] folder when monitoring stops.");
        AnsiConsole.WriteLine();
    }
}
