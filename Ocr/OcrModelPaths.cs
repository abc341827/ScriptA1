namespace WinFormsApp1
{
    public class OcrModelPaths
    {
        public string DetectionDirectory { get; init; }

        public string ClassificationDirectory { get; init; }

        public string RecognitionDirectory { get; init; }

        public string RecognitionLabelPath { get; init; } = string.Empty;

        public static OcrModelPaths FromBaseDirectory(string baseDirectory)
        {
            var inferenceDirectory = Path.Combine(baseDirectory, "inference");

            return new OcrModelPaths
            {
                DetectionDirectory = Path.Combine(inferenceDirectory, "PP-OCRv5_mobile_det_infer"),
                ClassificationDirectory = Path.Combine(inferenceDirectory, "ch_ppocr_mobile_v2.0_cls_infer"),
                RecognitionDirectory = Path.Combine(inferenceDirectory, "PP-OCRv5_mobile_rec_infer"),
                RecognitionLabelPath = string.Empty
            };
        }
    }
}
