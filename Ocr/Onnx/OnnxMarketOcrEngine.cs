using OpenCvSharp;

namespace WinFormsApp1
{
    public sealed class OnnxMarketOcrEngine : IMarketOcrEngine
    {
        private readonly OnnxDbTextDetector _detector;
        private readonly OnnxTextRecognizer _recognizer;

        public OnnxMarketOcrEngine(string modelRoot, out string providerSummary)
        {
            var detModelPath = Path.Combine(modelRoot, "det", "inference.onnx");
            var recModelPath = Path.Combine(modelRoot, "rec", "inference.onnx");
            var dictionaryPath = Path.Combine(modelRoot, "dict.txt");

            _detector = new OnnxDbTextDetector(detModelPath, out var detProvider);
            _recognizer = new OnnxTextRecognizer(recModelPath, dictionaryPath, out var recProvider);
            providerSummary = $"det={detProvider}, rec={recProvider}";
        }

        public IReadOnlyList<OcrTextLine> Recognize(Mat image)
        {
            var boxes = _detector.Detect(image);
            var lines = new List<OcrTextLine>(boxes.Count);
            foreach (var box in boxes)
            {
                var line = _recognizer.Recognize(image, box);
                if (!string.IsNullOrWhiteSpace(line.Text))
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        public void Dispose()
        {
            _recognizer.Dispose();
            _detector.Dispose();
        }
    }
}
