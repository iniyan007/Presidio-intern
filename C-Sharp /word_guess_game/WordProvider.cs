namespace word_guess_game
{
    internal class WordProvider
    {
        private List<string> _words;

        public WordProvider()
        {
            _words = new List<string> { "APPLE", "BRAVE", "CHAIR", "DREAM", "EAGLE",
                                        "FLAME", "GRAPE", "HOUSE", "INPUT", "JOKER",
                                        "KNIFE", "LEMON", "MAGIC", "NIGHT", "OCEAN",
                                        "PIANO", "QUEEN", "RIVER", "STONE", "TABLE",
                                        "UNDER", "VIVID", "WHALE", "XENON", "YOUTH",
                                        "ZEBRA", "ANGEL", "BAKER", "CANDY", "DELTA",
                                        "EARTH", "FANCY", "GIANT", "HAPPY", "IVORY",
                                        "JELLY", "KOALA", "LIGHT", "MANGO", "NOBLE",
                                        "OLIVE", "PEARL", "QUICK", "ROBOT", "SMART",
                                        "TIGER", "URBAN", "VAPOR", "WATER", "XYLOL",
                                        "YOUNG", "ZESTY", "ADORE", "BEACH", "CROWN",
                                        "DAISY", "ELITE", "FROST", "GLOBE", "HEART",
                                        "IDEAL", "JUMPY", "KARMA", "LUCKY", "METAL",
                                        "NINJA", "ORBIT", "PEACE", "QUEST", "RANCH",
                                        "SHINY", "TRACK", "UNITY", "VIGOR", "WOVEN",
                                        "XYLEM", "YIELD", "ZONAL", "AMBER", "BLISS",
                                        "CRISP", "DWELL", "EAGER", "FABLE", "GRIND",
                                        "HOVER", "INDEX", "JOLLY", "KNEEL", "LUNAR",
                                        "MIRTH", "NOVEL", "OPERA", "PRIDE", "RIDER" };
        }
        public string GetRandomWord()
        {
            Random random = new Random();
            int index = random.Next(_words.Count);
            string word = _words[index];
            _words.RemoveAt(index);
            return word;
        }
    }
}