using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using System.Text;

namespace WinFormsApp1
{
    public static class PaddleOcrFactory
    {
        public static PaddleOcrAll CreateDefaultChineseV5(Action<Exception>? gpuFallbackHandler = null)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var modelPaths = OcrModelPaths.FromBaseDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var ocr = new PaddleOcrAll(
                new FullOcrModel(
                    DetectionModel.FromDirectory(modelPaths.DetectionDirectory, ModelVersion.V5),
                    ClassificationModel.FromDirectory(modelPaths.ClassificationDirectory),
                    RecognizationModel.FromDirectory(modelPaths.RecognitionDirectory, modelPaths.RecognitionLabelPath, ModelVersion.V5)),
                device =>
                {
                    try
                    {
                        device.EnableUseGpu(500, 0);
                        var useGpu = device.UseGpu;
                    }
                    catch (Exception ex) when (gpuFallbackHandler != null)
                    {
                        gpuFallbackHandler(ex);
                        device.UseGpu = false;
                    }
                });

            ocr.AllowRotateDetection = false;
            ocr.Enable180Classification = false;
            return ocr;
        }
    }
}
