using System;
using System.Collections.Generic;
using System.Configuration;
using Npgsql;

namespace SIBDAT25_2S_Databaser_CCDL
{
    /// <summary>
    /// Repræsenterer en kunde og indeholder CRUD-metoder
    /// (Create, Read, Update, Delete) til tabellen 'customers'
    /// i en PostgreSQL-database.
    ///
    /// Forventet tabelstruktur:
    /// - costumer_id     (int, primærnøgle)
    /// - costumer_name   (text/varchar)
    /// - costumer_email  (text/varchar)
    /// </summary>
    public class Customers
    {
        // Connection string hentes fra app.config / web.config
        private string connectionString = ConfigurationManager
            .ConnectionStrings["PostgreSQL"].ConnectionString;

        /// <summary>
        /// Kundens navn.
        /// </summary>
        public string CostumerName { get; set; }

        /// <summary>
        /// Kundens e-mailadresse.
        /// </summary>
        public string CostumerEmail { get; set; }

        /// <summary>
        /// Kundens ID (primærnøgle i databasen).
        /// </summary>
        public int CostumerId { get; set; }

        /// <summary>
        /// Opretter et Customers-objekt med navn, email og id.
        /// </summary>
        public Customers(string costumerName, string costumerEmail, int costumerId)
        {
            CostumerName = costumerName;
            CostumerEmail = costumerEmail;
            CostumerId = costumerId;
        }

        /// <summary>
        /// Opretter (INSERT) en ny kunde i databasen.
        /// </summary>
        public void Add(Customers newCustomer)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query =
                    "INSERT INTO customers (costumer_name, costumer_email) " +
                    "VALUES (@costumerName, @costumerEmail)";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("costumerName", newCustomer.CostumerName);
                    cmd.Parameters.AddWithValue("costumerEmail", newCustomer.CostumerEmail);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Henter alle kunder fra databasen og returnerer dem
        /// som en liste af Customers-objekter.
        /// </summary>
        public List<Customers> GetAll()
        {
            var customersList = new List<Customers>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query =
                    "SELECT costumer_name, costumer_email, costumer_id FROM customers";

                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetString(0);
                        var email = reader.GetString(1);
                        var id = reader.GetInt32(2);

                        customersList.Add(new Customers(name, email, id));
                    }
                }
            }

            return customersList;
        }

        /// <summary>
        /// Opdaterer (UPDATE) kundens navn og email for et bestemt id.
        /// </summary>
        public void Update(Customers updatedCustomer)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query =
                    "UPDATE customers " +
                    "SET costumer_name = @costumerName, costumer_email = @costumerEmail " +
                    "WHERE costumer_id = @costumerId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("costumerName", updatedCustomer.CostumerName);
                    cmd.Parameters.AddWithValue("costumerEmail", updatedCustomer.CostumerEmail);
                    cmd.Parameters.AddWithValue("costumerId", updatedCustomer.CostumerId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Sletter (DELETE) en kunde ud fra costumer_id.
        /// </summary>
        public void Delete(int costumerId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query =
                    "DELETE FROM customers WHERE costumer_id = @costumerId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("costumerId", costumerId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}