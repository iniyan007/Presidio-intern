using Npgsql;
using word_guess_game.Data;

namespace word_guess_game.ScoreManagement
{
    public class ScoreManager
    {
        private readonly NpgsqlConnection _connection;

        public ScoreManager()
        {
            _connection = new NpgsqlConnection(DatabaseConfig.ConnectionString);
        }
        public void UpdateScore(byte score, string user_name)
        {
            string query = $"UPDATE Users SET user_highscore = '{score}' WHERE user_name = '{user_name}'";
            using var command = new NpgsqlCommand(query, _connection);
            try
            {
                _connection.Open();
                int result = command.ExecuteNonQuery();
                if (result > 0)
                    Console.WriteLine("Score updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating score: {ex.Message}");
            }
            finally
            {
                _connection?.Close();
            }
        }
    }
}