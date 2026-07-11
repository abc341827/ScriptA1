using System.Text;

namespace WinFormsApp1
{
    public class FileRecordWriter : IRecordWriter
    {
        private readonly object _syncRoot = new object();

        public void WriteLine(string category, string message)
        {
            try
            {
                var timestamp = DateTime.Now;
                var filePath = RecordPaths.GetDailyRecordFile(SanitizeCategory(category), timestamp);
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var line = $"[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}";
                lock (_syncRoot)
                {
                    File.AppendAllText(filePath, line, Encoding.UTF8);
                }
            }
            catch
            {
            }
        }

        private static string SanitizeCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return "default";
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(category.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            return string.IsNullOrWhiteSpace(sanitized) ? "default" : sanitized;
        }
    }
}
