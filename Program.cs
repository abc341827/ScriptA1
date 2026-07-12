using Microsoft.Extensions.DependencyInjection;

namespace WinFormsApp1
{

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            using var serviceProvider = ConfigureServices();
            Application.Run(serviceProvider.GetRequiredService<LaunchForm>());
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IRecordWriter, FileRecordWriter>();
            services.AddTransient<IInputController, Win32InputController>();
            services.AddTransient<LaunchForm>();
            services.AddTransient<Form1>();
            services.AddTransient<Form2>();

            return services.BuildServiceProvider();
        }
    }
}