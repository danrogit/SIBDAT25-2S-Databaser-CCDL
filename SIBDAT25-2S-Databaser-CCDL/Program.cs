using System;
using Npgsql;

namespace SIBDAT25_2S_DBString
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Programmet tester kun om der kan oprettes forbindelse til databasen.
            // Ingen læse-/skriveoperationer udføres her.
            // NOTE: I produktion bør du hente URI/password fra en sikker kilde (fx miljøvariabel).

            var uri = new Uri("postgres://postgres:FSP02UXAG14HBHLiqtjKMU7R47akAG2Hk0Lsh0ySg2DBO0OfkHLkvwp8WOoXx89u@95.211.27.223:5510/postgres");

            // Byg en Npgsql-tilslutningsstreng ud fra URI'en
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port,
                Username = uri.UserInfo.Split(':')[0],
                Password = uri.UserInfo.Split(':')[1],
                Database = uri.AbsolutePath.TrimStart('/'),
                SslMode = SslMode.Disable
            };

            string connectionString = builder.ConnectionString;

            // Forsøg at åbne forbindelse — rapportér kun succes eller fejl
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    Console.WriteLine("Forbindelse oprettet succesfuldt.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Forbindelse fejlede: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }
    }
}
