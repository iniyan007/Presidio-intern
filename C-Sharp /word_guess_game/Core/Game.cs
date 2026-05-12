using word_guess_game.Models;
using word_guess_game.Authentication;
using word_guess_game.Validation;
using word_guess_game.WordProvider;
using word_guess_game.FeedbackGenerator;
using word_guess_game.ScoreManagement;

namespace word_guess_game.Core
{
    public class Game
    {
        public void StartGame(string user_name)
        {
            WordProvider.WordProvider wordProvider = new WordProvider.WordProvider();
            GuessValidator guessValidator = new GuessValidator();
            string wordToGuess = wordProvider.GetRandomWord();
            ScoreManager scoreManager = new ScoreManager();
            FeedbackGenerator.FeedbackGenerator feedbackGenerator = new FeedbackGenerator.FeedbackGenerator();
            byte attempts = 6;

            while (attempts > 0)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"Enter your guess ({attempts} attempts left): ");
                Console.ResetColor();

                string userGuess = (Console.ReadLine() ?? "").ToUpper();

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
                        scoreManager.UpdateScore(Program.highScore, user_name);
                    }
                    
                    ScoreFeedback feedbackType = (ScoreFeedback)attempts;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{feedbackType}! You guessed correctly.");
                    Console.WriteLine($"Your score is: {Program.score}");
                    Console.ResetColor();
                    return;
                }
                else
                {
                    string feedback = feedbackGenerator.GenerateFeedback(userGuess, wordToGuess);
                    Console.WriteLine($"Your guess: {userGuess}");
                    Console.WriteLine($"Feedback: {feedback}");
                }

                attempts--;
            }

            Program.score = 0;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Game Over! The correct word was: {wordToGuess}");
            Console.WriteLine($"Your High Score is: {Program.highScore}");
            Console.ResetColor();
        }
    }
}
