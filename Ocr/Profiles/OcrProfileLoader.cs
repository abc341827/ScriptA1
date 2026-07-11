namespace WinFormsApp1
{
    public static class OcrProfileLoader
    {
        private const string DefaultProfileRelativePath = "models/paddleocr-v6-small/ocr-profile.json";
        private const string AppSettingsRelativePath = "config/appsettings.json";

        public static OcrProfile LoadDefault()
        {
            var defaultProfilePath = LoadConfiguredDefaultProfilePath();
            var profilePath = FindFileFromBaseDirectory(defaultProfilePath, AppContext.BaseDirectory);
            if (string.IsNullOrWhiteSpace(profilePath))
            {
                throw new FileNotFoundException($"Default OCR profile not found. Expected local model profile: {defaultProfilePath}");
            }

            var profile = JsonFileStore.Load<OcrProfile>(profilePath);
            if (profile == null)
            {
                throw new InvalidOperationException($"Default OCR profile could not be loaded: {profilePath}");
            }

            profile.RootDirectory = Path.GetDirectoryName(profilePath) ?? string.Empty;
            return profile;
        }

        private static string LoadConfiguredDefaultProfilePath()
        {
            var appSettingsPath = FindFileFromBaseDirectory(AppSettingsRelativePath, AppContext.BaseDirectory);
            if (string.IsNullOrWhiteSpace(appSettingsPath))
            {
                return DefaultProfileRelativePath;
            }

            var settings = JsonFileStore.Load<AppSettings>(appSettingsPath);
            if (string.IsNullOrWhiteSpace(settings?.Ocr?.DefaultProfile))
            {
                return DefaultProfileRelativePath;
            }

            return settings.Ocr.DefaultProfile;
        }

        private static string? FindFileFromBaseDirectory(string relativePath, string baseDirectory)
        {
            var directory = new DirectoryInfo(baseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, relativePath);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            return null;
        }
    }
}
