namespace WinFormsApp1
{
    public class TradeItem
    {
        public string Name { get; set; }

        public int Price { get; set; }

        public string All { get; set; }

        public string LowLeft { get; set; }

        public Rectangle Rect { get; set; }

        public Point Position
        {
            get
            {
                return new Point(Rect.X + Rect.Width / 2, Rect.Y + Rect.Height / 2);
            }
        }
    }
}
