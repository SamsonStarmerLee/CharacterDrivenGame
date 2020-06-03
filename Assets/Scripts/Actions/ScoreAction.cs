namespace Assets.Scripts.Actions
{
    class ScoreAction
    {
        public readonly int ScoreChange;
        public readonly string Word;

        public ScoreAction(int change, string word)
        {
            ScoreChange = change;
            Word = word;
        }
    }
}
