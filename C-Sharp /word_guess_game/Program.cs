namespace word_guess_game
{
    internal class Program
    {
        public static byte score = 0;
        public static byte highScore = 0;
        static int DisplayMenu()
        {
            int choice;
            Console.WriteLine("Enter 1 for new word");
            System.Console.WriteLine("Enter 2 to view the rules of the game");
            System.Console.WriteLine("Enter 3 to view the score");
            Console.WriteLine("Enter 4 to exit");

            while (!int.TryParse(Console.ReadLine(), out choice) ||
                   (choice >= 1 && choice <= 4) == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid input. Please enter valid input.");
                Console.ResetColor();
            }

            return choice;
        }

        static void Main(string[] args)
        {
            Game game = new Game();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Welcome to the Word Guess Game!");
            Console.WriteLine("Guess the 5-letter word in 6 attempts.");
            Console.ResetColor();
            while (true)
            {
                int userChoice = DisplayMenu();

                switch (userChoice)
                {
                    case 1:
                        game.StartGame();
                        break;
                    case 2:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine("Rules of the game:");
                        Console.WriteLine("1. You have 6 attempts to guess the 5-letter word.");
                        Console.WriteLine("2. After each guess, you will receive feedback:");
                        Console.WriteLine("   - 'C' indicates a correct letter in the correct position.");
                        Console.WriteLine("   - 'Y' indicates a correct letter in the wrong position.");
                        Console.WriteLine("   - 'X' indicates an incorrect letter.");
                        Console.WriteLine("3. Duplicate letters are handled carefully.");
                        Console.WriteLine("   Example:");
                        Console.WriteLine("   WORD  : PLANT");
                        Console.WriteLine("   GUESS : APPLE");
                        Console.WriteLine("   RESULT: YYXYX");
                        Console.WriteLine("   The second 'P' is marked as 'X' because");
                        Console.WriteLine("   the word contains only one 'P'.");
                        Console.WriteLine("4. Use the feedback to refine your guesses and find the correct word.");
                        Console.ResetColor();
                        break;
                    case 3:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Your current score is: {score}");
                        Console.WriteLine($"Your high score is: {highScore}");
                        Console.ResetColor();
                        break;
                    case 4:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Exiting the game. Goodbye!");
                        Console.ResetColor();
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }
    }
}