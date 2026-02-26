using System;
using System.Collections.Generic;
using System.Configuration;
using Npgsql;

namespace SIBDAT25_2S_Databaser_CCDL
{
    /// <summary>
    /// Repræsenterer en bil og indeholder enkle CRUD-operationer
    /// (Create, Read, Update, Delete) mod tabellen 'cars' i databasen.
    /// 
    /// Klassen er model + "repository" i én og samme klasse,
    /// hvilket er fint til et mindre studieprojekt.
    /// </summary>
    public class Cars
    {
        // Connection string hentes fra app.config/web.config under <connectionStrings>
        // med navnet "PostgreSQL".
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQL"].ConnectionString;

        /// <summary>
        /// Primærnøgle / id for bilen i databasen.
        /// </summary>
        public int CarId { get; set; }

        /// <summary>
        /// Bilens model (fx "Toyota Yaris").
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Bilens årgang.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Standardkonstruktør, der opretter et Cars-objekt
        /// med id, model og årgang.
        /// </summary>
        public Cars(int carId, string model, int year)
        {
            CarId = carId;
            Model = model;
            Year = year;
        }

        /// <summary>
        /// Opretter (INSERT) en ny bil i databasen.
        /// Bruger objektet <paramref name="newCar"/> til at få model og år.
        /// 
        /// Forventet tabelstruktur:
        /// - cars(model, year, ...)
        /// </summary>
        public void Add(Cars newCar)
        {
            // using sikrer at forbindelsen bliver lukket og disposed,
            // selv hvis der opstår en exception.
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query = "INSERT INTO cars (model, year) VALUES (@model, @year)";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    // Parameteriseret query for at undgå SQL injection.
                    cmd.Parameters.AddWithValue("model", newCar.Model);
                    cmd.Parameters.AddWithValue("year", newCar.Year);

                    // Vi forventer ingen returværdier, så ExecuteNonQuery bruges.
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Henter alle biler fra tabellen 'cars' og returnerer dem
        /// som en liste af Cars-objekter.
        /// 
        /// Forventet kolonner i databasen:
        /// - id (int)
        /// - model (text/varchar)
        /// - year (int)
        /// </summary>
        public List<Cars> GetAll()
        {
            var carsList = new List<Cars>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query = "SELECT id, model, year FROM cars";

                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    // Læs alle rækker og opret et Cars-objekt for hver række.
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var model = reader.GetString(1);
                        var year = reader.GetInt32(2);

                        carsList.Add(new Cars(id, model, year));
                    }
                }
            }

            return carsList;
        }

        /// <summary>
        /// Opdaterer (UPDATE) en eksisterende bil i databasen
        /// baseret på bilens id.
        /// 
        /// Kun felterne 'model' og 'year' opdateres her.
        /// </summary>
        public void Update(Cars updatedCar)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query = "UPDATE cars SET model = @model, year = @year WHERE id = @id";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("model", updatedCar.Model);
                    cmd.Parameters.AddWithValue("year", updatedCar.Year);
                    cmd.Parameters.AddWithValue("id", updatedCar.CarId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Sletter (DELETE) en bil med det angivne id fra databasen.
        ///
        /// Bemærk:
        /// - Metoden modtager parameteren <paramref name="carId"/>,
        ///   men bruger feltet CarId i parameteren til SQL.
        ///   Det betyder, at det er objektets nuværende CarId, der bruges.
        ///   I mange designs ville man i stedet bruge 'carId' direkte.
        /// </summary>
        public void Delete(int carId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query = "DELETE FROM cars WHERE id = @id";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    // Her bruges CarId-feltet på objektet, ikke metodes parameter.
                    cmd.Parameters.AddWithValue("id", CarId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Giver en pæn tekstrepræsentation, fx:
        /// "1: Toyota Yaris (2018)".
        /// Brugbar i lister, debugging osv.
        /// </summary>
        public override string ToString()
        {
            return $"{CarId}: {Model} ({Year})";
        }
    }
}