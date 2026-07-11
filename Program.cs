using Sdcb.PaddleInference;
using System.Runtime.InteropServices;
using System.Text;

namespace WinFormsApp1
{

    internal static class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool AllocConsole();


        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);
        private const int STD_OUTPUT_HANDLE = -11;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        [Obsolete]
        static void Main()
        {
            // 1. 先不加任何 PATH，直接尝试加载
            //var handle = LoadLibrary("cudnn64_8.dll");
            //Console.WriteLine($"直接加载: {(handle != IntPtr.Zero ? "成功" : "失败")}");
            //To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            //Environment.SetEnvironmentVariable("GLOG_v", "1");          // 最低级别
            //Environment.SetEnvironmentVariable("CUDA_LAUNCH_BLOCKING", "1");
            //Environment.SetEnvironmentVariable("FLAGS_allocator_strategy", "naive_best_fit");
            ////放在Main开头
            //AllocConsole(); // 弹出一个独立控制台窗口
            //Console.OutputEncoding = Encoding.UTF8;
            //IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            //using (var fs = new FileStream(stdHandle, FileAccess.Write))
            //{
            //    // 4. 创建 StreamWriter，注意编码（常用 UTF8 或 ASCII）
            //    var writer = new StreamWriter(fs, Encoding.UTF8) { AutoFlush = true };

            //    // 5. 重定向 Console.Out
            //    Console.SetOut(writer);

            //    // 6. 可选：同时重定向错误输出（如果也需要）
            //    //Console.SetError(writer);
            //    // 1. 配置运行时（显卡检测 + PATH注入）
            //    if (!PaddleRuntimeDeploy.ConfigureRuntime(out var runtime, out var error))
            //    {

            //        Console.WriteLine($"警告: {error}");
            //        // 继续，会 fallback 到 CPU
            //    }

            //    // 3. 如果是 GPU 运行时，启用 GPU
            //    if (runtime != null && runtime.StartsWith("cu"))
            //    {
            //        Console.WriteLine("GPU 加速已启用");
            //    }
            //    else
            //    {
            //        Console.WriteLine("使用 CPU 模式");
            //    }

            //}
            if (!PaddleRuntimeDeploy.ConfigureRuntime(out var runtime, out var error))
            {

                Console.WriteLine($"警告: {error}");
                // 继续，会 fallback 到 CPU
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new LaunchForm());
        }
    }
}