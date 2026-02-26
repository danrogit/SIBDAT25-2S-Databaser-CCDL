using System;
using System.Collections.Generic;
using System.Configuration;
using Npgsql;

namespace SIBDAT25_2S_Databaser_CCDL
{
    /// <summary>
    /// Repræsenterer en servicepost i værkstedssystemet
    /// og indeholder CRUD-metoder til tabellen 'service_center'.
    ///
    /// Forventet tabelstruktur:
    /// - submitted_date   (date)
    /// - completion_date  (date)
    /// - invoice_id       (int, PK)
    /// - invoice_price    (numeric/decimal)
    /// - intern           (text)
    /// - ekstern          (text)
    /// </summary>
    public class ServiceCenter
    {
        // Hentes fra app.config gennem <connectionStrings>.
        private string connectionString =
            ConfigurationManager.ConnectionStrings["PostgreSQL"].ConnectionString;

        /// <summary>Dato hvor servicen blev oprettet.</summary>
        public DateTime SubmittedDate { get; set; }

        /// <summary>Dato hvor servicen blev færdiggjort.</summary>
        public DateTime CompletionDate { get; set; }

        /// <summary>Fakturanummer (primærnøgle).</summary>
        public int InvoiceId { get; set; }

        /// <summary>Pris på servicearbejdet.</summary>
        public decimal InvoicePrice { get; set; }

        /// <summary>Intern kommentar (værkstedsnoter).</summary>
        public string Intern { get; set; }

        /// <summary>Ekstern kommentar (kundevendte noter).</summary>
        public string Ekstern { get; set; }

        /// <summary>
        /// Opretter et ServiceCenter-objekt ud fra alle felterne.
        /// </summary>
        public ServiceCenter(
            DateTime submittedDate,
            DateTime completionDate,
            int invoiceId,
            decimal invoicePrice,
            string intern,
            string ekstern)
        {
            SubmittedDate = submittedDate;
            CompletionDate = completionDate;
            InvoiceId = invoiceId;
            InvoicePrice = invoicePrice;
            Intern = intern;
            Ekstern = ekstern;
        }

        /// <summary>
        /// Opretter (INSERT) en ny servicepost i databasen.
        ///
        /// Bemærk:
        /// - Kolonnen invoice_id mangler i INSERT → det forventes autogenereret.
        /// - Der bruges to gange @serviceCenterType i stedet for @intern og @ekstern.
        ///   Dette vil få SQL til at fejle. Ikke rettet — kun kommenteret.
        /// </summary>
        public void Add(ServiceCenter newService)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query =
                    "INSERT INTO service_center " +
                    "(submitted_date, completion_date, invoice_price, intern, ekstern) " +
                    "VALUES (@submittedDate, @completionDate, @invoicePrice, @intern, @ekstern)";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("submittedDate", newService.SubmittedDate);
                    cmd.Parameters.AddWithValue("completionDate", newService.CompletionDate);
                    cmd.Parameters.AddWithValue("invoicePrice", newService.InvoicePrice);

                    // OBS – fejl i original kode:
                    // "serviceCenterType" bruges to gange → @intern og @ekstern sættes ikke korrekt!
                    cmd.Parameters.AddWithValue("serviceCenterType", newService.Intern);
                    cmd.Parameters.AddWithValue("serviceCenterType", newService.Ekstern);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Henter alle serviceposter fra databasen og returnerer dem
        /// som en liste af ServiceCenter-objekter.
        /// </summary>
        public List<ServiceCenter> GetAll()
        {
            var serviceList = new List<ServiceCenter>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query =
                    "SELECT submitted_date, completion_date, invoice_id, " +
                    "invoice_price, intern, ekstern FROM service_center";

                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var service = new ServiceCenter(
                            reader.GetDateTime(0),   // submitted_date
                            reader.GetDateTime(1),   // completion_date
                            reader.GetInt32(2),      // invoice_id
                            reader.GetDecimal(3),    // invoice_price
                            reader.GetString(4),     // intern
                            reader.GetString(5));    // ekstern

                        serviceList.Add(service);
                    }
                }
            }

            return serviceList;
        }

        /// <summary>
        /// Opdaterer (UPDATE) en servicepost i databasen.
        ///
        /// Bemærk:
        /// - SQL bruger @Intern og @Ekstern, men parametre hedder @serviceCenterType.
        /// - @invoiceId bruges både i SET og i WHERE (korrekt).
        /// - Dette vil give fejl — ikke rettet, kun kommenteret.
        /// </summary>
        public void Update(ServiceCenter updatedService)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query =
                    "UPDATE service_center SET " +
                    "submitted_date = @submittedDate, " +
                    "completion_date = @completionDate, " +
                    "invoice_id = @invoiceId, " +
                    "invoice_price = @invoicePrice, " +
                    "intern = @Intern, " +
                    "ekstern = @Ekstern " +
                    "WHERE invoice_id = @invoiceId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("submittedDate", updatedService.SubmittedDate);
                    cmd.Parameters.AddWithValue("completionDate", updatedService.CompletionDate);
                    cmd.Parameters.AddWithValue("invoiceId", updatedService.InvoiceId);
                    cmd.Parameters.AddWithValue("invoicePrice", updatedService.InvoicePrice);

                    // OBS – begge mangler korrekt parameter-navn.
                    cmd.Parameters.AddWithValue("serviceCenterType", updatedService.Intern);
                    cmd.Parameters.AddWithValue("serviceCenterType", updatedService.Ekstern);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Sletter (DELETE) en servicepost ud fra service_id.
        ///
        /// Bemærk:
        /// - Tabellen bruger invoice_id som primærnøgle, ikke service_id.
        /// - SQL vil derfor fejle i de fleste databaser.
        /// </summary>
        public void Delete(int serviceId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query =
                    "DELETE FROM service_center WHERE service_id = @serviceId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("serviceId", serviceId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}