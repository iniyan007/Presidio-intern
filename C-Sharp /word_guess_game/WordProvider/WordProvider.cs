using Npgsql;
using word_guess_game.Data;

namespace word_guess_game.WordProvider
{
    public class WordProvider
    {
        private readonly NpgsqlConnection _connection;

        public WordProvider()
        {
            _connection = new NpgsqlConnection(DatabaseConfig.ConnectionString);
        }

        public string GetRandomWord()
        {
            Random random = new Random();
            int index = random.Next(1, 95);
            string query = $"SELECT word FROM word WHERE Id = {index}";
            using var command = new NpgsqlCommand(query, _connection);
            try
            {
                _connection.Open();
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return reader[0]?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching word: {ex.Message}");
            }
            finally
            {
                _connection?.Close();
            }
            return "APPLE";
        }
    }
}