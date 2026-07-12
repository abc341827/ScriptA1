using System.Drawing;

namespace WinFormsApp1
{
    public sealed class OcrTextLine
    {
        public OcrTextLine(string text, float score, Rectangle bounds)
        {
            Text = text;
            Score = score;
            Bounds = bounds;
        }

        public string Text { get; }

        public float Score { get; }

        public Rectangle Bounds { get; }
    }
}
