using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    public class OcrService
    {
        private static readonly HttpClient _http = new HttpClient();
        private readonly string _baseUrl;

        public OcrService(string baseUrl = "http://127.0.0.1:8082")
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        /// <summary>
        /// 同步方法：对传入的 Bitmap 做必要预处理后调用本地 OCR Web 服务并返回原始响应字符串。
        /// 使用 /ocr/stream 上传文件流。此方法在后台线程调用是安全的（会阻塞直到返回）。
        /// </summary>
        public string PerformOCR(Bitmap image)
        {
            if (image == null) return string.Empty;

            // 先做项目中已有的预处理（会返回一张已处理且需释放的 Bitmap）
            try
            {

                // 转为 PNG 字节
                using var ms = new MemoryStream();
                image.Save(ms, ImageFormat.Png);
                var bytes = ms.ToArray();

                // 先尝试使用流接口
                try
                {
                    var b64 = Convert.ToBase64String(bytes);
                    return SendImageBase64Async(b64).GetAwaiter().GetResult();
                    return SendImageStreamAsync(bytes).GetAwaiter().GetResult();
                }
                catch
                {
                    // 如果流接口失败，尝试 base64 接口作为降级
                    var b64 = Convert.ToBase64String(bytes);
                    return SendImageBase64Async(b64).GetAwaiter().GetResult();
                }
            }
            finally
            {
                try { image?.Dispose(); } catch { }
            }
        }

        private async Task<string> SendImageStreamAsync(byte[] imageBytes)
        {
            using var content = new MultipartFormDataContent();
            var bytesContent = new ByteArrayContent(imageBytes);
            bytesContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            content.Add(bytesContent, "file", "img.png");

            var resp = await _http.PostAsync($"{_baseUrl}/ocr/stream", content).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private async Task<string> SendImageBase64Async(string base64)
        {
            var payload = new { base64str = 1 };
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync($"{_baseUrl}/ocr/base64", content).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
    public class NetImageProcessor
    {
        /// <summary>
        /// 使用纯.NET库进行OCR预处理
        /// </summary>
        public static Bitmap PreprocessForOCR(Bitmap original)
        {
            // 1. 调整大小
            Bitmap resized = ResizeImage(original, original.Width * 4, original.Height * 4);

            // 2. 转换为灰度
            Bitmap grayscale = ConvertToGrayscale(resized);

            // 3. 使用内置方法调整对比度
            Bitmap highContrast = AdjustContrast(grayscale, 1.5f);

            // 4. 二值化
            Bitmap binary = Threshold(highContrast, 128);

            // 5. 降噪
            Bitmap denoised = RemoveNoise(binary, 2);

            return denoised;
        }

        private static Bitmap ResizeImage(Bitmap original, int width, int height)
        {
            Bitmap resized = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(original, 0, 0, width, height);
            }
            original.Dispose();
            return resized;
        }

        private static Bitmap ConvertToGrayscale(Bitmap original)
        {
            Bitmap grayscale = new Bitmap(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color pixel = original.GetPixel(x, y);
                    int gray = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                    grayscale.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }

            original.Dispose();
            return grayscale;
        }

        private static Bitmap AdjustContrast(Bitmap image, float contrast)
        {
            // 使用颜色矩阵调整对比度
            float scale = contrast;
            float translate = -0.5f * scale + 0.5f;

            float[][] colorMatrixElements =
            {
            new float[] {scale, 0, 0, 0, 0},
            new float[] {0, scale, 0, 0, 0},
            new float[] {0, 0, scale, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {translate, translate, translate, 0, 1}
        };

            System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix(colorMatrixElements);

            Bitmap adjusted = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(adjusted))
            using (System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes())
            {
                attributes.SetColorMatrix(colorMatrix);
                g.DrawImage(image,
                    new Rectangle(0, 0, image.Width, image.Height),
                    0, 0, image.Width, image.Height,
                    GraphicsUnit.Pixel, attributes);
            }

            image.Dispose();
            return adjusted;
        }

        private static Bitmap Threshold(Bitmap image, int threshold)
        {
            Bitmap binary = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int value = pixel.R > threshold ? 255 : 0;
                    binary.SetPixel(x, y, Color.FromArgb(value, value, value));
                }
            }

            image.Dispose();
            return binary;
        }

        private static Bitmap RemoveNoise(Bitmap image, int radius)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);

            for (int y = radius; y < image.Height - radius; y++)
            {
                for (int x = radius; x < image.Width - radius; x++)
                {
                    int sum = 0;
                    int count = 0;

                    for (int j = -radius; j <= radius; j++)
                    {
                        for (int i = -radius; i <= radius; i++)
                        {
                            Color pixel = image.GetPixel(x + i, y + j);
                            sum += pixel.R;
                            count++;
                        }
                    }

                    int average = sum / count;
                    Color current = image.GetPixel(x, y);

                    // 如果当前像素与平均值差异很大，则认为是噪点
                    if (Math.Abs(current.R - average) > 50)
                    {
                        result.SetPixel(x, y, Color.FromArgb(average, average, average));
                    }
                    else
                    {
                        result.SetPixel(x, y, current);
                    }
                }
            }

            image.Dispose();
            return result;
        }
    }
}
