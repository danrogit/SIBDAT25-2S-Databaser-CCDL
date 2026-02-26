using System;
using Npgsql;

// AdminAuth.cs
// Enkel hjælpeklasse til at verificere en administrator-bruger mod en database.
// Kommentarerne i denne fil er tilpasset et første/andet semester projekt:
// - formål, parametre og vigtige antagelser beskrives kort.
// - ingen avanceret sikkerhed, kun simpel demo / studiebrug.

namespace FlaadesystemV1
{
    /// <summary>
    /// Statisk hjælpeklasse, der står for at validere admin-login.
    /// Bruges fra Program.Main, når en admin logger ind.
    /// </summary>
    public static class AdminAuth
    {
        /// <summary>
        /// Forsøger at autentificere en admin-bruger mod databasen.
        ///
        /// Forventninger til databasen:
        /// - Der findes en tabel 'admin'
        /// - Tabel 'admin' har mindst kolonnerne:
        ///     * admin_user (brugernavn)
        ///     * admin_password (kodeord, gemt i klartekst i dette projekt)
        ///
        /// Returnerer:
        /// - true  hvis brugernavn findes og password matcher 100%
        /// - false hvis bruger ikke findes, password er null/forkert,
        ///   eller der opstår en fejl undervejs.
        /// </summary>
        /// <param name="connectionString">
        /// Connection string til PostgreSQL-databasen (samme som i Program.cs).
        /// </param>
        /// <param name="username">
        /// Det indtastede admin-brugernavn fra konsollen.
        /// </param>
        /// <param name="password">
        /// Det indtastede admin-password fra konsollen.
        /// </param>
        public static bool Authenticate(string connectionString, string username, string password)
        {
            // Valider inddata tidligt (defensive programming):
            // Hvis brugernavn eller password er tomt, gider vi ikke engang slå op i DB.
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
                return false;

            // using sikrer at forbindelsen bliver lukket/disposed,
            // også selvom der opstår en exception.
            using (var conn = new NpgsqlConnection(connectionString))
            {
                // Åbn forbindelse og læs passwordet fra databasen.
                // Bemærk: i dette studieprojekt gemmes passwords i klartekst
                // (det er IKKE anbefalet i produktion – her bør man bruge hashing/salt).
                conn.Open();

                // Vi henter kun én række (LIMIT 1) for det angivne brugernavn.
                const string query = "SELECT admin_password FROM admin WHERE admin_user = @user LIMIT 1";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    // Parameteriseret query: beskytter imod SQL injection,
                    // fordi vi ikke concatenater brugernavn direkte ind i SQL-strengen.
                    cmd.Parameters.AddWithValue("user", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        // Hvis der ikke kom nogen rækker tilbage, findes brugeren ikke.
                        if (!reader.Read())
                            return false;

                        // Hvis feltet er NULL i databasen, kan vi ikke validere password.
                        if (reader.IsDBNull(0))
                            return false;

                        // Læs passwordet som string fra første kolonne (index 0).
                        var dbPassword = reader.GetString(0);

                        // Sammenlign brugers input med det der ligger i databasen.
                        // StringComparison.Ordinal = case-sensitiv sammenligning.
                        // Plaintext compare fordi admin_password er gemt som varchar/plaintext.
                        return string.Equals(dbPassword, password, StringComparison.Ordinal);
                    }
                }
            }
        }
    }
}