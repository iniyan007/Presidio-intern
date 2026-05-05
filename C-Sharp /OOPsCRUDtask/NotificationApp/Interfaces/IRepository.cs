
namespace NotificationApp.Interfaces
{
    public interface IRepository<K,T> where T: class
    {
        public T AddUser(T item);
        public T? GetUserByName(K key);
        public List<T>? GetAllUsers();
        public T? UpdateUser(K key,T item);
        public T? DeleteUser(K key);

    }
}