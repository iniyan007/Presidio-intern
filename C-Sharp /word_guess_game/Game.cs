namespace word_guess_game
{
    internal class Game
    {
        private enum ScoreFeedback
        {
            ThatWasClose = 1,
            NiceTry = 2,
            GoodWork = 3,
            GreatJob = 4,
            Excellent = 5,
            Genius = 6
        }
        public void StartGame()
        {
            WordProvider wordProvider = new WordProvider();
            GuessValidator guessValidator = new GuessValidator();
            string wordToGuess = wordProvider.GetRandomWord();
            FeedbackGenerator feedbackGenerator = new FeedbackGenerator();
            byte attempts = 6;

            while (attempts > 0)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"Enter your guess ({attempts} attempts left): ");
                Console.ResetColor();

                string userGuess = Console.ReadLine() ?? "";

                userGuess = userGuess.ToUpper();
                if (!guessValidator.ValidateGuess(userGuess, wordToGuess))
                {
                    continue;
                }
                
                if (userGuess == wordToGuess)
                {
                    Program.score++;
                    if (Program.score > Program.highScore)
                    {
                        Program.highScore = Program.score;
                    }
                    ScoreFeedback scoreFeedback = (ScoreFeedback)(attempts);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{scoreFeedback} You guessed correctly.");
                    System.Console.WriteLine($"Your score is: {Program.score}");
                    Console.ResetColor();
                    return;
                }
                else
                {
                    string feedback = feedbackGenerator.GenerateFeedback(userGuess, wordToGuess);
                    System.Console.WriteLine($"Your guess: {userGuess}");
                    Console.WriteLine($"Feedback: {feedback}");
                }

                attempts--;
            }
            Program.score = 0;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Game Over! The correct word was: {wordToGuess}\nYour High Score is: {Program.highScore}");
            Console.ResetColor();
        }
    }
}