
namespace LinQ
{
    class Program{
        public static void print(List<Employee> res){
            Console.WriteLine("---------------------------");
            foreach(var e in res){
                Console.WriteLine(e);
            }
            Console.WriteLine("---------------------------");
        }
        public static void Main(string[] args){
            List<Employee> employees = new List<Employee>
            {
                new Employee { Id = 1, Name = "Arun", Department = "HR", Age = 25, Salary = 30000 },
                new Employee { Id = 2, Name = "Kavin", Department = "IT", Age = 30, Salary = 60000 },
                new Employee { Id = 3, Name = "Riya", Department = "Finance", Age = 28, Salary = 50000 },
                new Employee { Id = 4, Name = "John", Department = "IT", Age = 35, Salary = 75000 },
                new Employee { Id = 5, Name = "Meena", Department = "HR", Age = 27, Salary = 40000 }
            };
            List<Employee> res = employees.Where(e => e.Department == "IT").ToList();
            Program.print(res);
            List<Employee> res2 = employees.Where(e => e.Salary > 50000).ToList();
            Program.print(res2);
            int res3 = employees.Count(e => e.Department == "HR");
            List<Employee> res4 = employees.Where(e => (e.Salary>40000 && e.Salary <70000)).ToList();
            Program.print(res4);
            int res5 = employees.Count(e => (e.Age>28));
            bool res6 = employees.Any(e => e.Department=="ADMIN");
            Employee emp = employees.FirstOrDefault(e => e.Name == "John");
            Console.WriteLine(emp);
            int tot_sal = (int)employees.Sum(e => e.Salary);
            Console.WriteLine(tot_sal);
            List<Employee> res7 = employees.OrderBy(e => e.Name).ToList();
            Program.print(res7);
            List<Employee> res8 = employees.OrderByDescending(e => e.Name).ToList();
            Program.print(res8);
            List<Employee> res9 = employees.OrderBy(e => e.Age).Take(2).ToList();
            Program.print(res9);
            List<Employee> res10 = employees.OrderByDescending(e => e.Salary).Skip(2).ToList();
            Program.print(res10);
            double avg = employees.Average(e=>e.Salary);
            Console.WriteLine(avg);
            double minsal = employees.Min(e=>e.Salary);
            Console.WriteLine(minsal);
            double MaxAge = employees.Max(e=>e.Age);
            Console.WriteLine(MaxAge);
            var c = employees.GroupBy(e => e.Department == "HR").Count()
        }
    }
}