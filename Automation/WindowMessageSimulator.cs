using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms; // 引用 Keys 枚举，方便按键输入

    /// <summary>
    /// 通过 Windows 消息机制模拟后台输入的轻量封装（支持 SendMessage/PostMessage）
    /// 适用于标准 Windows 应用程序（记事本、浏览器、旧软件等），不适用于 DirectX 游戏。
    /// </summary>
    public class WindowMessageSimulator
    {
        #region Windows API 声明


        #endregion

        #region 消息常量

        // 用于将鼠标消息中的 lParam 打包（低16位 x，高16位 y）
        private static IntPtr MakeLParam(int x, int y) => (IntPtr)((y << 16) | (x & 0xFFFF));

        #endregion

        /// <summary>
        /// 查找窗口（通过标题或类名）
        /// </summary>
        /// <param name="windowTitle">窗口标题（支持部分匹配？FindWindow需要完全匹配）</param>
        /// <param name="className">窗口类名（可选）</param>
        /// <returns>窗口句柄，未找到返回 IntPtr.Zero</returns>
        public static IntPtr FindWindowByTitle(string windowTitle, string className = null)
        {
            return Win32API.FindWindow(className, windowTitle);
        }

        /// <summary>
        /// 查找子窗口（例如窗口中的编辑框、按钮）
        /// </summary>
        /// <param name="parentHandle">父窗口句柄</param>
        /// <param name="childClass">子窗口类名（如 "Edit", "Button"）</param>
        /// <param name="childTitle">子窗口标题（没有可传 null）</param>
        /// <returns>子窗口句柄</returns>
        public static IntPtr FindChildWindow(IntPtr parentHandle, string childClass = null, string childTitle = null)
        {
            return Win32API.FindWindowEx(parentHandle, IntPtr.Zero, childClass, childTitle);
        }

        /// <summary>
        /// 发送字符输入（适合文本框输入，自动处理大小写）
        /// </summary>
        /// <param name="hWnd">目标窗口句柄（通常是编辑框）</param>
        /// <param name="text">要输入的文本</param>
        /// <param name="usePost">true 使用 PostMessage（异步），false 使用 SendMessage（同步）</param>
        public static void SendText(IntPtr hWnd, string text, bool usePost = false)
        {
            foreach (char c in text)
            {
                if (usePost)
                    Win32API.PostMessage(hWnd, Win32API.WM_CHAR, (IntPtr)c, IntPtr.Zero);
                else
                    Win32API.SendMessage(hWnd, Win32API.WM_CHAR, (IntPtr)c, IntPtr.Zero);
                // 稍微延迟，避免过快丢失字符（可根据需要调整）
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 模拟按键（按下并释放）
        /// </summary>
        /// <param name="hWnd">目标窗口句柄</param>
        /// <param name="key">要按下的键（使用 System.Windows.Forms.Keys 枚举）</param>
        /// <param name="usePost">是否异步</param>
        public static void SendKeyPress(IntPtr hWnd, Keys key, bool usePost = false)
        {
            if (usePost)
            {
                Win32API.PostMessage(hWnd, Win32API.WM_KEYDOWN, (IntPtr)key, (byte)Win32API.MapVirtualKey((uint)key, 0));
                Win32API.PostMessage(hWnd, Win32API.WM_KEYUP, (IntPtr)key, (byte)Win32API.MapVirtualKey((uint)key, 0));
            }
            else
            {
                Win32API.SendMessage(hWnd, Win32API.WM_KEYDOWN, (IntPtr)key, (byte)Win32API.MapVirtualKey((uint)key, 0));
                Win32API.SendMessage(hWnd, Win32API.WM_KEYUP, (IntPtr)key, (byte)Win32API.MapVirtualKey((uint)key, 0));
            }
        }

        /// <summary>
        /// 模拟鼠标点击（左键/右键）
        /// </summary>
        /// <param name="hWnd">目标窗口句柄</param>
        /// <param name="x">相对于窗口客户区的 X 坐标</param>
        /// <param name="y">相对于窗口客户区的 Y 坐标</param>
        /// <param name="rightButton">true 表示右键，false 表示左键</param>
        /// <param name="usePost">是否异步</param>
        public static void SendMouseClick(IntPtr hWnd, int x, int y, bool rightButton = false, bool usePost = false)
        {
            uint downMsg = rightButton ? Win32API.WM_RBUTTONDOWN : Win32API.WM_LBUTTONDOWN;
            uint upMsg = rightButton ? Win32API.WM_RBUTTONUP : Win32API.WM_LBUTTONUP;
            IntPtr lParam = MakeLParam(x, y);

            if (usePost)
            {
                Win32API.PostMessage(hWnd, downMsg, IntPtr.Zero, lParam);
                Win32API.PostMessage(hWnd, upMsg, IntPtr.Zero, lParam);
            }
            else
            {
                Win32API.SendMessage(hWnd, downMsg, IntPtr.Zero, lParam);
                Win32API.SendMessage(hWnd, upMsg, IntPtr.Zero, lParam);
            }
        }

        /// <summary>
        /// 模拟鼠标移动（仅发送移动消息，不会真正移动光标）
        /// </summary>
        public static void SendMouseMove(IntPtr hWnd, int x, int y, bool usePost = false)
        {
            IntPtr lParam = MakeLParam(x, y);
            if (usePost)
                Win32API.PostMessage(hWnd, Win32API.WM_MOUSEMOVE, IntPtr.Zero, lParam);
            else
                Win32API.SendMessage(hWnd, Win32API.WM_MOUSEMOVE, IntPtr.Zero, lParam);
        }


        public static bool IsValidWindow(IntPtr hWnd) => Win32API.IsWindow(hWnd);
    }
}
