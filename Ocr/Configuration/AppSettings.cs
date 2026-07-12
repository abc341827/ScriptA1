namespace WinFormsApp1
{
    public class AppSettings
    {
        public OcrSettings Ocr { get; set; } = new OcrSettings();
    }

    public class OcrSettings
    {
        public string Engine { get; set; } = "OnnxDirectML";

        public string OnnxModelRoot { get; set; } = "models/paddleocr-v6-small-onnx";

        public int DirectMlDeviceId { get; set; }
    }
}
