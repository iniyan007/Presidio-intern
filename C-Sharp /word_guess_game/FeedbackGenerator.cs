namespace word_guess_game
{
    internal class FeedbackGenerator
    {
        char[] wordToGuessArr = new char[5];
        char[] userGuessArr = new char[5];
       public string GenerateFeedback(string guess, string wordToGuess)
        {
            char[] result = { 'X', 'X', 'X', 'X', 'X' };
            bool[] usedWord = new bool[5];
            bool[] usedGuess = new bool[5];
            for (int i = 0; i < 5; i++)
            {
                if (guess[i] == wordToGuess[i])
                {
                    result[i] = 'C';
                    usedWord[i] = true;
                    usedGuess[i] = true;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                if (usedGuess[i])
                    continue;

                for (int j = 0; j < 5; j++)
                {
                    if (!usedWord[j] &&
                        guess[i] == wordToGuess[j])
                    {
                        result[i] = 'Y';
                        usedWord[j] = true;
                        break;
                    }
                }
            }
            return new string(result);
        }
    }
}