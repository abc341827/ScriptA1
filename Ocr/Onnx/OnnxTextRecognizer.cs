using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System.Text;

namespace WinFormsApp1
{
    public sealed class OnnxTextRecognizer : IDisposable
    {
        private const int InputHeight = 48;
        private readonly InferenceSession _session;
        private readonly string[] _characters;

        public OnnxTextRecognizer(string modelPath, string dictionaryPath, int deviceId, bool useDirectMl, out string provider)
        {
            _session = OnnxOcrSessionFactory.CreateSession(modelPath, deviceId, useDirectMl, out provider);
            _characters = LoadCharacters(dictionaryPath);
        }

        public OcrTextLine Recognize(Mat source, OnnxDetectedBox box)
        {
            var clipped = ClipRect(box.Bounds, source.Width, source.Height);
            if (clipped.Width <= 0 || clipped.Height <= 0)
            {
                return new OcrTextLine(string.Empty, 0, box.Bounds);
            }

            using var crop = new Mat(source, new Rect(clipped.X, clipped.Y, clipped.Width, clipped.Height));
            using var bgr = EnsureBgr(crop);
            var inputWidth = Math.Max(1, (int)MathF.Ceiling(bgr.Width * (InputHeight / (float)Math.Max(1, bgr.Height))));
            using var resized = new Mat();
            Cv2.Resize(bgr, resized, new OpenCvSharp.Size(inputWidth, InputHeight));

            var input = ToTensor(resized);
            using var results = _session.Run(new[] { NamedOnnxValue.CreateFromTensor("x", input) });
            var output = results.First().AsTensor<float>();
            return Decode(output, box.Bounds);
        }

        public void Dispose()
        {
            _session.Dispose();
        }

        private OcrTextLine Decode(Tensor<float> output, System.Drawing.Rectangle bounds)
        {
            var dimensions = output.Dimensions.ToArray();
            var timeSteps = dimensions[^2];
            var classes = dimensions[^1];
            var text = new StringBuilder();
            var scoreSum = 0f;
            var scoreCount = 0;
            var previousIndex = -1;

            for (var t = 0; t < timeSteps; t++)
            {
                var bestIndex = 0;
                var bestScore = float.MinValue;
                for (var c = 0; c < classes; c++)
                {
                    var score = output[0, t, c];
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIndex = c;
                    }
                }

                if (bestIndex > 0 && bestIndex != previousIndex && bestIndex < _characters.Length)
                {
                    text.Append(_characters[bestIndex]);
                    scoreSum += bestScore;
                    scoreCount++;
                }

                previousIndex = bestIndex;
            }

            var scoreValue = scoreCount == 0 ? 0 : scoreSum / scoreCount;
            return new OcrTextLine(text.ToString(), scoreValue, bounds);
        }

        private static DenseTensor<float> ToTensor(Mat image)
        {
            var tensor = new DenseTensor<float>(new[] { 1, 3, image.Height, image.Width });
            var indexer = image.GetGenericIndexer<Vec3b>();

            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    var pixel = indexer[y, x];
                    tensor[0, 0, y, x] = pixel.Item0 / 127.5f - 1f;
                    tensor[0, 1, y, x] = pixel.Item1 / 127.5f - 1f;
                    tensor[0, 2, y, x] = pixel.Item2 / 127.5f - 1f;
                }
            }

            return tensor;
        }

        private static string[] LoadCharacters(string dictionaryPath)
        {
            if (!File.Exists(dictionaryPath))
            {
                throw new FileNotFoundException($"ONNX OCR dictionary not found: {dictionaryPath}", dictionaryPath);
            }

            var characters = new List<string> { string.Empty };
            characters.AddRange(File.ReadAllLines(dictionaryPath, Encoding.UTF8));
            characters.Add(" ");
            return characters.ToArray();
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

        private static System.Drawing.Rectangle ClipRect(System.Drawing.Rectangle rectangle, int width, int height)
        {
            var x = Math.Clamp(rectangle.X, 0, width);
            var y = Math.Clamp(rectangle.Y, 0, height);
            var right = Math.Clamp(rectangle.Right, x, width);
            var bottom = Math.Clamp(rectangle.Bottom, y, height);
            return new System.Drawing.Rectangle(x, y, right - x, bottom - y);
        }
    }
}
