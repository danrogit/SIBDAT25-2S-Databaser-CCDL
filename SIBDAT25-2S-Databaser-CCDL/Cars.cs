using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Npgsql;

namespace SIBDAT25_2S_Databaser_CCDL
{
    public class Cars
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQL"].ConnectionString;

        public int Id { get; set; }   
        public string Model { get; set; }
        public int Year { get; set; }
        public Cars(int id, string model, int year)
        {
            Id = id;         
            Model = model;
            Year = year;
        }

        public void Add(Cars newCar)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO cars (model, year) VALUES (@model, @year)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("model", newCar.Model);
                    cmd.Parameters.AddWithValue("year", newCar.Year);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Cars> GetAll()
        {
            var carsList = new List<Cars>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, model, year FROM cars";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            carsList.Add(new Cars(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2)));
                        }
                    }
                }
            }
            return carsList;
        }

        public void Update(Cars updatedCar)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE cars SET model = @model, year = @year WHERE id = @id";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("model", updatedCar.Model);
                    cmd.Parameters.AddWithValue("year", updatedCar.Year);
                    cmd.Parameters.AddWithValue("id", updatedCar.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM cars WHERE id = @id";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public override string ToString()
        {
            return $"{Id}: {Model} ({Year})";
        }
    }
}
