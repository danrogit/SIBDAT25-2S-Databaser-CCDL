using System;
using Npgsql;
using Spectre.Console;

namespace FlaadesystemV1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Programmet tester kun om der kan oprettes forbindelse til databasen for "Flådesystem v1.0".
            // NOTE: I produktion bør du hente URI/password fra en sikker kilde (fx miljøvariabel).

            var uri = new Uri("postgres://postgres:FSP02UXAG14HBHLiqtjKMU7R47akAG2Hk0Lsh0ySg2DBO0OfkHLkvwp8WOoXx89u@95.211.27.223:5510/postgres");

            // Byg en Npgsql-tilslutningsstreng ud fra URI'en
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port,
                Username = uri.UserInfo.Split(':')[0],
                Password = uri.UserInfo.Split(':')[1],
                Database = uri.AbsolutePath.TrimStart('/'),
                SslMode = SslMode.Disable
            };

            string connectionString = builder.ConnectionString;

            // The exact ASCII art the user requested:
            var asciiArt = @"                                                                                           
              ▄▀▄                                                                          
██████ ▄▄     ▄█▄  ▄▄▄▄  ▄▄▄▄▄  ▄▄▄▄ ▄▄ ▄▄  ▄▄▄▄ ▄▄▄▄▄▄ ▄▄▄▄▄ ▄▄   ▄▄   ▄▄ ▄▄  ▄██   ▄██▄  
██▄▄   ██    ██▀██ ██▀██ ██▄▄  ███▄▄ ▀███▀ ███▄▄   ██   ██▄▄  ██▀▄▀██   ██▄██   ██  ██  ██ 
██     ██▄▄▄ ██▀██ ████▀ ██▄▄▄ ▄▄██▀   █   ▄▄██▀   ██   ██▄▄▄ ██   ██    ▀█▀  ▄ ██ ▄ ▀██▀  
                                                                                            ";

            try
            {
                // Print exact ASCII art inside a panel (no border) so spacing is preserved.
                var titlePanel = new Panel(new Text(asciiArt))
                    .Border(BoxBorder.None)
                    .Padding(0, 0)
                    .Expand();
                AnsiConsole.Write(titlePanel);

                // Status spinner while opening connection
                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .Start("Opretter forbindelse til databasen...", ctx =>
                    {
                        using (var conn = new NpgsqlConnection(connectionString))
                        {
                            conn.Open();
                        }
                    });

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[green]✓ Forbindelse til databasen er oprettet succesfuldt.[/]");
                AnsiConsole.WriteLine();

                // Info panel with proper Danish product name
                var info = new Panel("Konsolbaseret administrationssystem til den smarte bilhandler.")
                    .Header("Flådesystem v1.0", Justify.Center)
                    .Expand();
                AnsiConsole.Write(info);

                AnsiConsole.WriteLine();

                // Interaktive prompts (demonstration)
                var user = AnsiConsole.Ask<string>("Indtast [yellow]brugernavn[/]:");
                var pass = AnsiConsole.Prompt(
                    new TextPrompt<string>("Indtast [yellow]kodeord[/]:").Secret());

                AnsiConsole.MarkupLine($"Indtastet bruger: [bold]{Markup.Escape(user)}[/]  (kodeord skjult)");

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[grey]Tryk Enter for at afslutte...[/]");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Forbindelse fejlede:[/] {Markup.Escape(ex.Message)}");
                Environment.ExitCode = 1;
            }
        }
    }
}
