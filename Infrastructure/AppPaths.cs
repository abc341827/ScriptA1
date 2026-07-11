namespace WinFormsApp1
{
    public static class AppPaths
    {
        public static string AppDataDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinFormsApp1");

        public static string CubeAppStateFile => Path.Combine(AppDataDirectory, "appstate.json");

        public static string MarketListBoxItemsFile => Path.Combine(AppDataDirectory, "listbox_items.json");
    }
}
