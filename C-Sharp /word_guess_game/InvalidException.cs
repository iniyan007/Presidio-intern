namespace word_guess_game
{
    internal class InvalidException : Exception
    {
        string _messsage;
        public InvalidException(string msg)
        {
            _messsage = msg;
        }
        public override string Message => _messsage;
    }
}