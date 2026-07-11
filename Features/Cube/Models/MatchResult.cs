namespace WinFormsApp1
{
    public class MatchResult
    {
        public bool AllMatched { get; set; }
        public List<SingleMatch> Matches { get; set; } = new List<SingleMatch>();
    }
}
