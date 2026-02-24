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

        public int ServiceId { get; set; }
        public int CarId { get; set; }
        public DateTime SubmittedDate { get; set; }
        public DateTime CompletionDate { get; set; }
        public string WorkType { get; set; }
        public int InvoiceId { get; set; }
        public decimal InvoicePrice { get; set; }
        public string ServiceCenterType { get; set; }

        public ServiceCenter(int serviceId, int carId, DateTime submittedDate, DateTime completionDate, string workType, int invoiceId, decimal invoicePrice, string serviceCenterType)
        {
            ServiceId = serviceId;
            CarId = carId;
            SubmittedDate = submittedDate;
            CompletionDate = completionDate;
            WorkType = workType;
            InvoiceId = invoiceId;
            InvoicePrice = invoicePrice;
            ServiceCenterType = serviceCenterType;
        }

        public void Add(ServiceCenter newService)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO service_center (car_id, submitted_date, completion_date, work_type, invoice_id, invoice_price, service_center_type) VALUES (@carId, @submittedDate, @completionDate, @workType, @invoiceId, @invoicePrice, @serviceCenterType)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("carId", newService.CarId);
                    cmd.Parameters.AddWithValue("submittedDate", newService.SubmittedDate);
                    cmd.Parameters.AddWithValue("completionDate", newService.CompletionDate);
                    cmd.Parameters.AddWithValue("workType", newService.WorkType);
                    cmd.Parameters.AddWithValue("invoiceId", newService.InvoiceId);
                    cmd.Parameters.AddWithValue("invoicePrice", newService.InvoicePrice);
                    cmd.Parameters.AddWithValue("serviceCenterType", newService.ServiceCenterType);
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
                string query = "SELECT service_id, car_id, submitted_date, completion_date, work_type, invoice_id, invoice_price, service_center_type FROM service_center";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            serviceList.Add(new ServiceCenter(reader.GetInt32(0), reader.GetInt32(1), reader.GetDateTime(2), reader.GetDateTime(3), reader.GetString(4), reader.GetInt32(5), reader.GetDecimal(6), reader.GetString(7)));
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
                string query = "UPDATE service_center SET car_id = @carId, submitted_date = @submittedDate, completion_date = @completionDate, work_type = @workType, invoice_id = @invoiceId, invoice_price = @invoicePrice, service_center_type = @serviceCenterType WHERE service_id = @serviceId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("carId", updatedService.CarId);
                    cmd.Parameters.AddWithValue("submittedDate", updatedService.SubmittedDate);
                    cmd.Parameters.AddWithValue("completionDate", updatedService.CompletionDate);
                    cmd.Parameters.AddWithValue("workType", updatedService.WorkType);
                    cmd.Parameters.AddWithValue("invoiceId", updatedService.InvoiceId);
                    cmd.Parameters.AddWithValue("invoicePrice", updatedService.InvoicePrice);
                    cmd.Parameters.AddWithValue("serviceCenterType", updatedService.ServiceCenterType);
                    cmd.Parameters.AddWithValue("serviceId", updatedService.ServiceId);
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
