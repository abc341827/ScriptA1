using OpenCvSharp;
using Sdcb.PaddleOCR;

namespace WinFormsApp1
{
    public sealed class PaddleMarketOcrEngine : IMarketOcrEngine
    {
        private readonly PaddleOcrAll _ocr;

        public PaddleMarketOcrEngine(PaddleOcrAll ocr)
        {
            _ocr = ocr;
        }

        public IReadOnlyList<OcrTextLine> Recognize(Mat image)
        {
            var result = _ocr.Run(image);
            return result.Regions
                .Select(region => new OcrTextLine(region.Text, region.Score, default))
                .ToList();
        }

        public void Dispose()
        {
            _ocr.Dispose();
        }
    }
}
