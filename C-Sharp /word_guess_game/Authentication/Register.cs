using Npgsql;
using word_guess_game.Data;

namespace word_guess_game.Authentication
{
    public class Register
    {
        private readonly NpgsqlConnection _connection;

        public Register()
        {
            _connection = new NpgsqlConnection(DatabaseConfig.ConnectionString);
        }

        public void RegisterUser()
        {
            Console.WriteLine("--- Register ---");
            Console.Write("Enter username: ");
            string username = Console.ReadLine() ?? "";
            
            string password;
            string confirmPassword;

            do
            {
                Console.Write("Enter password: ");
                password = Console.ReadLine() ?? "";
                Console.Write("Confirm password: ");
                confirmPassword = Console.ReadLine() ?? "";

                if (password != confirmPassword)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Passwords do not match. Please try again.");
                    Console.ResetColor();
                }
            } while (password != confirmPassword);

            string query = $"INSERT INTO Users(user_name, user_password) VALUES ('{username}', '{password}')";
            using var command = new NpgsqlCommand(query, _connection);
            try
            {
                _connection.Open();
                int result = command.ExecuteNonQuery();
                if (result > 0)
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("User registered successfully");
                    Console.ResetColor();
            }
            catch (NpgsqlException ex)
            {
                if (ex.SqlState == "23505")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Username already exists.");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"Registration error: {ex.Message}");
                }
            }
            finally
            {
                _connection?.Close();
            }
        }
    }
}
