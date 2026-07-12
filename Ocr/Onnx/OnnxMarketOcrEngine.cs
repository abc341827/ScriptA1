using OpenCvSharp;

namespace WinFormsApp1
{
    public sealed class OnnxMarketOcrEngine : IMarketOcrEngine
    {
        private const int RecognitionCropPadding = 2;
        private readonly OnnxDbTextDetector _detector;
        private readonly OnnxTextRecognizer _recognizer;

        public OnnxMarketOcrEngine(string modelRoot, int deviceId, bool useDirectMl, out string providerSummary)
        {
            var detModelPath = Path.Combine(modelRoot, "det", "inference.onnx");
            var recModelPath = Path.Combine(modelRoot, "rec", "inference.onnx");
            var dictionaryPath = Path.Combine(modelRoot, "dict.txt");

            _detector = new OnnxDbTextDetector(detModelPath, deviceId, useDirectMl, out var detProvider);
            _recognizer = new OnnxTextRecognizer(recModelPath, dictionaryPath, deviceId, useDirectMl, out var recProvider);
            providerSummary = $"det={detProvider}, rec={recProvider}";
        }

        public IReadOnlyList<OcrTextLine> Recognize(Mat image)
        {
            var boxes = _detector.Detect(image);
            var lines = new List<OcrTextLine>(boxes.Count);
            foreach (var box in boxes)
            {
                var paddedBox = new OnnxDetectedBox(
                    Expand(box.Bounds, RecognitionCropPadding, image.Width, image.Height),
                    box.Score);
                var line = _recognizer.Recognize(image, paddedBox);
                if (!string.IsNullOrWhiteSpace(line.Text))
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        private static System.Drawing.Rectangle Expand(System.Drawing.Rectangle rectangle, int padding, int imageWidth, int imageHeight)
        {
            var x = Math.Max(0, rectangle.X - padding);
            var y = Math.Max(0, rectangle.Y - padding);
            var right = Math.Min(imageWidth, rectangle.Right + padding);
            var bottom = Math.Min(imageHeight, rectangle.Bottom + padding);
            return new System.Drawing.Rectangle(x, y, Math.Max(1, right - x), Math.Max(1, bottom - y));
        }

        public void Dispose()
        {
            _recognizer.Dispose();
            _detector.Dispose();
        }
    }
}
