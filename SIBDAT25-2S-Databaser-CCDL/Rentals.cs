using System;
using System.Collections.Generic;
using System.Configuration;
using Npgsql;

namespace SIBDAT25_2S_Databaser_CCDL
{
    /// <summary>
    /// Repræsenterer en udlejning og indeholder CRUD-metoder
    /// (Create, Read, Update, Delete) til tabellen 'rentals'.
    ///
    /// Forventet tabelstruktur:
    /// - rental_id          (int, PK)
    /// - rental_car_id      (int, FK til cars.carId)
    /// - rented_from_date   (date)
    /// - rented_to_date     (date)
    /// - daily_price        (numeric/decimal)
    /// - costumer_id        (int, FK til customers.costumer_id)
    ///
    /// Bemærk: projektet bruger staveformen “costumer” i databasen.
    /// </summary>
    public class Rentals
    {
        // Connection string fra app.config
        private string connectionString = ConfigurationManager
            .ConnectionStrings["PostgreSQL"].ConnectionString;

        /// <summary>
        /// Id for udlejningen (primærnøgle).
        /// </summary>
        public int RentalId { get; set; }

        /// <summary>
        /// Id på bilen der udlejes.
        /// </summary>
        public int RentalCarId { get; set; }

        /// <summary>
        /// Startdato for udlejningen.
        /// </summary>
        public DateTime RentedFromDate { get; set; }

        /// <summary>
        /// Slutdato for udlejningen.
        /// </summary>
        public DateTime RentedToDate { get; set; }

        /// <summary>
        /// Dagspris for udlejningen.
        /// </summary>
        public decimal DailyPrice { get; set; }

        /// <summary>
        /// Kunde-id der har lejet bilen.
        /// </summary>
        public int CostumerId { get; set; }

        /// <summary>
        /// Opretter et Rentals-objekt ud fra alle felterne.
        /// </summary>
        public Rentals(
            int rentalId,
            int rentalCarId,
            DateTime rentedFromDate,
            DateTime rentedToDate,
            decimal dailyPrice,
            int costumerId)
        {
            RentalId = rentalId;
            RentalCarId = rentalCarId;
            RentedFromDate = rentedFromDate;
            RentedToDate = rentedToDate;
            DailyPrice = dailyPrice;
            CostumerId = costumerId;
        }

        /// <summary>
        /// Opretter (INSERT) en ny udlejning i databasen.
        ///
        /// Bemærk: SQL’en har en fejl: @rentersName eksisterer ikke,
        /// og parameter-navne matcher ikke SQL. Uændret efter ønske.
        /// </summary>
        public void Add(Rentals newRental)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // OBS: SQL her indeholder fejl.
                string query =
                    "INSERT INTO rentals (rental_car_id, rented_from_date, rented_to_date, daily_price, costumer_id) " +
                    "VALUES (@rentalCarId, @rentedFromDate, @rentedToDate, @rentersName, @dailyPrice)";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    // OBS: parameter-navne matcher ikke SQL’en korrekt.
                    cmd.Parameters.AddWithValue("rental_car_id", newRental.RentalCarId);
                    cmd.Parameters.AddWithValue("rentedFromDate", newRental.RentedFromDate);
                    cmd.Parameters.AddWithValue("rentedToDate", newRental.RentedToDate);
                    cmd.Parameters.AddWithValue("dailyPrice", newRental.DailyPrice);
                    cmd.Parameters.AddWithValue("costumerId", newRental.CostumerId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Henter alle udlejninger fra databasen og returnerer dem som en liste.
        /// </summary>
        public List<Rentals> GetAll()
        {
            var rentalsList = new List<Rentals>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                const string query =
                    "SELECT rental_id, rental_car_id, rented_from_date, rented_to_date, daily_price, costumer_id FROM rentals";

                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var rental = new Rentals(
                            reader.GetInt32(0),   // rental_id
                            reader.GetInt32(1),   // rental_car_id
                            reader.GetDateTime(2),// rented_from_date
                            reader.GetDateTime(3),// rented_to_date
                            reader.GetDecimal(4), // daily_price
                            reader.GetInt32(5));  // costumer_id

                        rentalsList.Add(rental);
                    }
                }
            }

            return rentalsList;
        }

        /// <summary>
        /// Opdaterer (UPDATE) en udlejning i databasen.
        ///
        /// Bemærk: SQL og parameter-navne matcher ikke hinanden.
        /// Jeg ændrer dem ikke (efter dit ønske), men markerer fejlene.
        /// </summary>
        public void Update(Rentals updatedRental)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query =
                    "UPDATE rentals SET rental_car_id = @RentalCarId, rented_from_date = @rentedFromDate, " +
                    "rented_to_date = @rentedToDate, daily_price = @dailyPrice, costumer_id = @costumerId " +
                    "WHERE rental_id = @rentalId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    // OBS: Parameter-navnet bør være @RentalCarId, ikke @carId
                    cmd.Parameters.AddWithValue("carId", updatedRental.RentalCarId);

                    cmd.Parameters.AddWithValue("rentedFromDate", updatedRental.RentedFromDate);

                    // OBS: Der bruges både @rental_id og @rentalId i metoden.
                    cmd.Parameters.AddWithValue("rental_id", updatedRental.RentalId);

                    cmd.Parameters.AddWithValue("dailyPrice", updatedRental.DailyPrice);
                    cmd.Parameters.AddWithValue("rentalId", updatedRental.RentalId);
                    cmd.Parameters.AddWithValue("costumerId", updatedRental.CostumerId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Sletter (DELETE) en udlejning ud fra rental_id.
        /// </summary>
        public void Delete(int rentalId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query =
                    "DELETE FROM rentals WHERE rental_id = @rentalId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("rentalId", rentalId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}