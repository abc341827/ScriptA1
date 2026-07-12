using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System.Drawing;

namespace WinFormsApp1
{
    public sealed class OnnxDbTextDetector : IDisposable
    {
        private const int Stride = 32;
        private const float BinaryThreshold = 0.2f;
        private const float BoxThreshold = 0.45f;
        private readonly InferenceSession _session;

        public OnnxDbTextDetector(string modelPath, out string provider)
        {
            _session = OnnxOcrSessionFactory.CreateSession(modelPath, out provider);
        }

        public IReadOnlyList<OnnxDetectedBox> Detect(Mat source)
        {
            if (source.Empty())
            {
                return Array.Empty<OnnxDetectedBox>();
            }

            using var bgr = EnsureBgr(source);
            var inputWidth = RoundUp(Math.Max(Stride, bgr.Width), Stride);
            var inputHeight = RoundUp(Math.Max(Stride, bgr.Height), Stride);
            using var resized = new Mat();
            Cv2.Resize(bgr, resized, new OpenCvSharp.Size(inputWidth, inputHeight));

            var input = ToTensor(resized);
            using var results = _session.Run(new[] { NamedOnnxValue.CreateFromTensor("x", input) });
            var output = results.First().AsTensor<float>();
            var dimensions = output.Dimensions.ToArray();
            var mapHeight = dimensions[^2];
            var mapWidth = dimensions[^1];
            var values = output.ToArray();

            using var probability = Mat.FromPixelData(mapHeight, mapWidth, MatType.CV_32FC1, values);
            using var probabilityResized = new Mat();
            Cv2.Resize(probability, probabilityResized, new OpenCvSharp.Size(inputWidth, inputHeight));

            using var mask = new Mat();
            Cv2.Threshold(probabilityResized, mask, BinaryThreshold, 255, ThresholdTypes.Binary);
            mask.ConvertTo(mask, MatType.CV_8UC1);

            Cv2.FindContours(mask, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            var scaleX = bgr.Width / (float)inputWidth;
            var scaleY = bgr.Height / (float)inputHeight;
            var boxes = new List<OnnxDetectedBox>();

            foreach (var contour in contours)
            {
                if (contour.Length < 3)
                {
                    continue;
                }

                var rect = Cv2.BoundingRect(contour);
                if (rect.Width < 3 || rect.Height < 3)
                {
                    continue;
                }

                var score = ScoreBox(probabilityResized, rect);
                if (score < BoxThreshold)
                {
                    continue;
                }

                var x = Math.Clamp((int)MathF.Floor(rect.X * scaleX), 0, Math.Max(0, bgr.Width - 1));
                var y = Math.Clamp((int)MathF.Floor(rect.Y * scaleY), 0, Math.Max(0, bgr.Height - 1));
                var right = Math.Clamp((int)MathF.Ceiling((rect.X + rect.Width) * scaleX), x + 1, bgr.Width);
                var bottom = Math.Clamp((int)MathF.Ceiling((rect.Y + rect.Height) * scaleY), y + 1, bgr.Height);
                boxes.Add(new OnnxDetectedBox(new Rectangle(x, y, right - x, bottom - y), score));
            }

            return boxes
                .OrderBy(box => box.Bounds.Top)
                .ThenBy(box => box.Bounds.Left)
                .ToList();
        }

        public void Dispose()
        {
            _session.Dispose();
        }

        private static Mat EnsureBgr(Mat source)
        {
            if (source.Channels() == 3)
            {
                return source.Clone();
            }

            var converted = new Mat();
            if (source.Channels() == 4)
            {
                Cv2.CvtColor(source, converted, ColorConversionCodes.BGRA2BGR);
            }
            else
            {
                Cv2.CvtColor(source, converted, ColorConversionCodes.GRAY2BGR);
            }

            return converted;
        }

        private static DenseTensor<float> ToTensor(Mat image)
        {
            var tensor = new DenseTensor<float>(new[] { 1, 3, image.Height, image.Width });
            var indexer = image.GetGenericIndexer<Vec3b>();
            var mean = new[] { 0.485f, 0.456f, 0.406f };
            var std = new[] { 0.229f, 0.224f, 0.225f };

            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    var pixel = indexer[y, x];
                    tensor[0, 0, y, x] = (pixel.Item0 / 255f - mean[0]) / std[0];
                    tensor[0, 1, y, x] = (pixel.Item1 / 255f - mean[1]) / std[1];
                    tensor[0, 2, y, x] = (pixel.Item2 / 255f - mean[2]) / std[2];
                }
            }

            return tensor;
        }

        private static float ScoreBox(Mat probability, Rect rect)
        {
            using var roi = new Mat(probability, rect);
            return (float)Cv2.Mean(roi).Val0;
        }

        private static int RoundUp(int value, int multiple)
        {
            return ((value + multiple - 1) / multiple) * multiple;
        }
    }
}
