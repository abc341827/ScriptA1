using Microsoft.ML.OnnxRuntime;

namespace WinFormsApp1
{
    public static class OnnxOcrSessionFactory
    {
        public static InferenceSession CreateSession(string modelPath, out string provider)
        {
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException($"ONNX model not found: {modelPath}", modelPath);
            }

            try
            {
                using var directMlOptions = new SessionOptions();
                directMlOptions.AppendExecutionProvider_DML(0);
                directMlOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                provider = "DirectML";
                return new InferenceSession(modelPath, directMlOptions);
            }
            catch
            {
                var cpuOptions = new SessionOptions
                {
                    GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
                };

                provider = "CPU";
                return new InferenceSession(modelPath, cpuOptions);
            }
        }
    }
}
