using word_guess_game.Exceptions;

namespace word_guess_game.Validation
{
    public class GuessValidator
    {
        public bool ValidateGuess(string guess, string wordToGuess)
        {
            try
            {
                if (string.IsNullOrEmpty(guess) || string.IsNullOrEmpty(wordToGuess))
                {
                    throw new InvalidException("Guess and word to guess cannot be null or empty.");
                }

                if (!guess.All(char.IsLetter))
                {
                    throw new InvalidException("Guess must contain only letters.");
                }
                if (guess.Length != wordToGuess.Length)
                {
                    throw new InvalidException($"Guess must be {wordToGuess.Length} letters long.");
                }
                return true;
            }
            catch (InvalidException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unexpected error during validation: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }
    }
}