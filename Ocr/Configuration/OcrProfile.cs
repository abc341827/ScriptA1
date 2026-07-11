using System.Text.Json.Serialization;

namespace WinFormsApp1
{
    public class OcrProfile
    {
        public string Name { get; set; } = string.Empty;

        public string Engine { get; set; } = "Paddle";

        public string ModelVersion { get; set; } = "V5";

        public string DetectionModel { get; set; } = string.Empty;

        public string? ClassificationModel { get; set; }

        public string RecognitionModel { get; set; } = string.Empty;

        public string? Dictionary { get; set; }

        public bool AllowRotateDetection { get; set; }

        public bool Enable180Classification { get; set; }

        [JsonIgnore]
        public string RootDirectory { get; set; } = string.Empty;

        public string ResolvePath(string relativeOrAbsolutePath)
        {
            if (Path.IsPathRooted(relativeOrAbsolutePath))
            {
                return relativeOrAbsolutePath;
            }

            return Path.Combine(RootDirectory, relativeOrAbsolutePath);
        }

        public string ResolveOptionalPath(string? relativeOrAbsolutePath)
        {
            if (string.IsNullOrWhiteSpace(relativeOrAbsolutePath))
            {
                return string.Empty;
            }

            return ResolvePath(relativeOrAbsolutePath);
        }
    }
}
