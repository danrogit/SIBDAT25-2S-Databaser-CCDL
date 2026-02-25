using System;
using System.Collections.Generic;
using System.Configuration;
using Npgsql;

namespace FlaadesystemV1
{
    public class CarSold
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQL"].ConnectionString;

        public int SoldId { get; set; }
        public int SoldCarId { get; set; } // FK -> cars.carId

        public CarSold(int soldId, int soldCarId)
        {
            SoldId = soldId;
            SoldCarId = soldCarId;
        }

        // Create
        public void Add(CarSold newSold)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string query = "INSERT INTO car_sold (sold_car_id) VALUES (@soldCarId)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("soldCarId", newSold.SoldCarId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Read
        public List<CarSold> GetAll()
        {
            var list = new List<CarSold>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string query = "SELECT sold_id, sold_car_id FROM car_sold";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new CarSold(reader.GetInt32(0), reader.GetInt32(1)));
                        }
                    }
                }
            }
            return list;
        }

        // Update
        public void Update(CarSold updated)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string query = "UPDATE car_sold SET sold_car_id = @soldCarId WHERE sold_id = @soldId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("soldCarId", updated.SoldCarId);
                    cmd.Parameters.AddWithValue("soldId", updated.SoldId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Delete
        public void Delete(int soldId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string query = "DELETE FROM car_sold WHERE sold_id = @soldId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("soldId", soldId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public override string ToString()
        {
            return $"{SoldId}: CarId={SoldCarId}";
        }
    }
}