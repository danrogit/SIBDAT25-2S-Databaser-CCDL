using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Npgsql;

namespace SIBDAT25_2S_Databaser_CCDL
{
    public class ServiceCenter
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQL"].ConnectionString;

        public DateTime SubmittedDate { get; set; }
        public DateTime CompletionDate { get; set; }
        public int InvoiceId { get; set; }
        public decimal InvoicePrice { get; set; }
        public string Intern { get; set; }
        public string Ekstern { get; set; }


        public ServiceCenter(DateTime submittedDate, DateTime completionDate, int invoiceId, decimal invoicePrice, string intern, string ekstern)
        {

            SubmittedDate = submittedDate;
            CompletionDate = completionDate;
            InvoiceId = invoiceId;
            InvoicePrice = invoicePrice;
            Intern = intern;
            Ekstern = ekstern;
        }

        //CRUD
        public void Add(ServiceCenter newService)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO service_center (submitted_date, completion_date, invoice_price, intern, ekstern) VALUES (@submittedDate, @completionDate, @invoicePrice, @intern, @ekstern)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("submittedDate", newService.SubmittedDate);
                    cmd.Parameters.AddWithValue("completionDate", newService.CompletionDate);
                    cmd.Parameters.AddWithValue("invoicePrice", newService.InvoicePrice);
                    cmd.Parameters.AddWithValue("serviceCenterType", newService.Intern);
                    cmd.Parameters.AddWithValue("serviceCenterType", newService.Ekstern);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<ServiceCenter> GetAll()
        {
            var serviceList = new List<ServiceCenter>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT submitted_date, completion_date, invoice_id, invoice_price, intern, ekstern FROM service_center";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            serviceList.Add(new ServiceCenter(reader.GetDateTime(0), reader.GetDateTime(1), reader.GetInt32(2), reader.GetDecimal(3), reader.GetString(4), reader.GetString(5)));
                        }
                    }
                }               
            }
            return serviceList;
        }


        public void Update(ServiceCenter updatedService)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE service_center SET submitted_date = @submittedDate, completion_date = @completionDate, invoice_id = @invoiceId, invoice_price = @invoicePrice, intern = @Intern, ekstern = @Ekstern WHERE invoice_id = @invoiceId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("submittedDate", updatedService.SubmittedDate);
                    cmd.Parameters.AddWithValue("completionDate", updatedService.CompletionDate);
                    cmd.Parameters.AddWithValue("invoiceId", updatedService.InvoiceId);
                    cmd.Parameters.AddWithValue("invoicePrice", updatedService.InvoicePrice);
                    cmd.Parameters.AddWithValue("serviceCenterType", updatedService.Intern);
                    cmd.Parameters.AddWithValue("serviceCenterType", updatedService.Ekstern);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public void Delete(int serviceId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM service_center WHERE service_id = @serviceId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("serviceId", serviceId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}
