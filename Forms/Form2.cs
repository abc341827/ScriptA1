
using OLAPlug;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using static System.Windows.Forms.AxHost;
using static WinFormsApp1.Win32API;
using Point = System.Drawing.Point;

namespace WinFormsApp1
{
    public partial class Form2 : Form
    {
        // 最大日志长度，超过则从头部裁剪
        private const int _richTextMaxLength = 2000;
        private readonly string _listBoxDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinFormsApp1", "listbox_items.json");
        private ScreenCaptureManager _captureManager;
        private dawnBuy dawnBuy;
        private List<TradeItem> tradeItems;
        private List<Tuple<string, int>> toBuy = new List<Tuple<string, int>>();
        private bool _isSelecting = false;
        private IntPtr targetWindow;
        private List<Point> points;
        private readonly object _dawnLock = new object();
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

        public Form2()
        {
            InitializeComponent();
            // 额外设置 MaxLength，防止文本过大
            try { this.richTextBox1.MaxLength = _richTextMaxLength; } catch { }
            InitializeCaptureManager();
            var mf = Task.Run(() =>
            {
                dawnBuy = new dawnBuy(_captureManager);
                dawnBuy.ProgressChanged += DawnBuy_ProgressChanged;
                dawnBuy.postMessage += DawnBuy_postMessage; ;

            });

            tradeItems = new List<TradeItem> {
                new TradeItem { Name = "离子薄膜" , label = this.离子薄膜},
                new TradeItem { Name = "纳米晶核", label = this.纳米晶核 },
                new TradeItem { Name = "配件指南" , label = this.配件指南},
                new TradeItem { Name = "加固合金" , label = this.加固合金},
                new TradeItem { Name = "尖兵密钥" , label = this.尖兵密钥},
                new TradeItem { Name = "工艺蓝图" , label = this.工艺蓝图},
                new TradeItem { Name = "3级碳化硅晶粒" , label = this.三级碳化硅晶粒},
                new TradeItem { Name = "4级碳化硅晶粒" , label = this.四级碳化硅晶粒},
                new TradeItem { Name = "超导晶核" , label = this.超导晶核},
                new TradeItem { Name = "源能核心" , label = this.源能核心},
                new TradeItem { Name = "3级源能碎片" , label = this.三级源能碎片},
                new TradeItem { Name = "配件增幅器" , label = this.配件增幅器},
                new TradeItem { Name = "3级枪械增强核心", label = this.三级枪械增强核心 },
                new TradeItem { Name = "3级防具增强核心" , label = this.三级防具增强核心},
                new TradeItem { Name = "极光序列" , label = this.极光序列},
                new TradeItem { Name = "耀斑精华" , label = this.耀斑精华},
            };
            RegisterHotkeyOrShowError(HOTKEY_ID_1, None, (uint)Keys.F5);
            // load persisted listbox items
            LoadListBoxItems();
            //frmMain_LoadAsync();
        }

        private void frmMain_LoadAsync()
        {
            //Task.Run(async () =>
            //{
            //    Environment.SetEnvironmentVariable("GLOG_v", "0");
            //    FullOcrModel model = await OnlineFullModels.ChineseV5.DownloadAsync();
            //    FastCheck(model);
            //});

            ////string startupPath = Application.StartupPath;
            ////string rootDirectory = EngineBase.GetRootDirectory();
            ////string path = Path.Combine(rootDirectory, "inference");     
            ////string directoryPath = Path.Combine(path, "yt_PP-OCRv5_mobile_det_infer");
            ////string directoryPath2 = Path.Combine(path, "yt_PP-OCRv5_mobile_cls_infer");
            ////string directoryPath3 = Path.Combine(path, "yt_PP-OCRv5_mobile_rec_infer");
            ////string labelPath = startupPath + "inference\\ppocrv5_dict.txt";
            ////var model = new FullOcrModel(
            ////        DetectionModel.FromDirectory(directoryPath, ModelVersion.V5),
            ////        ClassificationModel.FromDirectory(directoryPath2),
            ////        RecognizationModel.FromDirectory(directoryPath3, labelPath, ModelVersion.V5));
            ////Program.paddleOcr = new PaddleOcrAll(model, PaddleDevice.Gpu());
            //Program.paddleOcr.AllowRotateDetection = true;
            //Program.paddleOcr.Enable180Classification = false;


        }

        //private static void FastCheck(FullOcrModel model)
        //{
        //    // from: https://visualstudio.microsoft.com/wp-content/uploads/2021/11/Home-page-extension-visual-updated.png
        //    byte[] sampleImageData = File.ReadAllBytes(@"C:\Users\liminghan\Pictures\Test\65561526-1c25-4147-a9d6-8ffc13c3c382.png");
        //    Environment.SetEnvironmentVariable("GLOG_logtostderr", "1");
        //    Environment.SetEnvironmentVariable("GLOG_v", "2");
        //    using (PaddleOcrAll all = new(model)
        //    {
        //        AllowRotateDetection = true,
        //        Enable180Classification = true,
        //    })
        //    {
        //        // Load local file by following code:
        //        // using (Mat src2 = Cv2.ImRead(@"C:\test.jpg"))
        //        using (Mat src = Cv2.ImDecode(sampleImageData, ImreadModes.Color))
        //        {
        //            PaddleOcrResult result = all.Run(src);
        //            Console.WriteLine("Detected all texts: \n" + result.Text);
        //            foreach (PaddleOcrResultRegion region in result.Regions)
        //            {
        //                Console.WriteLine($"Text: {region.Text}, Score: {region.Score}, RectCenter: {region.Rect.Center}, RectSize:    {region.Rect.Size}, Angle: {region.Rect.Angle}");
        //            }
        //        }
        //    }
        //}

        private void DawnBuy_postMessage(string obj)
        {
            if (obj == "执行向上滑动以回到顶部")
            {
                //然后刷新页面
                InputSimulator.MouseMove(this.points[0].X, this.points[0].Y, AbsPoint: true);
                Thread.Sleep(100);
                InputSimulator.LeftClick();
            }
            else
            {
                AppendLog(obj);
            }
        }

        // 安全追加日志到 richTextBox1，超过长度会从头部裁剪
        private void AppendLog(string message)
        {
            if (this.richTextBox1 == null) return;

            // Ensure we run on UI thread
            if (this.richTextBox1.InvokeRequired)
            {
                this.richTextBox1.Invoke(new Action(() => AppendLog(message)));
                return;
            }

            var textToAdd = message + Environment.NewLine;
            try
            {
                // 若加入后超过限制，则先删除头部多余部分
                if (this.richTextBox1.TextLength + textToAdd.Length > _richTextMaxLength)
                {
                    int remove = (this.richTextBox1.TextLength + textToAdd.Length) - _richTextMaxLength;
                    if (remove > 0 && remove <= this.richTextBox1.TextLength)
                    {
                        this.richTextBox1.Select(0, remove);
                        this.richTextBox1.SelectedText = string.Empty;
                    }
                }

                this.richTextBox1.AppendText(textToAdd);
                this.richTextBox1.SelectionStart = this.richTextBox1.TextLength;
                this.richTextBox1.ScrollToCaret();
            }
            catch
            {
                // ignore
            }
        }

        private void DawnBuy_ProgressChanged(TradeItem obj)
        {
            var item = tradeItems.FirstOrDefault(x => x.Name == obj.Name);
            if (item != null)
            {
                item.Price = obj.Price;
                item.All = obj.All;
                item.LowLeft = obj.LowLeft;
                item.Rect = obj.Rect;
                AppendLog("找到" + item.Name + "啦!");
            }
            else
            {
                return;
            }
            {
                item.label.Invoke(() =>
                {
                    item.label.Text = $"{item.Name} 价格:{item.Price}";
                });
            }
            var buy = this.toBuy.FirstOrDefault(x => x.Item1 == obj.Name);
            if (buy != null)
            {
                if (buy.Item2 >= obj.Price)
                {
                    InputSimulator.MouseMove(obj.Position.X, obj.Position.Y, AbsPoint: true);
                    Thread.Sleep(150);
                    InputSimulator.LeftClick();
                    Thread.Sleep(500);
                    InputSimulator.MouseMove(this.points[1].X, this.points[1].Y, AbsPoint: true);
                    Thread.Sleep(150);
                    InputSimulator.LeftClick();
                    Thread.Sleep(500);
                    InputSimulator.MouseMove(this.points[2].X, this.points[2].Y, AbsPoint: true);
                    Thread.Sleep(150);
                    InputSimulator.LeftClick();
                    AppendLog("买！" + buy.Item1);
                    Thread.Sleep(2000);
                }
                else
                {
                    AppendLog("不买！" + buy.Item1 + obj.Price + "太贵啦!");
                }

            }
        }

        private void InitializeCaptureManager()
        {
            _captureManager = new ScreenCaptureManager();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.toBuy.Clear();
            foreach (string item in listBox1.Items)
            {
                var p = item.Split('|');
                if (p.Length == 2)
                {
                    this.toBuy.Add(new Tuple<string, int>(p[0], int.Parse(p[1])));
                }
            }
            var num = 1;
            try
            {
                num = int.Parse(this.textBox2.Text);
            }
            catch
            {
            }
            var num1 = 1;
            try
            {
                num1 = int.Parse(this.textBox3.Text);
            }
            catch
            {
            }
            dawnBuy._rollNumber = num;
            dawnBuy._rollSize = num1;
            dawnBuy.Run();

        }
        public IntPtr temphandle = IntPtr.Zero;
        private void button2_Click(object sender, EventArgs e)
        {
            // 显示屏幕选择器，用户通过鼠标选择一个区域
            _captureManager.ShowSelector((a) =>
            {

            });

            // 在后台等待用户完成选择，完成后根据选择区域中心点获取窗口句柄
            Task.Run(() =>
            {
                // 等待选择窗口关闭
                while (_captureManager.IsSelecting)
                {
                    Thread.Sleep(100);
                }

                var sel = _captureManager.CurrentSelection;
                if (sel.IsEmpty) return;

                var center = new System.Drawing.Point(sel.X + sel.Width / 2, sel.Y + sel.Height / 2);

                // 获取位于该点的窗口句柄
                IntPtr hWnd = Win32API.WindowFromPoint(center);
                if (hWnd == IntPtr.Zero) return;

                // 获取顶层窗口句柄（方便识别进程）
                IntPtr top = GetAncestor(hWnd, GA_ROOT);
                var rec = GameWindowCapture.GetWindowBounds(top);
                var title = GetWindowTextW(top);
                this.dawnBuy._isLeiGod = title.Contains("雷电");
                // todo :show tips 


                // 将结果回到 UI 线程显示
                this.Invoke(() =>
                {
                    this.targetWindow = top;
                    // 计算默认需要点击的按钮位置（示例：窗口左上偏移 100,50）
                    var bounds = rec; // 已经是屏幕坐标
                    var defaultPoints = new List<Point>
                    {
                        new Point((int)(bounds.Width *0.8157),  (int)(bounds.Height *0.315)),//装备培养(刷新)
                        new Point((int)(bounds.Width *0.6607),  (int)(bounds.Height *0.615)),//最大
                        new Point( (int)(bounds.Width *0.5928),  (int)(bounds.Height *0.7337))//确定
                    };

                    // 在屏幕上显示可拖动的标记，让用户调整
                    using (var marker = new WindowMarkerForm(bounds, defaultPoints))
                    {
                        var dr = marker.ShowDialog();
                        if (dr == DialogResult.OK)
                        {
                            var pts = marker.Markers;
                            // 保存到成员变量，后续执行脚本读取
                            // 这里我们把第一个点作为示例
                            if (pts.Count > 0)
                            {
                                this.points = pts.ToList();
                            }
                        }

                        temphandle = hWnd;
                        InputSimulator.windowRec = rec;
                    }
                });
            });
        }
        private Color GetColorAt(int x, int y)
        {
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(1, 1));
                }
                return bmp.GetPixel(0, 0);
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



        private static string GetWindowTextW(IntPtr hWnd)
        {
            try
            {
                int len = GetWindowTextLength(hWnd);
                if (len <= 0) return string.Empty;

                var sb = new System.Text.StringBuilder(len + 1);
                GetWindowText(hWnd, sb, sb.Capacity);
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.dawnBuy.cancellationTokenSource.Cancel();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _captureManager.ShowSelector(action1: (a) =>
            {
                dawnBuy.color = GetColorAt(a.X, a.Y);
            });

        }

        private void button6_Click(object sender, EventArgs e)
        {
            var text = this.textBox1.Text;
            this.listBox1.Items.Add(text);
            SaveListBoxItems();
        }

        private void button7_Click(object sender, EventArgs e)
        {

            this.listBox1.Items.RemoveAt(this.listBox1.SelectedIndex);
            SaveListBoxItems();
        }

        private void LoadListBoxItems()
        {
            try
            {
                var dir = Path.GetDirectoryName(_listBoxDataPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                if (!File.Exists(_listBoxDataPath)) return;
                var json = File.ReadAllText(_listBoxDataPath);
                var list = JsonSerializer.Deserialize<List<string>>(json);
                if (list == null) return;
                this.listBox1.Items.Clear();
                foreach (var it in list) this.listBox1.Items.Add(it);
            }
            catch (Exception ex)
            {
                // ignore load failures but show to debug output
                try { AppendLog($"LoadListBoxItems error: {ex.Message}"); } catch { }
            }
        }

        private void SaveListBoxItems()
        {
            try
            {
                var dir = Path.GetDirectoryName(_listBoxDataPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var list = new List<string>();
                foreach (var it in this.listBox1.Items) list.Add(it?.ToString() ?? string.Empty);
                var json = JsonSerializer.Serialize(list);
                File.WriteAllText(_listBoxDataPath, json);
            }
            catch (Exception ex)
            {
                try { AppendLog($"SaveListBoxItems error: {ex.Message}"); } catch { }
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // 阻止非数字和非控制字符的输入
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {               //然后刷新页面
            InputSimulator.MouseMove(this.points[0].X, this.points[0].Y, AbsPoint: true);
            Thread.Sleep(100);

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(int.Parse(this.textBox2.Text));
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        }

    }

    public class TradeItem
    {
        public string Name { get; set; }

        public int Price { get; set; }

        public string All { get; set; }

        public string LowLeft { get; set; }

        public Rectangle Rect { get; set; }
        public Label label { get; set; }

        public Point Position
        {
            get
            {
                return new Point(Rect.X + Rect.Width / 2, Rect.Y + Rect.Height / 2);
            }
        }
    }
}
