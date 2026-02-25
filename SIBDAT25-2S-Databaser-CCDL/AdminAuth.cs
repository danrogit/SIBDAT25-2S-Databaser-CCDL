using System;
using Npgsql;

namespace FlaadesystemV1
{
    public static class AdminAuth
    {
        /// <summary>
        /// Authenticate admin using plaintext password stored in table 'admin'
        /// Expects columns: admin_user, admin_password
        /// </summary>
        public static bool Authenticate(string connectionString, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
                return false;

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                const string query = "SELECT admin_password FROM admin WHERE admin_user = @user LIMIT 1";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("user", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return false;

                        if (reader.IsDBNull(0))
                            return false;

                        var dbPassword = reader.GetString(0);
                        // Plaintext compare because your DB stores varchar/plaintext for admin_password
                        return string.Equals(dbPassword, password, StringComparison.Ordinal);
                    }
                }
            }
        }
    }
}