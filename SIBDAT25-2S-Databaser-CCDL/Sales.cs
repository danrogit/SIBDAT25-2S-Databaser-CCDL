using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Npgsql;

namespace SIBDAT25_2S_Databaser_CCDL
{
    public class Sales
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQL"].ConnectionString;

        public int CarId { get; set; }
        public string CarModel { get; set; }
        public DateTime CarFirstRegDate { get; set; }
        public decimal CarPrice { get; set; }

        public Sales(int carId, string carModel, DateTime carFirstRegDate, decimal carPrice)
        {
            CarId = carId;
            CarModel = carModel;
            CarFirstRegDate = carFirstRegDate;
            CarPrice = carPrice;
        }

        public void Add(Sales newSale)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO sales (car_id, car_model, car_first_reg_date, car_price) VALUES (@carId, @carModel, @carFirstRegDate, @carPrice)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("carId", newSale.CarId);
                    cmd.Parameters.AddWithValue("carModel", newSale.CarModel);
                    cmd.Parameters.AddWithValue("carFirstRegDate", newSale.CarFirstRegDate);
                    cmd.Parameters.AddWithValue("carPrice", newSale.CarPrice);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Sales> GetAll()
        {
            var salesList = new List<Sales>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT car_id, car_model, car_first_reg_date, car_price FROM sales";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            salesList.Add(new Sales(reader.GetInt32(0), reader.GetString(1), reader.GetDateTime(2), reader.GetDecimal(3)));
                        }
                    }
                }
            }
            return salesList;
        }

        public void Update(Sales updatedSale)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE sales SET car_model = @carModel, car_first_reg_date = @carFirstRegDate, car_price = @carPrice WHERE car_id = @carId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("carId", updatedSale.CarId);
                    cmd.Parameters.AddWithValue("carModel", updatedSale.CarModel);
                    cmd.Parameters.AddWithValue("carFirstRegDate", updatedSale.CarFirstRegDate);
                    cmd.Parameters.AddWithValue("carPrice", updatedSale.CarPrice);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int carId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM sales WHERE car_id = @carId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("carId", carId);
                    cmd.ExecuteNonQuery();
                }
            }


        }
    }
}
