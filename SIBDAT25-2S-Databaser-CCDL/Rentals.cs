using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Npgsql;

namespace SIBDAT25_2S_Databaser_CCDL
{
    public class Rentals
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQL"].ConnectionString;

        public int RentalId { get; set; }
        public int CarId { get; set; }
        public DateTime RentedFromDate { get; set; }
        public DateTime RentedToDate { get; set; }
        public string RentersName { get; set; }
        public decimal DailyPrice { get; set; }

        public Rentals(int rentalId, int carId, DateTime rentedFromDate, DateTime rentedToDate, string rentersName, decimal dailyPrice)
        {
            RentalId = rentalId;
            CarId = carId;
            RentedFromDate = rentedFromDate;
            RentedToDate = rentedToDate;
            RentersName = rentersName;
            DailyPrice = dailyPrice;
        }

        public void Add(Rentals newRental)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO rentals (car_id, rented_from_date, rented_to_date, renters_name, daily_price) VALUES (@carId, @rentedFromDate, @rentedToDate, @rentersName, @dailyPrice)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("carId", newRental.CarId);
                    cmd.Parameters.AddWithValue("rentedFromDate", newRental.RentedFromDate);
                    cmd.Parameters.AddWithValue("rentedToDate", newRental.RentedToDate);
                    cmd.Parameters.AddWithValue("rentersName", newRental.RentersName);
                    cmd.Parameters.AddWithValue("dailyPrice", newRental.DailyPrice);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Rentals> GetAll()
        {
            var rentalsList = new List<Rentals>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT rental_id, car_id, rented_from_date, rented_to_date, renters_name, daily_price FROM rentals";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rentalsList.Add(new Rentals(reader.GetInt32(0), reader.GetInt32(1), reader.GetDateTime(2), reader.GetDateTime(3), reader.GetString(4), reader.GetDecimal(5)));
                        }
                    }
                }
            }
            return rentalsList;
        }

        public void Update(Rentals updatedRental)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE rentals SET car_id = @carId, rented_from_date = @rentedFromDate, rented_to_date = @rentedToDate, renters_name = @rentersName, daily_price = @dailyPrice WHERE rental_id = @rentalId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("carId", updatedRental.CarId);
                    cmd.Parameters.AddWithValue("rentedFromDate", updatedRental.RentedFromDate);
                    cmd.Parameters.AddWithValue("rentedToDate", updatedRental.RentedToDate);
                    cmd.Parameters.AddWithValue("rentersName", updatedRental.RentersName);
                    cmd.Parameters.AddWithValue("dailyPrice", updatedRental.DailyPrice);
                    cmd.Parameters.AddWithValue("rentalId", updatedRental.RentalId);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public void Delete(int rentalId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM rentals WHERE rental_id = @rentalId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("rentalId", rentalId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
