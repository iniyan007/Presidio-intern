using Models;
using Interfaces;
using Npgsql;

namespace DataAccessLayer
{
    public class UserRepository : IUserRepository
    {
        string connectionString =
            "Host=localhost;Port=5432;Database=notification_app;Username=postgres;Password=iniyanavin";

        NpgsqlConnection connection;

        public UserRepository()
        {
            connection = new NpgsqlConnection(connectionString);
        }

        public User CreateUser(string name, string email, string phone)
        {
            User user = new User(name, email, phone);

            string insertCmd =
                $"insert into users(name,email,phone) " +
                $"values('{user.Name}','{user.Email}','{user.Phone}')";

            NpgsqlCommand command = new NpgsqlCommand(insertCmd, connection);
            try
            {
                connection.Open();

                int result = command.ExecuteNonQuery();
                if(result > 0)
                    Console.WriteLine("User created successfully");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
            return user;
        }

        public User? GetUserByName(string name)
        {
            User? user = null;
            string selectCmd = $"select * from users where name='{name}'";
            NpgsqlCommand command = new NpgsqlCommand(selectCmd, connection);
            try
            {
                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                if(reader.Read())
                {
                    user = new User
                    {
                        Id = Convert.ToInt32(reader[0]),
                        Name = reader[1].ToString()??"",
                        Email = reader[2].ToString()??"",
                        Phone = reader[3].ToString()??""
                    };
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

            return user;
        }
        public User? GetUserByPhone(string phone)
        {
            User? user = null;
            string selectCmd = $"select * from users where phone='{phone}'";
            NpgsqlCommand command = new NpgsqlCommand(selectCmd, connection);
            try
            {
                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                if(reader.Read())
                {
                    user = new User
                    {
                        Id = Convert.ToInt32(reader[0]),
                        Name = reader[1].ToString()??"",
                        Email = reader[2].ToString()??"",
                        Phone = reader[3].ToString()??""
                    };
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
            return user;
        }
        public User UpdateUser(User user,string newName,string newEmail,string newPhone)
        {
            string updateCmd =
                $"update users set " +
                $"name='{newName}', " +
                $"email='{newEmail}', " +
                $"phone='{newPhone}' " +
                $"where id={user.Id}";

            NpgsqlCommand command = new NpgsqlCommand(updateCmd, connection);

            try
            {
                connection.Open();
                int result = command.ExecuteNonQuery();
                if(result > 0)
                {
                    user.Name = newName;
                    user.Email = newEmail;
                    user.Phone = newPhone;
                    Console.WriteLine("User updated successfully");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

            return user;
        }
        public void DeleteUser(User user)
        {
            string deleteCmd = $"delete from users where id={user.Id}";

            NpgsqlCommand command = new NpgsqlCommand(deleteCmd, connection);

            try
            {
                connection.Open();
                int result = command.ExecuteNonQuery();
                if(result > 0)
                    Console.WriteLine("User deleted successfully");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
        }
    }
}