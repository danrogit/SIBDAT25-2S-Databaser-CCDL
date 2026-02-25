using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Npgsql;

namespace SIBDAT25_2S_Databaser_CCDL
{
    public class Customers
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQL"].ConnectionString;

        public string CostumerName { get; set; }
        public string CostumerEmail { get; set; }
        public int CostumerId { get; set; }

        public Customers(string costumerName, string costumerEmail, int costumerId)
        {          
            CostumerName = costumerName;           
            CostumerEmail = costumerEmail;
            CostumerId = costumerId;
        }

        //CRUD
        public void Add(Customers newCustomer)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO customers (costumer_name, costumer_email) VALUES (@costumerName, @costumerEmail)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("costumerName", newCustomer.CostumerName);
                    cmd.Parameters.AddWithValue("costumerEmail", newCustomer.CostumerEmail);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public List<Customers> GetAll()
        {
            var customersList = new List<Customers>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT costumer_name, costumer_email, costumer_id FROM customers";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customersList.Add(new Customers(reader.GetString(0), reader.GetString(1), reader.GetInt32(2)));
                        }
                    }
                }
            }
            return customersList;
        }

        public void Update(Customers updatedCustomer)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE customers SET costumer_name = @costumerName, costumer_email = @costumerEmail WHERE costumer_id = @costumerId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("costumerName", updatedCustomer.CostumerName);
                    cmd.Parameters.AddWithValue("costumerEmail", updatedCustomer.CostumerEmail);
                    cmd.Parameters.AddWithValue("costumerId", updatedCustomer.CostumerId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

         public void Delete(int costumerId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM customers WHERE costumer_id = @costumerId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("costumerId", costumerId);
                    cmd.ExecuteNonQuery();
                }
            }
        }     

    }
}
