namespace WinFormsApp1
{
    public class AppSettings
    {
        public OcrSettings Ocr { get; set; } = new OcrSettings();
    }

    public class OcrSettings
    {
        public string DefaultProfile { get; set; } = "models/paddleocr-v6-small/ocr-profile.json";
    }
}
