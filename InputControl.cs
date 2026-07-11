using OLAPlug;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using static WinFormsApp1.Win32API;
namespace WinFormsApp1
{

    public class InputSimulator
    {
        public static Rectangle windowRec { get; set; }


        #region 鼠标操作

        /// <summary>
        /// 移动鼠标到绝对坐标
        /// </summary>
        public static void MouseMove(int x, int y, OLAPlugServer? OlaServer = null, bool AbsPoint = false)
        {

            if (OlaServer != null)
            {
                if (AbsPoint)
                {
                    x -= windowRec.Left;
                    y -= windowRec.Top;
                }
                GetCursorPos(out Win32API.POINT cur);
                if (cur.X == x && cur.Y == y)
                    return;

                try { OLAPlug.OLAPlugDLLHelper.MoveToWithoutSimulator(OlaServer.OLAObject, x, y); return; }
                catch { }
            }
            SetCursorPos(x, y);
        }

        /// <summary>
        /// 左键单击
        /// </summary>
        public static void LeftClick(int downUpDelay = 150, OLAPlugServer? OlaServer = null)
        {
            if (OlaServer != null)
            {
                try
                {
                    OLAPlug.OLAPlugDLLHelper.LeftDown(OlaServer.OLAObject);
                    Thread.Sleep(downUpDelay);
                    OLAPlug.OLAPlugDLLHelper.LeftUp(OlaServer.OLAObject);
                }
                catch { /* fall back if OLA call fails */ }
            }
            else
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                Thread.Sleep(downUpDelay);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
            }
        }

        /// <summary>
        /// 左键双击
        /// </summary>
        public static void LeftDoubleClick(OLAPlugServer? OlaServer = null)
        {
            if (OlaServer != null)
            {
                try { OLAPlug.OLAPlugDLLHelper.LeftDoubleClick(OlaServer.OLAObject); return; }
                catch { }
            }
            LeftClick();
            Thread.Sleep(150);
            LeftClick();
        }

        /// <summary>
        /// 右键单击
        /// </summary>
        public static void RightClick(OLAPlugServer? OlaServer = null)
        {
            if (OlaServer == null)
            {
                try { OLAPlug.OLAPlugDLLHelper.RightClick(OlaServer.OLAObject); return; }
                catch { }
            }
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(10);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// 中键单击
        /// </summary>
        public static void MiddleClick(OLAPlugServer? OlaServer = null)
        {
            if (OlaServer != null)
            {
                try { OLAPlug.OLAPlugDLLHelper.MiddleClick(OlaServer.OLAObject); return; }
                catch { }
            }
            mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(10);
            mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// 左键按下
        /// </summary>
        public static void LeftDown(OLAPlugServer? OlaServer = null)
        {
            if (OlaServer != null)
            {
                try { OLAPlug.OLAPlugDLLHelper.LeftDown(OlaServer.OLAObject); return; }
                catch { }
            }
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// 左键释放
        /// </summary>
        public static void LeftUp(OLAPlugServer? OlaServer = null)
        {
            if (OlaServer != null)
            {
                try { OLAPlug.OLAPlugDLLHelper.LeftUp(OlaServer.OLAObject); return; }
                catch { }
            }
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// 右键按下
        /// </summary>
        public static void RightDown(OLAPlugServer? OlaServer = null)
        {
            if (OlaServer != null)
            {
                try { OLAPlug.OLAPlugDLLHelper.RightDown(OlaServer.OLAObject); return; }
                catch { }
            }
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// 右键释放
        /// </summary>
        public static void RightUp(OLAPlugServer? OlaServer = null)
        {
            if (OlaServer != null)
            {
                try { OLAPlug.OLAPlugDLLHelper.RightUp(OlaServer.OLAObject); return; }
                catch { }
            }
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// 鼠标滚轮滚动。
        /// 接受“刻度数”（notches），内部会乘以系统常量 WHEEL_DELTA (120)。
        /// 例如 notches = 1 表示向上一个刻度（+120），notches = -1 表示向下一个刻度（-120）。
        /// 调用前会把鼠标移动到指定位置，以确保目标窗口/控件在鼠标下方能够接收滚轮事件。
        /// </summary>
        /// <param name="x">屏幕 X 坐标（像素）。</param>
        /// <param name="y">屏幕 Y 坐标（像素）。</param>
        /// <param name="notches">刻度数，正数向上滚动，负数向下滚动。</param>
        public static void MouseWheel(int x, int y, int notches, OLAPlugServer? OlaServer = null)
        {
            const int WHEEL_DELTA = 120;
            int delta = notches * WHEEL_DELTA;
            // mouse_event 的 dwData 为 uint，负值需按补码传递
            //if (OlaServer != null && useOla)
            //{

            //    MouseMove(x, y, AbsPoint: true);

            //    Thread.Sleep(150);
            //    try
            //    {
            //        if (delta > 0)
            //            OlaServer.WheelUp();
            //        else
            //            OlaServer.WheelDown();
            //        return;
            //    }
            //    catch { }
            //}
            //var random = new Random();

            //SetCursorPos(x + random.Next(-100, 100), y + random.Next(-100, 100));
            SetCursorPos(x, y);
            for (int i = 0; i < Math.Abs(notches); i++)
            {
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, notches > 0 ? 120 : unchecked((uint)-120), UIntPtr.Zero);
                Thread.Sleep(100);
            }

        }
        // 获取系统 tick 计数（毫秒）
        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();

        /// <summary>
        /// 检查左键是否按下
        /// </summary>
        public static bool IsLeftButtonPressed()
        {
            return (GetAsyncKeyState(Keys.LButton) & 0x8000) != 0;
        }

        /// <summary>
        /// 检查右键是否按下
        /// </summary>
        public static bool IsRightButtonPressed()
        {
            return (GetAsyncKeyState(Keys.RButton) & 0x8000) != 0;
        }

        #endregion

        #region 键盘操作

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        /// <summary>
        /// 按下单个键
        /// </summary>
        public static void KeyDown(Keys key, OLAPlugServer? OlaServer = null)
        {
            if (OlaServer != null)
            {
                try { OLAPlug.OLAPlugDLLHelper.KeyDown(OlaServer.OLAObject, (int)key); return; }
                catch { }
            }

            keybd_event((byte)key, (byte)MapVirtualKey((uint)key, 0), 0, UIntPtr.Zero);
        }

        /// <summary>
        /// 释放单个键
        /// </summary>
        public static void KeyUp(Keys key, OLAPlugServer? OlaServer = null)
        {
            if (OlaServer != null)
            {
                try { OLAPlug.OLAPlugDLLHelper.KeyUp(OlaServer.OLAObject, (int)key); return; }
                catch { }
            }
            keybd_event((byte)key, (byte)MapVirtualKey((uint)key, 0), KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        /// <summary>
        /// 按键（按下并释放）
        /// </summary>
        public static void KeyPress(Keys key, int delay = 10, OLAPlugServer? OlaServer = null)
        {
            if (OlaServer != null)
            {
                try { OLAPlug.OLAPlugDLLHelper.KeyPress(OlaServer.OLAObject, (int)key); return; }
                catch { }
            }
            KeyDown(key);
            Thread.Sleep(delay);
            KeyUp(key);
        }

        /// <summary>
        /// 模拟输入文本
        /// </summary>
        public static void TypeText(string text, OLAPlugServer? OlaServer = null)
        {
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    KeyPress(Keys.Enter);
                }
                else if (c == '\t')
                {
                    KeyPress(Keys.Tab);
                }
                else
                {
                    // 对于普通字符，可以使用SendKeys（更简单）
                    SendKeys.SendWait(c.ToString());
                }
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 组合键（如Ctrl+C）
        /// </summary>
        public static void KeyCombination(params Keys[] keys)
        {
            // 按下所有键
            foreach (var key in keys)
            {
                KeyDown(key);
            }

            Thread.Sleep(20);

            // 释放所有键（逆序释放是良好习惯）
            for (int i = keys.Length - 1; i >= 0; i--)
            {
                KeyUp(keys[i]);
            }
        }

        /// <summary>
        /// 检查指定键是否按下
        /// </summary>
        public static bool IsKeyPressed(Keys key)
        {
            return (GetAsyncKeyState(key) & 0x8000) != 0;
        }

        /// <summary>
        /// 常用组合键的快捷方法
        /// </summary>
        public static class Shortcuts
        {
            public static void Copy() => KeyCombination(Keys.ControlKey, Keys.C);
            public static void Paste() => KeyCombination(Keys.ControlKey, Keys.V);
            public static void Cut() => KeyCombination(Keys.ControlKey, Keys.X);
            public static void SelectAll() => KeyCombination(Keys.ControlKey, Keys.A);
            public static void Save() => KeyCombination(Keys.ControlKey, Keys.S);
            public static void Undo() => KeyCombination(Keys.ControlKey, Keys.Z);
            public static void Redo() => KeyCombination(Keys.ControlKey, Keys.Y);
        }

        #endregion

        #region 高级操作
        /// <summary>
        /// 延迟执行（单位：毫秒）
        /// </summary>
        public static void Delay(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        #endregion
        public static bool ForceActivateWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return false;

            try
            {
                // 方法1: 使用SwitchToThisWindow (类似Alt+Tab)
                SwitchToThisWindow(hWnd, true);

                // 方法2: 设置活动窗口
                SetActiveWindow(hWnd);

                // 方法3: 设置焦点
                SetFocus(hWnd);

                // 方法4: 发送WM_SETFOCUS消息
                SendMessage(hWnd, WM_SETFOCUS, 0, 0);

                // 方法5: 使用SetForegroundWindow
                SetForegroundWindow(hWnd);

                // 检查是否成功
                System.Threading.Thread.Sleep(100);
                return GetForegroundWindow() == hWnd;
            }
            catch
            {
                return false;
            }
        }
    }
}