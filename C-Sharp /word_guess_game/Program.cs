using word_guess_game.Core;
using word_guess_game.Authentication;

namespace word_guess_game
{
    internal class Program
    {
        private static bool _isLoggedin = false;
        public static byte score = 0;
        public static byte highScore = 0;
        public static string user_name="";

        private static int DisplayMenu()
        {
            int choice;
            Console.WriteLine("\n--- Game Menu ---");
            Console.WriteLine("1. New Game (Get a new word)");
            Console.WriteLine("2. View Rules");
            Console.WriteLine("3. View High Score");
            Console.WriteLine("4. Logout");
            Console.WriteLine("5. Exit");
            Console.Write("Choice: ");

            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 5)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid input. Please enter a number between 1 and 5.");
                Console.ResetColor();
                Console.Write("Choice: ");
            }

            return choice;
        }

        public static void LoginOrRegister()
        {
            Console.WriteLine("\n--- Authentication ---");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine("3. Exit");
            Console.Write("Choice: ");

            if (!int.TryParse(Console.ReadLine(), out int choice) || !(choice ==1 || choice == 2 || choice == 3))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid choice.");
                Console.ResetColor();
                return;
            }

            if (choice == 1)
            {
                Login login = new Login();
                user_name = login.Validate(ref _isLoggedin, ref highScore);
            }
            else if(choice == 2)
            {
                Register register = new Register();
                register.RegisterUser();
            }
            else
            {
                Console.WriteLine("Exiting Application");
                Environment.Exit(0);
            }
        }

        static void Main(string[] args)
        {
            Game game = new Game();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("**************************************");
            Console.WriteLine("*   Welcome to the Word Guess Game!  *");
            Console.WriteLine("* Guess the 5-letter word in 6 tries *");
            Console.WriteLine("**************************************");
            Console.ResetColor();

            while (true)
            {
                if (!_isLoggedin)
                {
                    LoginOrRegister();
                }
                else
                {
                    int userChoice = DisplayMenu();
                    switch (userChoice)
                    {
                        case 1:
                            game.StartGame(user_name);
                            break;
                        case 2:
                            ShowRules();
                            break;
                        case 3:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Your high score is: {highScore}");
                            Console.ResetColor();
                            break;
                        case 4:
                            _isLoggedin = false;
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("Logged out successfully.");
                            Console.ResetColor();
                            break;
                        case 5:
                            Console.WriteLine("Goodbye!");
                            Environment.Exit(0);
                            break;
                    }
                }
            }
        }

        private static void ShowRules()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\nRules of the game:");
            Console.WriteLine("1. You have 6 attempts to guess the 5-letter word.");
            Console.WriteLine("2. Feedback after each guess:");
            Console.WriteLine("   - 'C': Correct letter, correct position.");
            Console.WriteLine("   - 'Y': Correct letter, wrong position.");
            Console.WriteLine("   - 'X': Incorrect letter.");
            Console.WriteLine("3. Duplicate letters are handled accurately.");
            Console.WriteLine("Example: Word: PLANT, Guess: APPLE -> Result: YYXYX");
            Console.ResetColor();
        }
    }
}