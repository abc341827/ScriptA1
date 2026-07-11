namespace WinFormsApp1
{
    public class MarketAutomationOptions
    {
        public bool IsLeiGod { get; set; }

        public int RollSize { get; set; } = 2;

        public int RollNumber { get; set; } = 1;

        public Color BorderColor { get; set; } = Color.FromArgb(102, 205, 213);

        public int SwipePause { get; set; } = 5;

        public int UpSwipeExtra { get; set; } = 1;
    }
}
