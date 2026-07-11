using System.Text.Json;

namespace WinFormsApp1
{
    public static class JsonFileStore
    {
        private static readonly JsonSerializerOptions LoadOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public static T? Load<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return default;
            }

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json, LoadOptions);
        }

        public static void Save<T>(string filePath, T value, bool writeIndented = false)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = writeIndented });
            File.WriteAllText(filePath, json);
        }
    }
}
