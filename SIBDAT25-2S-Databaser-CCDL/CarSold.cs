using System;
using System.Collections.Generic;
using System.Configuration;
using Npgsql;

namespace FlaadesystemV1
{
    /// <summary>
    /// Repræsenterer en registrering af en solgt bil.
    /// Klassen fungerer både som model (dataobjekt)
    /// og som "repository" med CRUD-metoder til tabellen car_sold.
    ///
    /// Forventet tabelstruktur i databasen:
    /// - sold_id       (primærnøgle, int)
    /// - sold_car_id   (foreign key til cars.carId)
    /// </summary>
    public class CarSold
    {
        // Hentes fra app.config/web.config via <connectionStrings>.
        private string connectionString = ConfigurationManager
            .ConnectionStrings["PostgreSQL"].ConnectionString;

        /// <summary>
        /// Unik id for salgsregistreringen.
        /// </summary>
        public int SoldId { get; set; }

        /// <summary>
        /// Id på bilen der er solgt (FK til cars-tabellen).
        /// </summary>
        public int SoldCarId { get; set; }

        /// <summary>
        /// Opretter et CarSold-objekt baseret på sold_id og sold_car_id.
        /// </summary>
        public CarSold(int soldId, int soldCarId)
        {
            SoldId = soldId;
            SoldCarId = soldCarId;
        }

        /// <summary>
        /// Opretter (INSERT) en ny salgsregistrering i databasen.
        /// Der forventes kun et carId, da sold_id typisk autogenereres.
        /// </summary>
        public void Add(CarSold newSold)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query =
                    "INSERT INTO car_sold (sold_car_id) VALUES (@soldCarId)";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("soldCarId", newSold.SoldCarId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Henter alle salgsregistreringer fra car_sold-tabellen.
        /// Returnerer en liste med CarSold-objekter.
        /// </summary>
        public List<CarSold> GetAll()
        {
            var list = new List<CarSold>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query =
                    "SELECT sold_id, sold_car_id FROM car_sold";

                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var carId = reader.GetInt32(1);

                        list.Add(new CarSold(id, carId));
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Opdaterer (UPDATE) en salgsregistrering.
        /// Typisk bruges denne hvis en forkert bil blev registreret som solgt.
        /// </summary>
        public void Update(CarSold updated)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query =
                    "UPDATE car_sold SET sold_car_id = @soldCarId WHERE sold_id = @soldId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("soldCarId", updated.SoldCarId);
                    cmd.Parameters.AddWithValue("soldId", updated.SoldId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Sletter (DELETE) en salgsregistrering ud fra sold_id.
        /// </summary>
        public void Delete(int soldId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query =
                    "DELETE FROM car_sold WHERE sold_id = @soldId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("soldId", soldId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Giver en enkel tekstvisning af objektet,
        /// fx "5: CarId=12".
        /// </summary>
        public override string ToString()
        {
            return $"{SoldId}: CarId={SoldCarId}";
        }
    }
}