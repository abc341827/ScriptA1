using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using System.Text;

namespace WinFormsApp1
{
    public static class PaddleOcrFactory
    {
        public static PaddleOcrAll CreateDefaultChineseV5()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var profile = OcrProfileLoader.LoadDefault();
            var modelVersion = ParseModelVersion(profile.ModelVersion);
            var detectionModelDirectory = profile.ResolvePath(profile.DetectionModel);
            var recognitionModelDirectory = profile.ResolvePath(profile.RecognitionModel);
            EnsureModelDirectory(detectionModelDirectory, nameof(profile.DetectionModel), requireYaml: true);
            EnsureModelDirectory(recognitionModelDirectory, nameof(profile.RecognitionModel), requireYaml: true);

            var detectionModel = DetectionModel.FromDirectory(detectionModelDirectory, modelVersion);
            var recognitionModel = RecognizationModel.FromDirectory(
                recognitionModelDirectory,
                profile.ResolveOptionalPath(profile.Dictionary),
                modelVersion);

            FullOcrModel fullModel;
            if (string.IsNullOrWhiteSpace(profile.ClassificationModel))
            {
                fullModel = new FullOcrModel(detectionModel, recognitionModel);
            }
            else
            {
                var classificationModelDirectory = profile.ResolvePath(profile.ClassificationModel);
                EnsureModelDirectory(classificationModelDirectory, nameof(profile.ClassificationModel), requireYaml: false);
                var classificationModel = ClassificationModel.FromDirectory(classificationModelDirectory);
                fullModel = new FullOcrModel(detectionModel, classificationModel, recognitionModel);
            }

            var ocr = new PaddleOcrAll(fullModel);

            ocr.AllowRotateDetection = profile.AllowRotateDetection;
            ocr.Enable180Classification = profile.Enable180Classification;
            return ocr;
        }

        private static ModelVersion ParseModelVersion(string modelVersion)
        {
            return Enum.TryParse<ModelVersion>(modelVersion, ignoreCase: true, out var parsed)
                ? parsed
                : ModelVersion.V5;
        }

        private static void EnsureModelDirectory(string directory, string profilePropertyName, bool requireYaml)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"OCR profile {profilePropertyName} directory not found: {directory}");
            }

            if (requireYaml && !File.Exists(Path.Combine(directory, "inference.yml")))
            {
                throw new FileNotFoundException($"inference.yml not found in OCR profile {profilePropertyName} directory: {directory}");
            }
        }
    }
}
