namespace word_guess_game.FeedbackGenerator
{
    public class FeedbackGenerator
    {
        public string GenerateFeedback(string guess, string wordToGuess)
        {
            int length = wordToGuess.Length;
            char[] result = new string('X', length).ToCharArray();
            bool[] usedWord = new bool[length];
            bool[] usedGuess = new bool[length];
            for (int i = 0; i < length; i++)
            {
                if (guess[i] == wordToGuess[i])
                {
                    result[i] = 'C';
                    usedWord[i] = true;
                    usedGuess[i] = true;
                }
            }
            for (int i = 0; i < length; i++)
            {
                if (usedGuess[i]) continue;

                for (int j = 0; j < length; j++)
                {
                    if (!usedWord[j] && guess[i] == wordToGuess[j])
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