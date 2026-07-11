using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;


namespace WinFormsApp1
{

    public class GameWindowCapture
    {
        // Windows API 函数声明
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest,
            int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        // 常量
        private const int SRCCOPY = 0x00CC0020;

        // 委托和回调
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // 结构体
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;
            public int Height => Bottom - Top;
            public Rectangle ToRectangle() => new Rectangle(Left, Top, Width, Height);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }

        // 窗口信息类
        public class WindowInfo
        {
            public IntPtr Handle { get; set; }
            public string Title { get; set; }
            public string ProcessName { get; set; }
            public int ProcessId { get; set; }
            public Rectangle Bounds { get; set; }
            public bool IsVisible { get; set; }
            public bool IsMainWindow { get; set; }

            public override string ToString()
            {
                return $"进程: {ProcessName} (PID: {ProcessId}), 窗口: \"{Title}\", 位置: {Bounds.X},{Bounds.Y}, 大小: {Bounds.Width}x{Bounds.Height}";
            }
        }

        // 截图区域类型枚举
        public enum CaptureAreaType
        {
            Window,         // 整个窗口
            Client,         // 客户区
            Custom          // 自定义区域
        }

        // ==================== 进程查找方法 ====================

        // 方法1：通过进程名查找主窗口（最常用）
        public static List<WindowInfo> FindWindowByProcessName(string processName, bool exactMatch = true)
        {
            try
            {
                Process[] processes;
                List<WindowInfo> result = new List<WindowInfo>();
                if (exactMatch)
                {
                    // 精确匹配进程名
                    processes = Process.GetProcessesByName(processName);
                }
                else
                {
                    // 模糊匹配进程名
                    processes = Process.GetProcesses()
                        .Where(p => p.ProcessName.Contains(processName, StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                }

                if (processes.Length == 0)
                    return null;

                // 优先使用主窗口句柄不为零的进程
                foreach (var process in processes)
                {
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        result.Add(CreateWindowInfoFromProcess(process));
                    }
                }

                // 如果没有进程有主窗口，返回第一个进程
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查找进程 '{processName}' 时出错: {ex.Message}");
                return null;
            }
        }

        // 方法2：通过进程名查找所有相关窗口
        public static List<WindowInfo> FindAllWindowsByProcessName(string processName)
        {
            var results = new List<WindowInfo>();

            try
            {
                // 获取所有匹配的进程
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0)
                    return results;

                // 为每个进程获取窗口信息
                foreach (var process in processes)
                {
                    var windows = GetWindowsByProcessId(process.Id);
                    results.AddRange(windows);
                }

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查找进程 '{processName}' 的所有窗口时出错: {ex.Message}");
                return results;
            }
        }

        // 方法3：通过进程ID查找窗口
        public static List<WindowInfo> GetWindowsByProcessId(int processId)
        {
            var windows = new List<WindowInfo>();

            // 使用EnumWindows枚举所有窗口，然后筛选出属于指定进程的窗口
            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                uint windowPid;
                GetWindowThreadProcessId(hWnd, out windowPid);

                if (windowPid == processId && IsWindowVisible(hWnd))
                {
                    var info = CreateWindowInfoFromHandle(hWnd, processId);
                    if (info != null)
                    {
                        windows.Add(info);
                    }
                }

                return true; // 继续枚举
            }, IntPtr.Zero);

            return windows;
        }

        // 方法4：查找所有正在运行的游戏的窗口（通过常见游戏进程名）
        public static List<WindowInfo> FindGameWindows()
        {
            var gameWindows = new List<WindowInfo>();
            var commonGameProcessNames = new List<string>
        {
            // 常见游戏进程名（可根据需要扩展）
            "dota2", "csgo", "overwatch", "valorant", "leagueoflegends", "lol",
            "fortnite", "pubg", "apex", "minecraft", "wow", "destiny2",
            "eldenring", "cyberpunk2077", "gta5", "gtaiv", "skyrim", "fallout",
            "steam", "battle.net", "origin", "epicgameslauncher"
        };

            foreach (var processName in commonGameProcessNames)
            {
                try
                {
                    var windows = FindAllWindowsByProcessName(processName);
                    gameWindows.AddRange(windows);
                }
                catch
                {
                    // 忽略不存在的进程
                }
            }

            return gameWindows;
        }

        // 方法5：智能查找游戏窗口（自动检测可能的游戏）
        public static WindowInfo FindGameWindowSmart(string preferredProcessName = null)
        {
            // 1. 如果指定了优先的进程名，先尝试它
            if (!string.IsNullOrEmpty(preferredProcessName))
            {
                var preferredWindow = FindWindowByProcessName(preferredProcessName, false).FirstOrDefault();
                if (preferredWindow != null)
                    return preferredWindow;
            }

            // 2. 查找所有可能的游戏窗口
            var allGameWindows = FindGameWindows();

            // 3. 选择最可能的主游戏窗口（最大的可见窗口）
            if (allGameWindows.Count > 0)
            {
                return allGameWindows
                    .Where(w => w.IsVisible)
                    .OrderByDescending(w => w.Bounds.Width * w.Bounds.Height)
                    .FirstOrDefault();
            }

            return null;
        }

        // ==================== 辅助方法 ====================

        private static WindowInfo CreateWindowInfoFromProcess(Process process)
        {
            try
            {
                var info = new WindowInfo
                {
                    Handle = process.MainWindowHandle,
                    ProcessName = process.ProcessName,
                    ProcessId = process.Id,
                    IsMainWindow = true
                };

                // 获取窗口标题
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    info.Title = GetWindowTitle(process.MainWindowHandle);
                    info.Bounds = GetWindowBounds(process.MainWindowHandle);
                    info.IsVisible = IsWindowVisible(process.MainWindowHandle);
                }

                return info;
            }
            catch
            {
                return null;
            }
        }

        private static WindowInfo CreateWindowInfoFromHandle(IntPtr hWnd, int processId)
        {
            try
            {
                Process process = Process.GetProcessById(processId);

                return new WindowInfo
                {
                    Handle = hWnd,
                    Title = GetWindowTitle(hWnd),
                    ProcessName = process.ProcessName,
                    ProcessId = processId,
                    Bounds = GetWindowBounds(hWnd),
                    IsVisible = IsWindowVisible(hWnd),
                    IsMainWindow = (process.MainWindowHandle == hWnd)
                };
            }
            catch
            {
                return null;
            }
        }

        private static string GetWindowTitle(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            if (length == 0) return string.Empty;

            var title = new System.Text.StringBuilder(length + 1);
            GetWindowText(hWnd, title, title.Capacity);
            return title.ToString();
        }

        // ==================== 窗口操作和截图方法 ====================

        // 获取窗口位置和大小
        public static Rectangle GetWindowBounds(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("无效的窗口句柄");

            GetWindowRect(hWnd, out RECT rect);
            return rect.ToRectangle();
        }

        // 获取客户区位置和大小（去掉边框）
        public static Rectangle GetClientBounds(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("无效的窗口句柄");

            GetClientRect(hWnd, out RECT rect);

            // 将客户区坐标转换为屏幕坐标
            Point point = new Point { X = rect.Left, Y = rect.Top };
            ClientToScreen(hWnd, ref point);

            return new Rectangle(point.X, point.Y, rect.Width, rect.Height);
        }

        // 核心截图方法
        public static Bitmap CaptureArea(IntPtr hWnd, CaptureAreaType areaType, Rectangle customArea)
        {
            Rectangle sourceRect;
            Rectangle targetRect;

            switch (areaType)
            {
                case CaptureAreaType.Window:
                    sourceRect = GetWindowBounds(hWnd);
                    targetRect = new Rectangle(0, 0, sourceRect.Width, sourceRect.Height);
                    break;

                case CaptureAreaType.Client:
                    sourceRect = GetClientBounds(hWnd);
                    targetRect = new Rectangle(0, 0, sourceRect.Width, sourceRect.Height);
                    break;

                case CaptureAreaType.Custom:
                    if (customArea.IsEmpty)
                        throw new ArgumentException("自定义区域不能为空");

                    Rectangle clientBounds = GetClientBounds(hWnd);
                    sourceRect = new Rectangle(
                        clientBounds.X + customArea.X,
                        clientBounds.Y + customArea.Y,
                        customArea.Width,
                        customArea.Height
                    );
                    targetRect = new Rectangle(0, 0, customArea.Width, customArea.Height);
                    break;

                default:
                    throw new ArgumentException("不支持的截图区域类型");
            }

            Bitmap screenshot = new Bitmap(targetRect.Width, targetRect.Height, PixelFormat.Format32bppArgb);

            using (Graphics gfx = Graphics.FromImage(screenshot))
            {
                IntPtr hdcBitmap = gfx.GetHdc();
                IntPtr hdcWindow = GetWindowDC(hWnd);

                Rectangle windowBounds = GetWindowBounds(hWnd);
                int offsetX = sourceRect.X - windowBounds.X;
                int offsetY = sourceRect.Y - windowBounds.Y;

                bool success = BitBlt(hdcBitmap, 0, 0, targetRect.Width, targetRect.Height,
                                     hdcWindow, offsetX, offsetY, SRCCOPY);

                gfx.ReleaseHdc(hdcBitmap);
                ReleaseDC(hWnd, hdcWindow);

                if (!success)
                    throw new Exception("截图失败");
            }

            return screenshot;
        }

        // 截取窗口中的指定区域
        public static Bitmap CaptureWindowArea(IntPtr hWnd, int x, int y, int width, int height)
        {
            return CaptureArea(hWnd, CaptureAreaType.Custom, new Rectangle(x, y, width, height));
        }

        // 截取客户区中的指定区域
        public static Bitmap CaptureClientArea(IntPtr hWnd, int x, int y, int width, int height)
        {
            Rectangle clientBounds = GetClientBounds(hWnd);
            Rectangle windowBounds = GetWindowBounds(hWnd);

            int relativeX = x + (clientBounds.X - windowBounds.X);
            int relativeY = y + (clientBounds.Y - windowBounds.Y);

            return CaptureArea(hWnd, CaptureAreaType.Custom, new Rectangle(relativeX, relativeY, width, height));
        }

        // 截取整个窗口
        public static Bitmap CaptureWindow(IntPtr hWnd)
        {
            return CaptureArea(hWnd, CaptureAreaType.Window, Rectangle.Empty);
        }

        // 截取客户区
        public static Bitmap CaptureClientArea(IntPtr hWnd)
        {
            return CaptureArea(hWnd, CaptureAreaType.Client, Rectangle.Empty);
        }
    }

    // 扩展工具类：提供更便捷的查找和操作功能
    public static class GameWindowHelper
    {
        // 获取当前运行的所有进程列表
        public static List<ProcessInfo> GetAllProcesses(bool includeSystemProcesses = false)
        {
            var processes = Process.GetProcesses();
            var result = new List<ProcessInfo>();

            foreach (var process in processes)
            {
                try
                {
                    // 跳过系统进程（如果需要）
                    if (!includeSystemProcesses && IsSystemProcess(process))
                        continue;

                    var info = new ProcessInfo
                    {
                        Id = process.Id,
                        Name = process.ProcessName,
                        MainWindowTitle = process.MainWindowTitle,
                        HasWindow = (process.MainWindowHandle != IntPtr.Zero),
                        MemoryUsage = process.WorkingSet64
                    };

                    result.Add(info);
                }
                catch
                {
                    // 忽略无法访问的进程
                }
            }

            return result.OrderBy(p => p.Name).ToList();
        }

        // 查找可能包含游戏的进程
        public static List<ProcessInfo> FindPotentialGameProcesses()
        {
            var allProcesses = GetAllProcesses();

            // 常见的游戏进程特征
            var gameKeywords = new[]
            {
            "game", "launcher", "client", "dota", "cs", "overwatch",
            "valorant", "league", "fortnite", "pubg", "apex", "minecraft",
            "steam", "battle.net", "origin", "epic", "ubisoft", "bethesda"
        };

            return allProcesses
                .Where(p => p.HasWindow && !string.IsNullOrEmpty(p.MainWindowTitle))
                .Where(p => gameKeywords.Any(kw =>
                    p.Name.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    p.MainWindowTitle.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        // 判断是否为系统进程
        private static bool IsSystemProcess(Process process)
        {
            try
            {
                return process.ProcessName.StartsWith("svchost", StringComparison.OrdinalIgnoreCase) ||
                       process.ProcessName.StartsWith("csrss", StringComparison.OrdinalIgnoreCase) ||
                       process.ProcessName.StartsWith("lsass", StringComparison.OrdinalIgnoreCase) ||
                       process.ProcessName.StartsWith("wininit", StringComparison.OrdinalIgnoreCase) ||
                       process.ProcessName.StartsWith("services", StringComparison.OrdinalIgnoreCase) ||
                       process.ProcessName.StartsWith("smss", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }

    // 进程信息类
    public class ProcessInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MainWindowTitle { get; set; }
        public bool HasWindow { get; set; }
        public long MemoryUsage { get; set; }

        public override string ToString()
        {
            return $"[PID: {Id}] {Name} - \"{MainWindowTitle}\" (内存: {MemoryUsage / 1024 / 1024}MB)";
        }
    }

    // 增强版：支持更复杂的截图配置
    public class AdvancedWindowCapture
    {

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);
        // Windows API声明
        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest,
            int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // 常量
        private const int SRCCOPY = 0x00CC0020;
        private const int SW_RESTORE = 9;
        private const uint PW_CLIENTONLY = 0x00000001;
        private const uint PW_RENDERFULLCONTENT = 0x00000002; // Windows 8.1+ 可以截取DX内容

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        // 截图方法枚举
        public enum CaptureMethod
        {
            Auto,               // 自动选择最佳方法
            BitBlt,             // 传统BitBlt（适合GDI窗口）
            PrintWindow,        // PrintWindow（适合大多数窗口）
            PrintWindowFull,    // PrintWindow with PW_RENDERFULLCONTENT（适合DX窗口）
            DesktopDuplicate    // 桌面复制（最强大但复杂）
        }

        // 核心截图方法
        public static Bitmap CaptureWindowEx(IntPtr hWnd, CaptureMethod method = CaptureMethod.Auto)
        {
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("无效的窗口句柄");

            // 确保窗口不是最小化的
            if (IsIconic(hWnd))
            {
                ShowWindow(hWnd, SW_RESTORE);
                System.Threading.Thread.Sleep(100); // 等待窗口恢复
            }

            // 获取窗口大小
            GetWindowRect(hWnd, out RECT rect);
            if (rect.Width <= 0 || rect.Height <= 0)
                throw new Exception("窗口大小无效");

            // 根据方法选择截图方式
            Bitmap result = null;

            try
            {
                switch (method)
                {
                    case CaptureMethod.BitBlt:
                        result = CaptureUsingBitBlt(hWnd, rect);
                        break;
                    case CaptureMethod.PrintWindow:
                        result = CaptureUsingPrintWindow(hWnd, rect, 0);
                        break;
                    case CaptureMethod.PrintWindowFull:
                        result = CaptureUsingPrintWindow(hWnd, rect, PW_RENDERFULLCONTENT);
                        break;
                    case CaptureMethod.Auto:
                    default:
                        // 自动选择：先尝试PrintWindow，失败则尝试BitBlt
                        try
                        {
                            result = CaptureUsingPrintWindow(hWnd, rect, PW_RENDERFULLCONTENT);
                        }
                        catch
                        {
                            result = CaptureUsingBitBlt(hWnd, rect);
                        }
                        break;
                }

                // 验证截图是否有效
                if (result != null && IsValidBitmap(result))
                {
                    return result;
                }
                else
                {
                    // 如果自动模式失败，尝试其他方法
                    if (method == CaptureMethod.Auto)
                    {
                        try
                        {
                            result = CaptureUsingPrintWindow(hWnd, rect, 0);
                            if (IsValidBitmap(result)) return result;
                        }
                        catch { }

                        try
                        {
                            result = CaptureUsingBitBlt(hWnd, rect);
                            if (IsValidBitmap(result)) return result;
                        }
                        catch { }

                        throw new Exception("所有截图方法都失败");
                    }
                    else
                    {
                        throw new Exception("截图方法失败");
                    }
                }
            }
            catch (Exception ex)
            {
                if (result != null)
                    result.Dispose();
                throw new Exception($"截图失败: {ex.Message}", ex);
            }
        }

        // 方法1：使用BitBlt（传统方法）
        private static Bitmap CaptureUsingBitBlt(IntPtr hWnd, RECT rect)
        {
            Bitmap screenshot = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);

            using (Graphics gfx = Graphics.FromImage(screenshot))
            {
                IntPtr hdcBitmap = gfx.GetHdc();
                IntPtr hdcWindow = GetWindowDC(hWnd);

                bool success = BitBlt(hdcBitmap, 0, 0, rect.Width, rect.Height,
                                     hdcWindow, 0, 0, SRCCOPY);

                gfx.ReleaseHdc(hdcBitmap);
                ReleaseDC(hWnd, hdcWindow);

                if (!success)
                    throw new Exception("BitBlt操作失败");
            }

            return screenshot;
        }

        // 方法2：使用PrintWindow（更适合现代应用）
        private static Bitmap CaptureUsingPrintWindow(IntPtr hWnd, RECT rect, uint flags)
        {
            // 创建兼容的DC和位图
            IntPtr hdcScreen = GetWindowDC(IntPtr.Zero);
            IntPtr hdcMem = CreateCompatibleDC(hdcScreen);
            IntPtr hBitmap = CreateCompatibleBitmap(hdcScreen, rect.Width, rect.Height);
            IntPtr hOld = SelectObject(hdcMem, hBitmap);

            try
            {
                // 使用PrintWindow截取窗口内容
                bool success = PrintWindow(hWnd, hdcMem, flags);

                if (!success)
                {
                    // 如果失败，尝试不使用标志
                    success = PrintWindow(hWnd, hdcMem, 0);
                }

                if (!success)
                {
                    // 如果仍然失败，可能是窗口不支持PrintWindow
                    throw new Exception("PrintWindow操作失败，窗口可能不支持此方法");
                }

                // 从HBITMAP创建Bitmap
                Bitmap bitmap = Image.FromHbitmap(hBitmap);

                // 清理资源
                SelectObject(hdcMem, hOld);
                DeleteObject(hBitmap);
                DeleteDC(hdcMem);
                ReleaseDC(IntPtr.Zero, hdcScreen);

                return bitmap;
            }
            catch
            {
                // 确保资源被清理
                SelectObject(hdcMem, hOld);
                if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
                if (hdcMem != IntPtr.Zero) DeleteDC(hdcMem);
                ReleaseDC(IntPtr.Zero, hdcScreen);
                throw;
            }
        }

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        // 验证位图是否有效
        private static bool IsValidBitmap(Bitmap bitmap)
        {
            if (bitmap == null) return false;

            // 检查尺寸
            if (bitmap.Width <= 0 || bitmap.Height <= 0)
                return false;

            // 检查是否为全黑（可能是截图失败）
            using (Bitmap small = new Bitmap(bitmap, new Size(16, 16)))
            {
                int blackPixels = 0;
                int whitePixels = 0;

                for (int x = 0; x < small.Width; x++)
                {
                    for (int y = 0; y < small.Height; y++)
                    {
                        Color color = small.GetPixel(x, y);
                        if (color.R < 10 && color.G < 10 && color.B < 10)
                            blackPixels++;
                        if (color.R > 245 && color.G > 245 && color.B > 245)
                            whitePixels++;
                    }
                }

                int totalPixels = small.Width * small.Height;
                float blackRatio = (float)blackPixels / totalPixels;
                float whiteRatio = (float)whitePixels / totalPixels;

                // 如果超过90%是黑色或白色，可能是无效截图
                if (blackRatio > 0.9f || whiteRatio > 0.9f)
                {
                    Console.WriteLine($"警告：截图可能是无效的（黑色比例: {blackRatio:P0}, 白色比例: {whiteRatio:P0})");
                    return false;
                }
            }

            return true;
        }

        // 诊断截图问题
        public static void DiagnoseCaptureIssue(IntPtr hWnd)
        {
            Console.WriteLine("\n=== 截图问题诊断 ===\n");

            if (hWnd == IntPtr.Zero)
            {
                Console.WriteLine("❌ 窗口句柄无效");
                return;
            }

            // 检查窗口状态
            bool isMinimized = IsIconic(hWnd);
            Console.WriteLine($"窗口最小化: {isMinimized}");

            // 获取窗口大小
            GetWindowRect(hWnd, out RECT rect);
            Console.WriteLine($"窗口大小: {rect.Width}x{rect.Height}");

            if (rect.Width <= 0 || rect.Height <= 0)
            {
                Console.WriteLine("❌ 窗口大小为0，可能被隐藏或未准备好");
                return;
            }

            // 测试各种截图方法
            Console.WriteLine("\n测试各种截图方法:");

            string[] methods = { "BitBlt", "PrintWindow", "PrintWindow with PW_RENDERFULLCONTENT" };
            uint[] flags = { 0, 0, PW_RENDERFULLCONTENT };

            for (int i = 0; i < methods.Length; i++)
            {
                try
                {
                    Console.Write($"正在测试 {methods[i]}... ");
                    Bitmap bmp = null;

                    if (i == 0)
                        bmp = CaptureUsingBitBlt(hWnd, rect);
                    else
                        bmp = CaptureUsingPrintWindow(hWnd, rect, flags[i]);

                    if (bmp != null && IsValidBitmap(bmp))
                    {
                        string fileName = $"test_{methods[i].Replace(" ", "_")}.png";
                        bmp.Save(fileName, ImageFormat.Png);
                        Console.WriteLine($"✅ 成功！已保存为 {fileName}");
                        bmp.Dispose();
                    }
                    else
                    {
                        Console.WriteLine($"❌ 截图无效");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ 失败: {ex.Message}");
                }
            }
        }

        // 更强大的截图方法：使用Windows.Graphics.Capture（需要Windows 10 1809+）
        // 注意：这个方法需要引用WinRT API，比较复杂
        public static async Task<Bitmap> CaptureWindowModern(IntPtr hWnd)
        {
            // 这个方法适用于Windows 10 1809+，可以捕获DirectX/OpenGL内容
            // 但是需要处理异步和WinRT API

            // 由于实现较复杂，这里只提供思路：
            // 1. 使用Windows.Graphics.Capture.GraphicsCapturePicker
            // 2. 或者使用Windows.Graphics.Capture.GraphicsCaptureItem.CreateFromWindowId

            throw new NotImplementedException("此方法需要Windows 10 1809+和WinRT API支持");
        }
    }

    public class DirectXWindowCapture
    {
        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);
        // 对于无法使用传统方法截图的游戏，需要使用特殊方法
        // 这里提供一个使用Windows API的特殊方法

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute,
            out RECT pvAttribute, int cbAttribute);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest,
            int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;
        private const int SRCCOPY = 0x00CC0020;
        private const uint PW_RENDERFULLCONTENT = 0x00000002;

        // 方法1：尝试多种方法截图游戏窗口
        public static Bitmap CaptureGameWindow(IntPtr hWnd, RECT rect)
        {
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("无效的窗口句柄");

            // 方法2：使用桌面DC
            try
            {
                return CaptureUsingDesktopDC(hWnd, rect);
            }
            catch
            {
                throw new Exception($"所有游戏截图方法都失败");
            }
        }

        // 使用PrintWindow with PW_RENDERFULLCONTENT（最适合DirectX）
        private static Bitmap CaptureUsingPrintWindowFull(IntPtr hWnd)
        {
            GetWindowRect(hWnd, out RECT rect);

            IntPtr hdcScreen = GetWindowDC(GetDesktopWindow());
            IntPtr hdcMem = CreateCompatibleDC(hdcScreen);
            IntPtr hBitmap = CreateCompatibleBitmap(hdcScreen, rect.Width, rect.Height);
            IntPtr hOld = SelectObject(hdcMem, hBitmap);

            try
            {
                // 使用PW_RENDERFULLCONTENT标志
                bool success = PrintWindow(hWnd, hdcMem, PW_RENDERFULLCONTENT);

                if (!success)
                {
                    throw new Exception("PrintWindow with PW_RENDERFULLCONTENT失败");
                }

                Bitmap bitmap = Image.FromHbitmap(hBitmap);

                SelectObject(hdcMem, hOld);
                DeleteObject(hBitmap);
                DeleteDC(hdcMem);
                ReleaseDC(GetDesktopWindow(), hdcScreen);

                return bitmap;
            }
            catch
            {
                SelectObject(hdcMem, hOld);
                if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
                if (hdcMem != IntPtr.Zero) DeleteDC(hdcMem);
                ReleaseDC(GetDesktopWindow(), hdcScreen);
                throw;
            }
        }

        // 使用桌面DC截取
        private static Bitmap CaptureUsingDesktopDC(IntPtr hWnd, RECT rect)
        {
            GetWindowRect(hWnd, out RECT srcRect);

            {

                IntPtr hdcScreen = GetWindowDC(GetDesktopWindow());
                IntPtr hdcMem = CreateCompatibleDC(hdcScreen);
                IntPtr hBitmap = CreateCompatibleBitmap(hdcScreen, rect.Width, rect.Height);
                IntPtr hOld = SelectObject(hdcMem, hBitmap);


                try
                {
                    bool success = BitBlt(
                        hdcMem, 0, 0, rect.Width, rect.Height,
                        hdcScreen, rect.Left + srcRect.Left, rect.Top + srcRect.Top, 0x00CC0020
                    );

                    if (!success)
                        throw new Exception("BitBlt操作失败");

                    return Image.FromHbitmap(hBitmap);
                }
                finally
                {
                    SelectObject(hdcMem, hOld);
                    DeleteObject(hBitmap);
                    DeleteDC(hdcMem);
                }
            }

            //GetWindowRect(hWnd, out RECT srcRect);
            //IntPtr hdcScreen = GetWindowDC(GetDesktopWindow());
            //IntPtr hdcMem = CreateCompatibleDC(hdcScreen);
            //IntPtr hBitmap = CreateCompatibleBitmap(hdcScreen, rect.Width, rect.Height);
            //IntPtr hOld = SelectObject(hdcMem, hBitmap);

            //try
            //{
            //    // 从桌面DC复制区域
            //    ////bool success = BitBlt(hdcMem, rect.Left, rect.Top, rect.Width, rect.Height,
            //    ////                     hdcScreen, rect.Left, rect.Top, SRCCOPY);
            //    bool success = BitBlt(
            //            hdcMem,              // 目标DC（内存DC）
            //            0, 0,                // 目标起始位置（0,0）
            //            rect.Width, rect.Height,       // 复制尺寸
            //            hdcScreen,           // 源DC（屏幕DC）
            //            rect.Left, rect.Top,                // 源起始位置（指定偏移）
            //            0x00CC0020           // SRCCOPY - 直接复制
            //        );


            //    if (!success)
            //    {
            //        throw new Exception("从桌面DC复制失败");
            //    }

            //    Bitmap bitmap = Image.FromHbitmap(hBitmap);

            //    SelectObject(hdcMem, hOld);
            //    DeleteObject(hBitmap);
            //    DeleteDC(hdcMem);
            //    ReleaseDC(GetDesktopWindow(), hdcScreen);

            //    return bitmap;
            //}
            //catch
            //{
            //    SelectObject(hdcMem, hOld);
            //    if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
            //    if (hdcMem != IntPtr.Zero) DeleteDC(hdcMem);
            //    ReleaseDC(GetDesktopWindow(), hdcScreen);
            //    throw;
            //}
        }

        // 检查是否为DirectX/OpenGL窗口
        public static bool IsDirectXWindow(IntPtr hWnd)
        {
            // 通过窗口类名判断
            StringBuilder className = new StringBuilder(256);
            GetClassName(hWnd, className, className.Capacity);
            string classNameStr = className.ToString();

            // 常见的DirectX/OpenGL窗口类名
            string[] dxWindowClasses =
            {
            "UnityWndClass",       // Unity引擎
            "SDL_app",             // SDL
            "GLUT",                // OpenGL UT
            "D3D",                 // Direct3D
            "Valve001",            // Valve游戏
            "UnrealWindow",        // Unreal引擎
            "CryENGINE"            // CryEngine
        };

            return dxWindowClasses.Any(c => classNameStr.Contains(c, StringComparison.OrdinalIgnoreCase));
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
    }
}
