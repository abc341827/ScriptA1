namespace WinFormsApp1
{
    public static class OcrEngineFactory
    {
        private const string AppSettingsRelativePath = "config/appsettings.json";
        private const string DefaultOnnxModelRoot = "models/paddleocr-v6-small-onnx";

        public static IMarketOcrEngine CreateDefault(out string runtimeDescription)
        {
            var settings = LoadSettings();
            var engine = settings?.Ocr?.Engine ?? "OnnxDirectML";

            if (engine.Equals("OnnxDirectML", StringComparison.OrdinalIgnoreCase) ||
                engine.Equals("Onnx", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var modelRoot = FindDirectoryFromBaseDirectory(
                        settings?.Ocr?.OnnxModelRoot ?? DefaultOnnxModelRoot,
                        AppContext.BaseDirectory);
                    if (!string.IsNullOrWhiteSpace(modelRoot))
                    {
                        var onnx = new OnnxMarketOcrEngine(modelRoot, out var providers);
                        runtimeDescription = $"ONNX Runtime ({providers})";
                        return onnx;
                    }
                }
                catch (Exception ex)
                {
                    runtimeDescription = $"ONNX 初始化失败，回退 Paddle：{ex.Message}";
                    return new PaddleMarketOcrEngine(PaddleOcrFactory.CreateDefaultChineseV5());
                }
            }

            runtimeDescription = "Paddle";
            return new PaddleMarketOcrEngine(PaddleOcrFactory.CreateDefaultChineseV5());
        }

        private static AppSettings? LoadSettings()
        {
            var appSettingsPath = FindFileFromBaseDirectory(AppSettingsRelativePath, AppContext.BaseDirectory);
            return string.IsNullOrWhiteSpace(appSettingsPath)
                ? null
                : JsonFileStore.Load<AppSettings>(appSettingsPath);
        }

        private static string? FindFileFromBaseDirectory(string relativePath, string baseDirectory)
        {
            var directory = new DirectoryInfo(baseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, relativePath);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            return null;
        }

        private static string? FindDirectoryFromBaseDirectory(string relativePath, string baseDirectory)
        {
            var directory = new DirectoryInfo(baseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, relativePath);
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            return null;
        }
    }
}
