using NAudio.MediaFoundation;
using OpenCvSharp.Flann;
using System.Runtime.InteropServices;
using System.Text;
using YamlDotNet.Core.Tokens;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // 全局热键相关
        private const int HOTKEY_ID_1 = 1;
        private const int HOTKEY_ID_2 = 2;
        private const int HOTKEY_ID_3 = 3;
        private const int HOTKEY_ID_4 = 4;

        private const uint None = 0x0000;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        private const int WM_HOTKEY = 0x0312;

        private const uint WM_KEYDOWN = 0x100;
        private const uint WM_KEYUP = 0x101;
        private const int WA_ACTIVE = 1;
        private const int WA_CLICKACTIVE = 2;
        private const uint WM_ACTIVATE = 0x0006;
        private const uint WM_SETFOCUS = 0x0007;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private int count = 1;
        private CubeAutomationService mofang;
        private ScreenCaptureManager _captureManager;
        private readonly IInputController _inputController;
        private readonly IRecordWriter _recordWriter;
        private bool _isSelecting = false;

        public Form1()
            : this(new Win32InputController(), new FileRecordWriter())
        {
        }

        public Form1(IInputController inputController, IRecordWriter recordWriter)
        {
            _inputController = inputController;
            _recordWriter = recordWriter;
            InitializeComponent();
            InitializeCaptureManager();

            // 注册 Load/Closed 事件，在窗口句柄创建后注册全局热键，窗口关闭时注销
            this.Load += Form1_Load;
            this.FormClosed += Form1_FormClosed;
            var mf = Task.Run(async () =>
            {
                mofang = new CubeAutomationService(_captureManager, _inputController, _recordWriter);
                //var model = await OnlineFullModels.ChineseServerV5.DownloadAsync();
                //mofang.myocrService = new PaddleOcrAll(model);

                mofang.ProgressChanged += Mofang_ProgressChanged;
                mofang.ProgressCompeleted += Mofang_ProgressCompeleted;
                this.button1.Invoke(() =>
                {
                    button1.Enabled = true;
                });

                this.button6.Invoke(() =>
                {
                    button6.Enabled = true;
                });
            });

            listBox1.SelectionMode = SelectionMode.MultiExtended;

            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            // 示例热键：Ctrl+Shift+1 调用 button1，Ctrl+Shift+2 调用 button6，
            // Ctrl+Shift+3 调用 button4（停止），Ctrl+Shift+4 调用 button8（截取并识别）
            // 可以按需修改修饰符和按键（Keys 枚举值）
            RegisterHotkeyOrShowError(HOTKEY_ID_1, None, (uint)Keys.F5);

            // 尝试加载上次保存的状态（组合框和列表项）
            LoadAppState();
        }

        private void Form1_FormClosed(object? sender, FormClosedEventArgs e)
        {
            // 注销所有热键
            UnregisterHotKey(this.Handle, HOTKEY_ID_1);
            // 保存状态（组合框和列表项）
            SaveAppState();
        }

        private void LoadAppState()
        {
            try
            {
                var state = JsonFileStore.Load<AppState>(AppPaths.CubeAppStateFile);
                if (state == null)
                    return;

                comboBox1.Text = state.Combo1 ?? string.Empty;
                comboBox2.Text = state.Combo2 ?? string.Empty;
                comboBox3.Text = state.Combo3 ?? string.Empty;
                comboBox4.Text = state.Combo4 ?? string.Empty;
                comboBox5.Text = state.Combo5 ?? string.Empty;
                comboBox6.Text = state.Combo6 ?? string.Empty;

                listBox1.Items.Clear();
                if (state.ListBoxItems != null)
                {
                    foreach (var it in state.ListBoxItems)
                        listBox1.Items.Add(it);
                }
            }
            catch
            {
                // 忽略加载错误
            }
        }

        private void SaveAppState()
        {
            try
            {
                var state = new AppState
                {
                    Combo1 = comboBox1.Text,
                    Combo2 = comboBox2.Text,
                    Combo3 = comboBox3.Text,
                    Combo4 = comboBox4.Text,
                    Combo5 = comboBox5.Text,
                    Combo6 = comboBox6.Text,
                    ListBoxItems = new List<string>()
                };

                foreach (var it in listBox1.Items)
                    state.ListBoxItems.Add(it?.ToString() ?? string.Empty);

                JsonFileStore.Save(AppPaths.CubeAppStateFile, state, writeIndented: true);
            }
            catch
            {
                // 忽略保存错误
            }
        }

        private void RegisterHotkeyOrShowError(int id, uint modifiers, uint vk)
        {
            bool ok = RegisterHotKey(this.Handle, id, modifiers, vk);
            if (!ok)
            {
                // 仅在开发/调试时提示；生产可以改为记录日志
                var err = Marshal.GetLastWin32Error();
                MessageBox.Show($"注册全局热键失败 (id={id}, vk={vk})，错误码: {err}", "热键注册失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // 捕获系统消息，处理 WM_HOTKEY
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                switch (id)
                {
                    case HOTKEY_ID_1:
                        // 在 UI 线程安全调用按钮事件
                        BeginInvoke(new Action(() =>
                        {
                            button4.PerformClick();
                        }));
                        break;
                }
            }

            base.WndProc(ref m);
        }

        private void Mofang_ProgressCompeleted(string msg)
        {
            var player = new BasicAudioPlayer();
            // When publishing as a single-file app the runtime may extract files to a temporary
            // directory. Use AppContext.BaseDirectory which points to the runtime directory
            // (extraction directory for single-file) to find the audio file.
            string audioFileName = "MissonCompelet.mp3";
            string audioPath = Path.Combine(AppContext.BaseDirectory, audioFileName);
            if (!File.Exists(audioPath))
            {
                // Fallback to current working directory if not found in base directory
                audioPath = Path.Combine(Directory.GetCurrentDirectory(), audioFileName);
            }

            player.PlayAudioFile(audioPath); // 支持MP3, WAV, AIFF等
            button1.Invoke(() =>
            {
                button1.Enabled = true;
            });
            button6.Invoke(() =>
            {
                button6.Enabled = true;
            });
            if (!string.IsNullOrWhiteSpace(msg))
            {
                MessageBox.Show(msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeCaptureManager()
        {
            _captureManager = new ScreenCaptureManager();
        }

        private void Mofang_ProgressChanged(string current, string old, Bitmap bitmap)
        {
            label2.Invoke(new Action(() =>
            {
                this.label2.Text = "當前:" + current.Trim().Replace("\n", "");
            }));
            label4.Invoke(new Action(() =>
            {
                this.label4.Text = "上次:" + old.Trim().Replace("\n", "");
            }));
            pictureBox3.Invoke(new Action(() =>
            {
                this.pictureBox3.Image?.Dispose();
                this.pictureBox3.Image = pictureBox2.Image;
            }));
            pictureBox2.Invoke(new Action(() =>
            {
                this.pictureBox2.Image = bitmap;
            }));
        }

        private bool ApplyTargetProperties()
        {
            var targetProperties = new List<List<string>>();
            foreach (string item in listBox1.Items)
            {
                targetProperties.Add([.. item.Split("|")]);
            }

            if (targetProperties.Count == 0)
            {
                MessageBox.Show("未設置屬性");
                return false;
            }

            mofang.SetTargetProperties(targetProperties);
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //var text = new OcrService().RecognizeBaidu("D:\\Project\\WinFormsApp1\\Resource\\属性.png");
            if (!ApplyTargetProperties())
            {
                return;
            }
            string gameProcessName = this.textBox2.Text;
            var windowInfo = GameWindowCapture.FindWindowByProcessName(gameProcessName, false);
            var delay = 2000;
            try
            {
                delay = int.Parse(this.textBox1.Text);
            }
            catch
            {
            }

            mofang.Run(windowInfo.FirstOrDefault().Handle, "普通", delay);
            button1.Enabled = false;
            button6.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(comboBox1.Text + "|" + comboBox2.Text + "|" + comboBox3.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                // 必须从后往前删除，避免索引变化
                for (int i = listBox1.SelectedItems.Count - 1; i >= 0; i--)
                {
                    listBox1.Items.Remove(listBox1.SelectedItems[i]);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.mofang.Stop();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!_isSelecting)
            {
                // 开始选择
                _isSelecting = _captureManager.ShowSelector((a) =>
                {
                    var hWnd = (IntPtr)Win32API.WindowFromPoint(new System.Drawing.Point(a.Left, a.Top));
                    // 获取顶层窗口句柄（方便识别进程）
                    IntPtr top = Win32API.GetAncestor(hWnd, Win32API.GA_ROOT);
                    var rec = GameWindowCapture.GetWindowBounds(top);
                    MessageBox.Show(temphandle.ToString());
                    temphandle = hWnd;
                    _inputController.WindowBounds = rec;
                });
                button5.Text = _isSelecting ? "停止选择 (ESC)" : "选择区域";
                button5.BackColor = _isSelecting ? Color.LightCoral : SystemColors.Control;

            }
            else
            {
                // 停止选择
                _captureManager.CloseSelector();
                _isSelecting = false;
                button5.Text = "选择区域";
                button5.BackColor = SystemColors.Control;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (!ApplyTargetProperties())
            {
                return;
            }
            string gameProcessName = this.textBox2.Text;
            var windowInfo = GameWindowCapture.FindWindowByProcessName(gameProcessName, false);
            var delay = 2000;
            try
            {
                delay = int.Parse(this.textBox1.Text);
            }
            catch
            {
            }

            mofang.Run(windowInfo.FirstOrDefault().Handle, "怪怪", delay);
            button6.Enabled = false;
            button1.Enabled = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(comboBox4.Text + "|" + comboBox5.Text + "|" + comboBox6.Text);
        }
        public IntPtr temphandle = IntPtr.Zero;

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // 阻止非数字和非控制字符的输入
            }
        }

        private int x = 0;
        private int y = 0;
        // 窗体鼠标点击事件
        private void button8_Click(object sender, EventArgs e)
        {
            _captureManager.ShowSelector(action1: (a) =>
            {
                x = a.X;
                y = a.Y;
            });
        }
        private void button10_Click(object sender, EventArgs e)
        {
            // 获取目标窗口所属线程ID
            uint targetThreadId = Win32API.GetWindowThreadProcessId(temphandle, out _);
            // 获取当前线程ID
            uint currentThreadId = Win32API.GetCurrentThreadId();
            Win32API.GetWindowThreadProcessId(temphandle, out uint processid);
            int pid = (int)processid;
            if (pid == 0)
            {
                return;
            }

            //string dllFullPath = @"D:\Project\MXDWndProc\x64\Debug\MXDWndProc.dll"; // 改为实际路径
            //Injector.CallRemoteFunction(pid, dllFullPath, "CheckAndExecuteKey", 56);
            {
                Task.Run(() =>
                {
                    try
                    {
                        // 附加输入线程并使用 InputSimulator 来发送按键
                        //if (!Win32API.AttachThreadInput(currentThreadId, targetThreadId, true))
                        //{
                        //    int error = Marshal.GetLastWin32Error();
                        //    MessageBox.Show($"AttachThreadInput 失败，错误码: {error}");
                        //    return;
                        //}

                        Thread.Sleep(2000);
                        WindowMessageSimulator.SendKeyPress(temphandle, Keys.Left);
                    }
                    finally
                    {
                        Win32API.AttachThreadInput(currentThreadId, targetThreadId, false);
                    }
                });
            }

            #region 调用取消钩子
            //Win32API.GetWindowThreadProcessId(temphandle, out uint processid);
            //int pid = (int)processid;
            //if (pid == 0)
            //{
            //    return;
            //}
            //string dllFullPath = @"D:\Project\MXDWndProc\x64\Debug\MXDWndProc.dll"; // 改为实际路径

            //Injector.CallRemoteFunction(pid, dllFullPath, "UninstallHook", 0);
            #endregion
        }
    }
}