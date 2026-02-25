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

        public int SalesId { get; set; }
        public DateTime CarFirstRegDate { get; set; }
        public decimal CarPrice { get; set; }

        public Sales(int salesId, DateTime carFirstRegDate, decimal carPrice)
        {
            SalesId = salesId;           
            CarFirstRegDate = carFirstRegDate;
            CarPrice = carPrice;
        }

        //CRUD
        public void Add(Sales newSale)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO sales (car_first_reg_date, car_price) VALUES (@carFirstRegDate, @carPrice)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {               
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
                string query = "SELECT sales_id, car_first_reg_date, car_price FROM sales";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            salesList.Add(new Sales(reader.GetInt32(0), reader.GetDateTime(1), reader.GetDecimal(2)));
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
                string query = "UPDATE sales SET car_first_reg_date = @carFirstRegDate, car_price = @carPrice WHERE sales_id = @salesCId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("salesId", updatedSale.SalesId);
                    cmd.Parameters.AddWithValue("carFirstRegDate", updatedSale.CarFirstRegDate);
                    cmd.Parameters.AddWithValue("carPrice", updatedSale.CarPrice);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int salesId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM sales WHERE car_id = @salesCarId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("salesId", salesId);
                    cmd.ExecuteNonQuery();
                }
            }


        }
    }
}
