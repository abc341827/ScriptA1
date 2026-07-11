namespace WinFormsApp1
{
    public static class RecordPaths
    {
        public static string RootDirectory => Path.Combine(AppContext.BaseDirectory, "records");

        public static string GetDailyRecordFile(string category, DateTime timestamp)
        {
            var categoryDirectory = Path.Combine(RootDirectory, category);
            return Path.Combine(categoryDirectory, $"{timestamp:yyyy-MM-dd}.txt");
        }
    }
}
