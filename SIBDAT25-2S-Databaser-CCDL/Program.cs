using FlaadesystemV1;
using Npgsql;           // .NET driver til PostgreSQL
using Spectre.Console;  // bruges til pæne tabeller, menuer og farvet output
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SIBDAT25_2S_DBString
{
    internal class Program
    {
        /// <summary>
        /// Program entry point – ansvarlig for:
        /// 1) Opsætning af DB connection string
        /// 2) Visning af velkomstskærm
        /// 3) Login-flow (admin)
        /// 4) Hovedmenu + routing til undermenuer
        /// </summary>
        static void Main(string[] args)
        {
            // Parser DB-forbindelsesinfo ud af en URI-streng
            var uri = new Uri("postgres://postgres:FSP02UXAG14HBHLiqtjKMU7R47akAG2Hk0Lsh0ySg2DBO0OfkHLkvwp8WOoXx89u@95.211.27.223:5510/postgres");

            // Bygger den egentlige connection string fra URI-delene
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port,
                Username = uri.UserInfo.Split(':')[0],      // brugernavn sidder før ':'
                Password = uri.UserInfo.Split(':')[1],      // password sidder efter ':'
                Database = uri.AbsolutePath.TrimStart('/'), // fjern det ledende '/'
                SslMode = SslMode.Disable                   // slukket til test, ville bruges i produktion
            };

            // ConnectionString, der bruges overalt i programmet til at åbne forbindelser
            string connectionString = builder.ConnectionString;

            // ASCII-logo til velkomstskærmen
            var asciiArt = @"                                                                                            
              ▄▀▄                                                                          
██████ ▄▄     ▄█▄  ▄▄▄▄  ▄▄▄▄▄  ▄▄▄▄ ▄▄ ▄▄  ▄▄▄▄ ▄▄▄▄▄▄ ▄▄▄▄▄ ▄▄   ▄▄   ▄▄ ▄▄  ▄██   ▄██▄  
██▄▄   ██    ██▀██ ██▀██ ██▄▄  ███▄▄ ▀███▀ ███▄▄   ██   ██▄▄  ██▀▄▀██   ██▄██   ██  ██  ██ 
██     ██▄▄▄ ██▀██ ████▀ ██▄▄▄ ▄▄██▀   █   ▄▄██▀   ██   ██▄▄▄ ██   ██    ▀█▀  ▄ ██ ▄ ▀██▀  
                                                                                           ";

            try
            {
                // Vis logo øverst uden ramme
                var titlePanel = new Panel(new Text(asciiArt))
                    .Border(BoxBorder.None)
                    .Padding(0, 0)
                    .Expand();
                AnsiConsole.Write(titlePanel);

                // Prøv at åbne en DB-forbindelse for at tjekke at serveren svarer
                // AnsiConsole.Status viser en spinner imens handlingen udføres
                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .Start("Opretter forbindelse til databasen...", ctx =>
                    {
                        // using sikrer at forbindelsen bliver lukket og disposed igen
                        using (var conn = new NpgsqlConnection(connectionString))
                        {
                            conn.Open(); // kaster exception hvis DB ikke kan nås
                        }
                    });

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[green]✓ Forbindelse til databasen er oprettet succesfuldt.[/]");
                AnsiConsole.WriteLine();

                // Lille info-boks med systemnavn
                var info = new Panel("Konsolbaseret administrationssystem til den smarte bilhandler.")
                    .Header("Flådesystem v1.0", Justify.Center)
                    .Expand();
                AnsiConsole.Write(info);
                AnsiConsole.WriteLine();

                // Hint til testbrugeren – ville ikke være her i et rigtigt system
                AnsiConsole.MarkupLine("[grey]Pst... admin er 'ccdl', koden er '12345678' ;)[/]");

                // Login – maks 3 forsøg inden programmet lukker
                const int maxAttempts = 3;
                bool authenticated = false; // flag, der angiver om login er lykkedes

                // Login-loop – stopper enten når authenticated = true eller når maxAttempts er nået
                for (int attempt = 1; attempt <= maxAttempts && !authenticated; attempt++)
                {
                    // Læs brugernavn fra konsollen
                    var adminUser = AnsiConsole.Ask<string>("Indtast [yellow]admin brugernavn[/]:");
                    // .Secret() skjuler det der tastes i konsollen
                    var adminPass = AnsiConsole.Prompt(new TextPrompt<string>("Indtast [yellow]admin kodeord[/]:").Secret());

                    // Status-spinner mens loginoplysninger verificeres mod databasen
                    AnsiConsole.Status()
                        .Spinner(Spinner.Known.Line)
                        .Start("Verificerer legitimationsoplysninger...", ctx =>
                        {
                            // Kald til AdminAuth som slår brugernavn+password op i DB
                            authenticated = AdminAuth.Authenticate(connectionString, adminUser, adminPass);
                        });

                    if (authenticated)
                    {
                        // Så snart login er godkendt, springes ud af login-loopet
                        AnsiConsole.MarkupLine("[green]✓ Login succesfuldt. Velkommen.[/]");
                        break; // hop ud af login-loopet
                    }

                    // Vis fejl og tæl forsøg
                    AnsiConsole.MarkupLine("[red]Brugernavn eller kodeord er forkert.[/]");
                    if (attempt < maxAttempts)
                        AnsiConsole.MarkupLine($"[grey]Forsøg {attempt}/{maxAttempts} mislykkedes. Prøv igen.[/]");
                    else
                        AnsiConsole.MarkupLine("[grey]Maksimalt antal forsøg nået.[/]");
                }

                // Afslut hvis alle loginforsøg mislykkedes
                if (!authenticated)
                {
                    AnsiConsole.MarkupLine("[red]Login mislykkedes. Program afsluttes.[/]");
                    // ExitCode kan bruges af operativsystemet / scripts til at tjekke om programmet fejlede
                    Environment.ExitCode = 1;
                    return;
                }

                // Hovedmenu – løber til brugeren vælger "Afslut"
                while (true)
                {
                    // SelectionPrompt viser en simpel menu, hvor man kan vælge med piletaster
                    var choice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Vælg en handling:")
                            .AddChoices(new[]
                            {
                                "Kunder",
                                "Biler",
                                "Udlejninger",
                                "Salg",
                                "Service Center",
                                "Afslut"
                            }));

                    // Ruter til den rigtige undermenu baseret på valget
                    switch (choice)
                    {
                        case "Kunder": CustomersMenu(connectionString); break;
                        case "Biler": CarsMenu(connectionString); break;
                        case "Udlejninger": RentalsMenu(connectionString); break;
                        case "Salg": SalesMenu(connectionString); break;
                        case "Service Center": ServiceCenterMenu(connectionString); break;
                        case "Afslut": return; // bryder ud af while-loopet og afslutter programmet
                    }
                }
            }
            catch (Exception ex)
            {
                // Generel fejlfangst – viser fejlbeskeden og sætter exit-kode til 1
                // Markup.Escape sikrer at evt. specialtegn i fejlbeskeden ikke tolkes som markup
                AnsiConsole.MarkupLine($"[red]Fejl:[/] {Markup.Escape(ex.Message)}");
                Environment.ExitCode = 1;
            }
        }

        // ===================== KUNDER =====================

        private static void CustomersMenu(string connectionString)
        {
            while (true)
            {
                ShowCustomers(connectionString);

                var action = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Kunder - vælg handling:")
                        .AddChoices(new[] { "Tilføj kunde", "Slet kunde", "Tilbage" }));

                if (action == "Tilføj kunde")
                {
                    AddCustomer(connectionString);
                }
                else if (action == "Slet kunde")
                {
                    var items = new List<string>();
                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();
                        const string sql = "SELECT customer_id, customer_name FROM customers ORDER BY customer_id";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                                items.Add($"{rdr.GetInt32(0)}: {(rdr.IsDBNull(1) ? "" : rdr.GetString(1))}");
                        }
                    }

                    if (items.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[grey]Ingen kunder at slette.[/]");
                        continue;
                    }

                    var sel = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Vælg kunde at slette:").AddChoices(items));
                    var id = int.Parse(sel.Split(':')[0]);

                    if (AnsiConsole.Confirm($"Er du sikker på du vil slette kunde {sel}?"))
                    {
                        using (var conn = new NpgsqlConnection(connectionString))
                        {
                            conn.Open();
                            const string del = "DELETE FROM customers WHERE customer_id = @id";
                            using (var cmd = new NpgsqlCommand(del, conn))
                            {
                                cmd.Parameters.AddWithValue("id", id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        AnsiConsole.MarkupLine("[green]Kunde slettet.[/]");
                    }
                }
                else
                {
                    return;
                }
            }
        }

        // ===================== BILER =====================

        private static void CarsMenu(string connectionString)
        {
            while (true)
            {
                ShowCars(connectionString);

                var action = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Biler - vælg handling:")
                        .AddChoices(new[] { "Tilføj bil", "Slet bil", "Tilbage" }));

                if (action == "Tilføj bil")
                {
                    AddCar(connectionString);
                }
                else if (action == "Slet bil")
                {
                    var items = new List<string>();
                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();
                        const string sql = "SELECT \"carId\", \"carModel\" FROM cars ORDER BY \"carId\"";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                                items.Add($"{rdr.GetInt32(0)}: {(rdr.IsDBNull(1) ? "" : rdr.GetString(1))}");
                        }
                    }

                    if (items.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[grey]Ingen biler at slette.[/]");
                        continue;
                    }

                    var sel = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Vælg bil at slette:").AddChoices(items));
                    var id = int.Parse(sel.Split(':')[0]);

                    if (AnsiConsole.Confirm($"Er du sikker på du vil slette bil {sel}?"))
                    {
                        using (var conn = new NpgsqlConnection(connectionString))
                        {
                            conn.Open();
                            const string del = "DELETE FROM cars WHERE \"carId\" = @id";
                            using (var cmd = new NpgsqlCommand(del, conn))
                            {
                                cmd.Parameters.AddWithValue("id", id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        AnsiConsole.MarkupLine("[green]Bil slettet.[/]");
                    }
                }
                else
                {
                    return;
                }
            }
        }

        // ===================== UDLEJNINGER =====================

        private static void RentalsMenu(string connectionString)
        {
            while (true)
            {
                ShowRentals(connectionString);

                var action = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Udlejninger - vælg handling:")
                        .AddChoices(new[] { "Opret udlejning", "Slet udlejning", "Tilbage" }));

                if (action == "Opret udlejning")
                {
                    CreateRental(connectionString);
                }
                else if (action == "Slet udlejning")
                {
                    var items = new List<string>();
                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();
                        const string sql = "SELECT rental_id, rental_car_id FROM rentals ORDER BY rental_id";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                                items.Add($"{rdr.GetInt32(0)}: Car={rdr.GetInt32(1)}");
                        }
                    }

                    if (items.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[grey]Ingen udlejninger at slette.[/]");
                        continue;
                    }

                    var sel = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Vælg udlejning at slette:").AddChoices(items));
                    var id = int.Parse(sel.Split(':')[0]);

                    if (AnsiConsole.Confirm($"Er du sikker på du vil slette udlejning {sel}?"))
                    {
                        using (var conn = new NpgsqlConnection(connectionString))
                        {
                            conn.Open();
                            const string del = "DELETE FROM rentals WHERE rental_id = @id";
                            using (var cmd = new NpgsqlCommand(del, conn))
                            {
                                cmd.Parameters.AddWithValue("id", id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        AnsiConsole.MarkupLine("[green]Udlejning slettet.[/]");
                    }
                }
                else
                {
                    return;
                }
            }
        }

        // ===================== SALG =====================

        private static void SalesMenu(string connectionString)
        {
            while (true)
            {
                ShowSales(connectionString);

                var action = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Salg - vælg handling:")
                        .AddChoices(new[] { "Slet salg", "Tilbage" }));

                if (action == "Slet salg")
                {
                    var items = new List<string>();
                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();
                        const string sql = "SELECT sales_id, car_price FROM sales ORDER BY sales_id";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                                items.Add($"{rdr.GetInt32(0)}: Price={rdr.GetDecimal(1)}");
                        }
                    }

                    if (items.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[grey]Ingen salg at slette.[/]");
                        continue;
                    }

                    var sel = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Vælg salg at slette:").AddChoices(items));
                    var id = int.Parse(sel.Split(':')[0]);

                    if (AnsiConsole.Confirm($"Er du sikker på du vil slette salg {sel}?"))
                    {
                        using (var conn = new NpgsqlConnection(connectionString))
                        {
                            conn.Open();
                            const string del = "DELETE FROM sales WHERE sales_id = @id";
                            using (var cmd = new NpgsqlCommand(del, conn))
                            {
                                cmd.Parameters.AddWithValue("id", id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        AnsiConsole.MarkupLine("[green]Salg slettet.[/]");
                    }
                }
                else
                {
                    return;
                }
            }
        }

        // ===================== SERVICE CENTER =====================

        private static void ServiceCenterMenu(string connectionString)
        {
            while (true)
            {
                ShowServiceCenter(connectionString);

                var action = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Service Center - vælg handling:")
                        .AddChoices(new[] { "Slet service", "Tilbage" }));

                if (action == "Slet service")
                {
                    var items = new List<(string Key, string Label)>();
                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();

                        bool hasServiceId;
                        using (var chk = new NpgsqlCommand(
                            "SELECT 1 FROM information_schema.columns WHERE table_name = 'service_center' AND column_name = 'service_id' LIMIT 1", conn))
                        {
                            hasServiceId = chk.ExecuteScalar() != null;
                        }

                        if (hasServiceId)
                        {
                            const string sql = "SELECT service_id, invoice_id FROM service_center ORDER BY service_id";
                            using (var cmd = new NpgsqlCommand(sql, conn))
                            using (var rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    if (rdr.IsDBNull(0)) continue;
                                    var svcId = rdr.GetInt32(0).ToString();
                                    var invoice = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                                    items.Add((svcId, $"{svcId}: {invoice}"));
                                }
                            }
                        }
                        else
                        {
                            const string sql = "SELECT invoice_id, submitted_date FROM service_center ORDER BY invoice_id";
                            using (var cmd = new NpgsqlCommand(sql, conn))
                            using (var rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    var invoice = rdr.IsDBNull(0) ? "" : rdr.GetString(0);
                                    var submitted = rdr.IsDBNull(1) ? "" : rdr.GetDateTime(1).ToString("yyyy-MM-dd");
                                    items.Add((invoice, $"{invoice} ({submitted})"));
                                }
                            }
                        }
                    }

                    if (items.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[grey]Ingen service-poster fundet.[/]");
                        continue;
                    }

                    var selLabel = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Vælg service at slette:").AddChoices(items.ConvertAll(i => i.Label)));
                    var selected = items.Find(i => i.Label == selLabel);

                    if (AnsiConsole.Confirm($"Er du sikker på du vil slette service {selLabel}?"))
                    {
                        using (var conn = new NpgsqlConnection(connectionString))
                        {
                            conn.Open();

                            int numericId;
                            if (int.TryParse(selected.Key, out numericId))
                            {
                                const string del = "DELETE FROM service_center WHERE service_id = @id";
                                using (var cmd = new NpgsqlCommand(del, conn))
                                {
                                    cmd.Parameters.AddWithValue("id", numericId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                const string del = "DELETE FROM service_center WHERE invoice_id = @invoice";
                                using (var cmd = new NpgsqlCommand(del, conn))
                                {
                                    cmd.Parameters.AddWithValue("invoice", selected.Key);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        AnsiConsole.MarkupLine("[green]Service slettet.[/]");
                    }
                }
                else
                {
                    return;
                }
            }
        }

        // ===================== SHOW / ADD HJÆLPEMETODER =====================

        private static void ShowCustomers(string connectionString)
        {
            var table = new Table().Expand();
            table.AddColumn("ID");
            table.AddColumn("Name");
            table.AddColumn("Email");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string sql = "SELECT customer_id, customer_name, customer_email FROM customers ORDER BY customer_id";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        table.AddRow(
                            rdr.GetInt32(0).ToString(),
                            rdr.IsDBNull(1) ? "" : rdr.GetString(1),
                            rdr.IsDBNull(2) ? "" : rdr.GetString(2));
                    }
                }
            }

            AnsiConsole.Write(table);
        }

        private static void AddCustomer(string connectionString)
        {
            var name = AnsiConsole.Ask<string>("Indtast [yellow]kundens fornavn og efternavn[/]:");
            var email = AnsiConsole.Ask<string>("Indtast [yellow]kunde email[/]:");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string sql = "INSERT INTO customers (customer_name, customer_email) VALUES (@name, @email)";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("name", NpgsqlTypes.NpgsqlDbType.Text, name);
                    cmd.Parameters.AddWithValue("email", NpgsqlTypes.NpgsqlDbType.Text, email);
                    cmd.ExecuteNonQuery();
                }
            }

            AnsiConsole.MarkupLine("[green]Kunde tilføjet.[/]");
        }

        private static void ShowCars(string connectionString)
        {
            var table = new Table().Expand();
            table.AddColumn("ID");
            table.AddColumn("Model");
            table.AddColumn("Year");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string sql = "SELECT \"carId\", \"carModel\", \"carYear\" FROM cars ORDER BY \"carId\"";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        table.AddRow(
                            rdr.GetInt32(0).ToString(),
                            rdr.IsDBNull(1) ? "" : rdr.GetString(1),
                            rdr.IsDBNull(2) ? "" : rdr.GetInt32(2).ToString());
                    }
                }
            }

            AnsiConsole.Write(table);
        }

        private static void AddCar(string connectionString)
        {
            var model = AnsiConsole.Ask<string>("Indtast [yellow]model[/]:");
            var year = AnsiConsole.Ask<int>("Indtast [yellow]årgang[/]:");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string sql = "INSERT INTO cars (\"carModel\", \"carYear\") VALUES (@model, @year)";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("model", NpgsqlTypes.NpgsqlDbType.Text, model);
                    cmd.Parameters.AddWithValue("year", NpgsqlTypes.NpgsqlDbType.Integer, year);
                    cmd.ExecuteNonQuery();
                }
            }

            AnsiConsole.MarkupLine("[green]Bil tilføjet.[/]");
        }

        private static void ShowRentals(string connectionString)
        {
            var table = new Table().Expand();
            table.AddColumn("ID");
            table.AddColumn("Car ID");
            table.AddColumn("From");
            table.AddColumn("To");
            table.AddColumn("Daily Price");
            table.AddColumn("Customer ID");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string sql = "SELECT rental_id, rental_car_id, rented_from_date, rented_to_date, daily_price, customer_id FROM rentals ORDER BY rental_id";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        table.AddRow(
                            rdr.GetInt32(0).ToString(),
                            rdr.IsDBNull(1) ? "" : rdr.GetInt32(1).ToString(),
                            rdr.IsDBNull(2) ? "" : rdr.GetDateTime(2).ToString("yyyy-MM-dd"),
                            rdr.IsDBNull(3) ? "" : rdr.GetDateTime(3).ToString("yyyy-MM-dd"),
                            rdr.IsDBNull(4) ? "" : rdr.GetDecimal(4).ToString(CultureInfo.InvariantCulture),
                            rdr.IsDBNull(5) ? "" : rdr.GetInt32(5).ToString()
                        );
                    }
                }
            }

            AnsiConsole.Write(table);
        }

        private static void CreateRental(string connectionString)
        {
            var cars = new List<(int Id, string Label)>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string carSql = "SELECT \"carId\", \"carModel\", \"carYear\" FROM cars ORDER BY \"carId\"";
                using (var cmd = new NpgsqlCommand(carSql, conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var id = rdr.GetInt32(0);
                        var model = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                        var year = rdr.IsDBNull(2) ? 0 : rdr.GetInt32(2);
                        cars.Add((id, $"{id}: {model} ({year})"));
                    }
                }
            }

            if (cars.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Ingen biler fundet. Tilføj en bil først.[/]");
                return;
            }

            var carChoice = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Vælg bil:").AddChoices(cars.ConvertAll(c => c.Label)));
            var selectedCar = cars.Find(c => c.Label == carChoice).Id;

            var customers = new List<(int Id, string Label)>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string custSql = "SELECT customer_id, customer_name FROM customers ORDER BY customer_id";
                using (var cmd = new NpgsqlCommand(custSql, conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var id = rdr.GetInt32(0);
                        var name = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                        customers.Add((id, $"{id}: {name}"));
                    }
                }
            }

            if (customers.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Ingen kunder fundet. Tilføj en kunde først.[/]");
                return;
            }

            var custChoice = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Vælg kunde for udlejning:").AddChoices(customers.ConvertAll(c => c.Label)));
            var selectedCustomer = customers.Find(c => c.Label == custChoice).Id;

            var from = AnsiConsole.Ask<DateTime>("Indtast [yellow]startdato[/] (yyyy-MM-dd):", DateTime.Now.Date);
            var to = AnsiConsole.Ask<DateTime>("Indtast [yellow]slutdato[/] (yyyy-MM-dd):", DateTime.Now.Date.AddDays(1));
            var dailyPrice = AnsiConsole.Ask<decimal>("Indtast [yellow]pris per dag[/]:");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string insert = "INSERT INTO rentals (rental_car_id, rented_from_date, rented_to_date, daily_price, customer_id) " +
                                      "VALUES (@carId, @from, @to, @dailyPrice, @customerId)";
                using (var cmd = new NpgsqlCommand(insert, conn))
                {
                    cmd.Parameters.AddWithValue("carId", NpgsqlTypes.NpgsqlDbType.Integer, selectedCar);
                    cmd.Parameters.AddWithValue("from", NpgsqlTypes.NpgsqlDbType.Date, from);
                    cmd.Parameters.AddWithValue("to", NpgsqlTypes.NpgsqlDbType.Date, to);
                    cmd.Parameters.AddWithValue("dailyPrice", NpgsqlTypes.NpgsqlDbType.Numeric, dailyPrice);
                    cmd.Parameters.AddWithValue("customerId", NpgsqlTypes.NpgsqlDbType.Integer, selectedCustomer);
                    cmd.ExecuteNonQuery();
                }
            }

            AnsiConsole.MarkupLine("[green]Udlejning oprettet.[/]");
        }

        private static void ShowSales(string connectionString)
        {
            var table = new Table().Expand();
            table.AddColumn("ID");
            table.AddColumn("First reg date");
            table.AddColumn("Price");
            table.AddColumn("Customer ID");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string sql = "SELECT sales_id, car_first_reg_date, car_price, customer_id FROM sales ORDER BY sales_id";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        table.AddRow(
                            rdr.GetInt32(0).ToString(),
                            rdr.IsDBNull(1) ? "" : rdr.GetDateTime(1).ToString("yyyy-MM-dd"),
                            rdr.IsDBNull(2) ? "" : rdr.GetDecimal(2).ToString(CultureInfo.InvariantCulture),
                            rdr.IsDBNull(3) ? "" : rdr.GetInt32(3).ToString());
                    }
                }
            }

            AnsiConsole.Write(table);
        }

        private static void ShowServiceCenter(string connectionString)
        {
            var table = new Table().Expand();
            table.AddColumn("Submitted");
            table.AddColumn("Completion");
            table.AddColumn("Invoice ID");
            table.AddColumn("Invoice Price");
            table.AddColumn("Intern");
            table.AddColumn("Ekstern");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string sql = "SELECT submitted_date, completion_date, invoice_id, invoice_price, intern, ekstern FROM service_center ORDER BY invoice_id";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        table.AddRow(
                            rdr.IsDBNull(0) ? "" : rdr.GetDateTime(0).ToString("yyyy-MM-dd"),
                            rdr.IsDBNull(1) ? "" : rdr.GetDateTime(1).ToString("yyyy-MM-dd"),
                            rdr.IsDBNull(2) ? "" : rdr.GetString(2),
                            rdr.IsDBNull(3) ? "" : rdr.GetDecimal(3).ToString(CultureInfo.InvariantCulture),
                            rdr.IsDBNull(4) ? "" : rdr.GetString(4),
                            rdr.IsDBNull(5) ? "" : rdr.GetString(5)
                        );
                    }
                }
            }

            AnsiConsole.Write(table);
        }
    }
}