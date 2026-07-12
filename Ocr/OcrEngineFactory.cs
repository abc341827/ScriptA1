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

            var useDirectMl = engine.Equals("OnnxDirectML", StringComparison.OrdinalIgnoreCase) ||
                engine.Equals("Onnx", StringComparison.OrdinalIgnoreCase);
            if (!useDirectMl && !engine.Equals("OnnxCpu", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Unsupported OCR engine '{engine}'. Supported values: OnnxDirectML, OnnxCpu.");
            }

            var modelRoot = FindDirectoryFromBaseDirectory(
                settings?.Ocr?.OnnxModelRoot ?? DefaultOnnxModelRoot,
                AppContext.BaseDirectory);
            if (string.IsNullOrWhiteSpace(modelRoot))
            {
                throw new DirectoryNotFoundException($"ONNX OCR model directory not found: {settings?.Ocr?.OnnxModelRoot ?? DefaultOnnxModelRoot}");
            }

            var deviceId = Math.Max(0, settings?.Ocr?.DirectMlDeviceId ?? 0);
            var onnx = new OnnxMarketOcrEngine(modelRoot, deviceId, useDirectMl, out var providers);
            runtimeDescription = $"ONNX Runtime ({providers})";
            return onnx;
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
