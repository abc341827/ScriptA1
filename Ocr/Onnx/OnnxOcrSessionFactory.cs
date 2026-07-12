using Microsoft.ML.OnnxRuntime;

namespace WinFormsApp1
{
    public static class OnnxOcrSessionFactory
    {
        public static InferenceSession CreateSession(string modelPath, int deviceId, bool useDirectMl, out string provider)
        {
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException($"ONNX model not found: {modelPath}", modelPath);
            }

            if (useDirectMl)
            {
                try
                {
                    using var directMlOptions = new SessionOptions();
                    directMlOptions.AppendExecutionProvider_DML(deviceId);
                    directMlOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                    directMlOptions.ExecutionMode = ExecutionMode.ORT_SEQUENTIAL;
                    directMlOptions.EnableMemoryPattern = false;
                    provider = $"DirectML(deviceId={deviceId})";
                    return new InferenceSession(modelPath, directMlOptions);
                }
                catch
                {
                }
            }

            var cpuOptions = new SessionOptions
            {
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
            };

            provider = "CPU";
            return new InferenceSession(modelPath, cpuOptions);
        }
    }
}
