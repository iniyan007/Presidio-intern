using Npgsql;
using word_guess_game.Data;

namespace word_guess_game.Authentication
{
    public class Login
    {
        private string? _username;
        private readonly NpgsqlConnection _connection;

        public Login()
        {
            _connection = new NpgsqlConnection(DatabaseConfig.ConnectionString);
        }

       

        public string Validate(ref bool isLoggedin, ref byte highScore)
        {
            Console.Write("Enter username: ");
            _username = Console.ReadLine() ?? "";
            Console.Write("Enter password: ");
            string password = Console.ReadLine() ?? "";

            string query = $"SELECT user_password, user_highscore FROM Users WHERE user_name = '{_username}'";
            using var command = new NpgsqlCommand(query, _connection);
            try
            {
                _connection.Open();
                using var reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    isLoggedin = false;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid username");
                    Console.ResetColor();
                    return "";
                }

                string? dbPassword = reader[0].ToString();
                if (dbPassword == password)
                {
                    isLoggedin = true;
                    highScore = Convert.ToByte(reader[1]);
                    Console.WriteLine("Login successful");
                    return _username;
                }
                else
                {
                    isLoggedin = false;
                    Console.WriteLine("Invalid credentials");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
            }
            finally
            {
                _connection?.Close();
            }
            return "";
        }
    }
}
