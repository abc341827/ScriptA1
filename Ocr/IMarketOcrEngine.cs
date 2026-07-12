using OpenCvSharp;

namespace WinFormsApp1
{
    public interface IMarketOcrEngine : IDisposable
    {
        IReadOnlyList<OcrTextLine> Recognize(Mat image);
    }
}
