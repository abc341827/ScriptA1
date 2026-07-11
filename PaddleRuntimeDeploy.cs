using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    using Sdcb.PaddleInference;
    using System.Reflection.Metadata;
    using System.Runtime.InteropServices;

    public class PaddleRuntimeDeploy
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);
        // 注入 PATH 的核心
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        public static bool ConfigureRuntime(out string selectedRuntime, out string error)
        {
            selectedRuntime = null;
            error = null;
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string runtimeRoot = Path.Combine(baseDir, "runtimes");

                // === 1. 检测显卡算力 ===
                string computeCapability = DetectComputeCapability();
                Console.WriteLine($"检测到显卡算力: {computeCapability ?? "无"}");

                // === 2. 确定要加载的 sm 文件夹 ===
                string smFolder = null;
                if (computeCapability == "6.1") smFolder = "cu118_cudnn89_sm61";   // GTX 1060
                else if (computeCapability == "7.5") smFolder = "cu118_cudnn89_sm75"; // RTX 20
                else if (computeCapability == "8.6") smFolder = "cu118_cudnn89_sm86"; // RTX 30
                else if (computeCapability == "8.9") smFolder = "cu126_cudnn95_sm89"; // RTX 40
                else if(computeCapability == "12.0") smFolder = "cu129_cudnn910_sm120"; // RTX 50
                else
                {
                    // 无显卡或未知显卡，回退 CPU
                    selectedRuntime = "cpu";
                    error = "未检测到支持的 NVIDIA 显卡，将使用 CPU 模式";
                    return false;
                }

                string smFullPath = Path.Combine(runtimeRoot, smFolder);
                if (!Directory.Exists(smFullPath))
                {
                    error = $"缺少运行时文件：{smFolder}，请联系开发商";
                    return false;
                }

                // === 3. PATH 注入：顺序至关重要 ===
                // 先把 cuDNN 公共目录加进去，再把 sm 特定目录加进去
                string cudnnCommonPath = Path.Combine(runtimeRoot, "cu118_cudnn_common", "bin");
                string currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";

                // 新 PATH = cuDNN目录 + sm目录 + 原PATH
                string newPath = $"{cudnnCommonPath};{smFullPath};{currentPath}";
                Environment.SetEnvironmentVariable("PATH", newPath);

                // Windows 下强力手段：SetDllDirectory 直接指定搜索目录
                SetDllDirectory(cudnnCommonPath);
                SetDllDirectory(smFullPath);  // 多次调用有效，后添加的先搜索
                var handle = LoadLibrary("cudnn64_9.dll");
                Console.WriteLine($"PATH注入后: {(handle != IntPtr.Zero ? "成功" : "失败")}");
                selectedRuntime = smFolder;
                Console.WriteLine($"已加载运行时: {smFolder}");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 调用 nvidia-smi 获取算力
        /// </summary>
        static string DetectComputeCapability()
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo("nvidia-smi",
                    "--query-gpu=compute_cap --format=csv,noheader")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = System.Diagnostics.Process.Start(psi);
                if (p != null)
                {
                    string output = p.StandardOutput.ReadToEnd().Trim();
                    p.WaitForExit(500);
                    if (!string.IsNullOrEmpty(output))
                    {
                        // 输出格式如 "8.9"
                        return output.Split(',')[0].Trim();
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
