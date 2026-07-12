using System.Drawing;

namespace WinFormsApp1
{
    public sealed class OnnxDetectedBox
    {
        public OnnxDetectedBox(Rectangle bounds, float score)
        {
            Bounds = bounds;
            Score = score;
        }

        public Rectangle Bounds { get; }

        public float Score { get; }
    }
}
