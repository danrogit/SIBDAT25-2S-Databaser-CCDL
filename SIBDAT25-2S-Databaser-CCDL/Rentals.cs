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
        public int RentalCarId { get; set; }
        public DateTime RentedFromDate { get; set; }
        public DateTime RentedToDate { get; set; }
        public decimal DailyPrice { get; set; }
        public int CostumerId { get; set; }

        public Rentals(int rentalId, int rentalCarId, DateTime rentedFromDate, DateTime rentedToDate, decimal dailyPrice, int costumerId)
        {
            RentalId = rentalId;
            RentalCarId = rentalCarId;
            RentedFromDate = rentedFromDate;
            RentedToDate = rentedToDate;
            DailyPrice = dailyPrice;
            CostumerId = costumerId;
        }

        //CRUD
        public void Add(Rentals newRental)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO rentals (rental_car_id, rented_from_date, rented_to_date, daily_price, costumer_id) VALUES (@rentalCarId, @rentedFromDate, @rentedToDate, @rentersName, @dailyPrice)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("rental_car_id", newRental.RentalCarId);
                    cmd.Parameters.AddWithValue("rentedFromDate", newRental.RentedFromDate);
                    cmd.Parameters.AddWithValue("rentedToDate", newRental.RentedToDate);
                    cmd.Parameters.AddWithValue("dailyPrice", newRental.DailyPrice);
                    cmd.Parameters.AddWithValue("costumerId", newRental.CostumerId);
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
                string query = "SELECT rental_id, rental_car_id, rented_from_date, rented_to_date, daily_price, costumer_id FROM rentals";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rentalsList.Add(new Rentals(reader.GetInt32(0), reader.GetInt32(1), reader.GetDateTime(2), reader.GetDateTime(3), reader.GetDecimal(4), reader.GetInt32(5)));
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
                string query = "UPDATE rentals SET rental_car_id = @RentalCarId, rented_from_date = @rentedFromDate, rented_to_date = @rentedToDate, daily_price = @dailyPrice, costumer_id = @costumerId WHERE rental_id = @rentalId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("carId", updatedRental.RentalCarId);
                    cmd.Parameters.AddWithValue("rentedFromDate", updatedRental.RentedFromDate);
                    cmd.Parameters.AddWithValue("rental_id", updatedRental.RentalId);
                    cmd.Parameters.AddWithValue("dailyPrice", updatedRental.DailyPrice);
                    cmd.Parameters.AddWithValue("rentalId", updatedRental.RentalId);
                    cmd.Parameters.AddWithValue("costumerId", updatedRental.CostumerId);
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
