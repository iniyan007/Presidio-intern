using Interfaces;
using Models;
using Npgsql;

namespace DataAccessLayer
{
    public class NotificationRepository : INotificationRepository
    {
        string connectionString =
            "Host=localhost;Port=5432;Database=notification_app;Username=postgres;Password=iniyanavin";
        NpgsqlConnection connection;
        public NotificationRepository()
        {
            connection =
                new NpgsqlConnection(connectionString);
        }

        public Notification SaveNotification(string notificationType,string to_address,string message)
        {
            Notification notification =new Notification(notificationType,to_address,message);
            string insertCmd =
                $"insert into notifications (notification_type,to_address,message) values ('{notification.NotificationType}'," +
                $"'{notification.ToAddress}'," +
                $"'{notification.Message}')";
            NpgsqlCommand command = new NpgsqlCommand(insertCmd, connection);
            try
            {
                connection.Open();
                int result = command.ExecuteNonQuery();
                if(result > 0)
                    Console.WriteLine("Notification saved successfully");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

            return notification;
        }
        public List<Notification> GetAllNotifications()
        {
            List<Notification> notifications =new List<Notification>();
            string selectCmd ="select * from notifications";
            NpgsqlCommand command = new NpgsqlCommand(selectCmd, connection);
            try
            {
                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                while(reader.Read())
                {
                    Notification notification =
                        new Notification
                        {
                            Id =Convert.ToInt32(reader[0]),
                            NotificationType = reader[1].ToString()??"",
                            ToAddress = reader[2].ToString()??"",
                            Message = reader[3].ToString()??"",
                            Time = Convert.ToDateTime(reader[4])
                        };

                    notifications.Add(notification);
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

            return notifications;
        }

        public List<Notification> GetNotificationsByUser(string to_email, string to_phone)
        {
            List<Notification> notifications = new List<Notification>();

            string selectCmd =
                $"select * from notifications " +
                $"where to_address='{to_email}' " +
                $"or to_address='{to_phone}'";

            NpgsqlCommand command = new NpgsqlCommand(selectCmd, connection);
            try
            {
                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                while(reader.Read())
                {
                    Notification notification =new Notification
                        {
                            Id = Convert.ToInt32(reader[0]),
                            NotificationType = reader[1].ToString()??"",
                            ToAddress = reader[2].ToString()??"",
                            Message =reader[3].ToString()??"",
                            Time = Convert.ToDateTime(reader[4])
                        };
                    notifications.Add(notification);
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
            return notifications;
        }
    }
}