using System;
using System.Collections.Generic;
using System.Configuration;
using Npgsql;

namespace SIBDAT25_2S_Databaser_CCDL
{
    /// <summary>
    /// Repræsenterer et salg og indeholder CRUD-metoder
    /// til tabellen 'sales' i PostgreSQL.
    ///
    /// Forventet tabelstruktur:
    /// - sales_id             (int, PK)
    /// - car_first_reg_date   (date)
    /// - car_price            (numeric/decimal)
    /// </summary>
    public class Sales
    {
        // Connection string hentet fra app.config.
        private string connectionString =
            ConfigurationManager.ConnectionStrings["PostgreSQL"].ConnectionString;

        /// <summary>Primærnøgle for salget.</summary>
        public int SalesId { get; set; }

        /// <summary>Bilens første registreringsdato.</summary>
        public DateTime CarFirstRegDate { get; set; }

        /// <summary>Salgsprisen på bilen.</summary>
        public decimal CarPrice { get; set; }

        /// <summary>
        /// Opretter et Sales-objekt med id, registreringsdato og pris.
        /// </summary>
        public Sales(int salesId, DateTime carFirstRegDate, decimal carPrice)
        {
            SalesId = salesId;
            CarFirstRegDate = carFirstRegDate;
            CarPrice = carPrice;
        }

        /// <summary>
        /// Opretter (INSERT) en ny salgspost i databasen.
        /// </summary>
        public void Add(Sales newSale)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query =
                    "INSERT INTO sales (car_first_reg_date, car_price) " +
                    "VALUES (@carFirstRegDate, @carPrice)";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("carFirstRegDate", newSale.CarFirstRegDate);
                    cmd.Parameters.AddWithValue("carPrice", newSale.CarPrice);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Henter alle salgsposter og returnerer dem som en liste af Sales-objekter.
        /// </summary>
        public List<Sales> GetAll()
        {
            var salesList = new List<Sales>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query =
                    "SELECT sales_id, car_first_reg_date, car_price FROM sales";

                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var sales = new Sales(
                            reader.GetInt32(0),
                            reader.GetDateTime(1),
                            reader.GetDecimal(2));

                        salesList.Add(sales);
                    }
                }
            }

            return salesList;
        }

        /// <summary>
        /// Opdaterer (UPDATE) en salgspost i databasen.
        ///
        /// Bemærk: SQL'en forventer parameteren @salesCId,
        /// men der oprettes kun @salesId → matchfejl.
        /// (Ikke rettet, kun kommenteret.)
        /// </summary>
        public void Update(Sales updatedSale)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query =
                    "UPDATE sales SET car_first_reg_date = @carFirstRegDate, " +
                    "car_price = @carPrice WHERE sales_id = @salesCId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    // Parameter-navn matcher ikke WHERE-salesCId
                    cmd.Parameters.AddWithValue("salesId", updatedSale.SalesId);

                    cmd.Parameters.AddWithValue("carFirstRegDate", updatedSale.CarFirstRegDate);
                    cmd.Parameters.AddWithValue("carPrice", updatedSale.CarPrice);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Sletter (DELETE) en salgspost i databasen.
        ///
        /// Bemærk: SQL sletter efter car_id (!), men burde bruge sales_id.
        /// (Ikke rettet, kun kommenteret.)
        /// </summary>
        public void Delete(int salesId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query =
                    "DELETE FROM sales WHERE car_id = @salesCarId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    // Parameter-navnet @salesCarId findes ikke i SQL — mismatch.
                    cmd.Parameters.AddWithValue("salesId", salesId);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}