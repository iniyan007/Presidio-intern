using Npgsql;
using System.Net.NetworkInformation;

namespace UnderstandingADOApp
{
    
    internal class Program
    {
        string connectionString =
            "Host=localhost;Port=5437;Database=gen_sparks_training;Username=iniyan;Password=iniyanavin";
        NpgsqlConnection connection;
        public Program()
        {
          connection = new NpgsqlConnection(connectionString);
           
        }
        void GetProductDataFromDatabase()
        {
            string selectQuery = "Select * from Products";
            NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection);
            try
            {
                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("Product Id : " + reader[0].ToString());
                    Console.WriteLine("Product Name : " + reader[1].ToString());
                }
                Console.WriteLine("Done reading");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

        }
        void GetUserFromDatabase()
        {
            Console.WriteLine("Enter Name to get the details");
            string name = Console.ReadLine()??"";
            string selectQuery = $"Select * from Users Where user_name = '{name}'";
            NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection);
            try
            {
                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("Product Id : " + reader[0].ToString());
                    Console.WriteLine("Product Name : " + reader[1].ToString());
                }
                Console.WriteLine("Done reading");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
        }

        void InsertUserInToDatabase()
        {
            User user = GetUserDataFromConsole();
            string insertCmd = $"Insert into Users values('{user.Username}','{user.Password}','{user.Role}')";
            NpgsqlCommand command = new NpgsqlCommand(insertCmd, connection);
            try
            {
                connection.Open();
                int result = command.ExecuteNonQuery();
                if(result>0)
                    Console.WriteLine("User created successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                connection?.Close();
            }
        }
        void UpdateUserPassword()
        {
            Console.WriteLine("Enter user_name to update password");
            string name  = Console.ReadLine()??"";
            Console.WriteLine("Enter new Password to update");
            string new_password = Console.ReadLine()??"";
            string updatecmd = $"UPDATE Users SET user_password = '{new_password}' WHERE user_name = '{name}'";
            NpgsqlCommand command = new NpgsqlCommand(updatecmd, connection);
            try
            {
                connection.Open();
                int result = command.ExecuteNonQuery();
                if(result>0)
                    Console.WriteLine("User password updated successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                connection?.Close();
            }
        }
        private User GetUserDataFromConsole()
        {
            User user = new User();
            Console.WriteLine("Please eneter your preffered username");
            user.Username = Console.ReadLine()??"";
            Console.WriteLine("Please eneter teh password");
            user.Password = Console.ReadLine()??"";
            Console.WriteLine("Please eneter your role");
            user.Role = Console.ReadLine() ?? "";
            return user;

        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Program prgm = new Program();
            //prgm.InsertUserInToDatabase();
            prgm.UpdateUserPassword();


        }
    }
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}


// "Host=localhost;Port=5437;Database=gen_sparks_training;Username=iniyan;Password=iniyanavin";