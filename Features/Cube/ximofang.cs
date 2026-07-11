
using NAudio.CoreAudioApi;
using NAudio.Utils;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Sdcb.PaddleInference;
using Sdcb.PaddleInference.Native;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;

using System.Diagnostics;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace WinFormsApp1
{
    public class ximofang
    {
        private ScreenCaptureManager _captureManager;
        public event Action<string, string, Bitmap> ProgressChanged;
        public event Action<string> ProgressCompeleted;
        public PaddleOcrAll myocrService;
        public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public static List<List<string>> targetProperties = new List<List<string>>();
        public static readonly List<string> AllProperties = new List<string>
        {
            "STR",
            "LUK",
            "INT",
            "DEX",
            "全屬性",
            "MaxHP",
            "魔法攻擊力",
            "攻擊力",
            "攻擊Boss怪物時傷害",
            "以角色等級爲準每9級 STR",
            "以角色等級爲準每9級 LUK",
            "以角色等級爲準每9級 INT",
            "以角色等級爲準每9級 DEX",
            "暴擊傷害",
            "技能冷卻時間",
            " "
        };
        private Bitmap lastBit = null;
        private string lastText = "";

        public ximofang(ScreenCaptureManager captureManager)
        {

            InitOcr();
            _captureManager = captureManager;
        }

        private void InitOcr()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string startupPath = AppDomain.CurrentDomain.BaseDirectory;
            string directoryPath = startupPath + "inference\\PP-OCRv5_mobile_det_infer";
            string directoryPath2 = startupPath + "inference\\ch_ppocr_mobile_v2.0_cls_infer";
            //string directoryPath3 = startupPath + "inference\\EqAndMosterRec";
            string directoryPath3 = startupPath + "inference\\PP-OCRv5_mobile_rec_infer";
            //
            //string labelPath = startupPath + "inference\\EqMonster_dict.txt";
            string labelPath = "";
            // string labelPath = startupPath + "inference\\ppocrv5_dict.txt";

            myocrService = new PaddleOcrAll(
                new FullOcrModel(
                    DetectionModel.FromDirectory(directoryPath, ModelVersion.V5),
                    ClassificationModel.FromDirectory(directoryPath2),
                    RecognizationModel.FromDirectory(directoryPath3, labelPath, ModelVersion.V5)),
               (device) =>
               {
                   device.EnableUseGpu(500, 0);
                   var a = device.UseGpu;
               });
            myocrService.AllowRotateDetection = false;
            myocrService.Enable180Classification = false;

        }


        public string myGetProperties()
        {
            var bmp = _captureManager.CaptureSelectedArea();
            Mat matImage = BitmapConverter.ToMat(bmp);
            //using var bmp = DirectXWindowCapture.CaptureGameWindow(Handle, rect);
            //bmp.Save("C:\\Users\\liminghan\\Pictures\\Screenshots\\test.png", ImageFormat.Png);
            var text = myocrService.Run(matImage).Text;
            ProgressChanged.Invoke(text, lastText, bmp);
            lastText = text;
            lastBit = bmp;
            return text;
        }
        public string myItems(Mat bitmap)
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 150; i++)
            {
                try
                {
                    var a = myocrService.Run(bitmap).Text;
                }
                catch (Exception ex)
                {

                    throw ex;
                }

            }
            sw.Stop();
            MessageBox.Show($"平均识别时间: {sw.ElapsedMilliseconds / 100.0} ms");
            return "";
            //using var bmp = DirectXWindowCapture.CaptureGameWindow(Handle, rect);
            //bmp.Save("C:\\Users\\liminghan\\Pictures\\Screenshots\\test.png", ImageFormat.Png);

        }


        public string CheckProperties(string pros)
        {
            foreach (var src in targetProperties)
            {
                var isMatched = CanMatchElementsFuzzy(pros, [.. src], 73.0);
                if (!string.IsNullOrEmpty(isMatched))
                {
                    return isMatched;
                }
            }

            return string.Empty;
        }
        /// <summary>
        /// 带编辑距离容错的顺序匹配
        /// </summary>
        public static string CanMatchElementsFuzzy(string A, string[] B, double similarityThreshold = 85.0, int maxSearchWindow = 50)
        {
            int currentIndex = 0;
            string result = string.Empty;
            foreach (string element in B)
            {
                if (string.IsNullOrWhiteSpace(element))
                    continue;

                bool found = false;
                string temp = string.Empty;
                int elementLength = element.Length;

                // 在当前索引之后滑动窗口进行模糊查找
                for (int start = currentIndex; start < A.Length; start++)
                {
                    // 计算最大搜索长度（考虑可能的识别错误）
                    int maxSubLength = Math.Min(elementLength + 5, A.Length - start);
                    if (maxSubLength < elementLength * 0.5) // 剩余文本太短
                        break;

                    // 尝试不同长度的子串
                    for (int subLength = maxSubLength; subLength >= Math.Max(1, elementLength - 3); subLength--)
                    {
                        if (start + subLength > A.Length)
                            continue;

                        string substring = A.Substring(start, subLength);

                        double similarity = LevenshteinDistance.ComputeSimilarity(substring, element);

                        if (similarity >= similarityThreshold)
                        {
                            A = A.Remove(start, subLength);
                            // 找到匹配，更新当前位置
                            currentIndex = 0;
                            found = true;
                            temp = substring;
                            //Console.WriteLine($"模糊匹配成功: '{substring}' ≈ '{element}' (相似度: {similarity:F1}%)");
                            break;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(temp))
                    {
                        result += temp;
                        break;
                    }


                    // 限制搜索窗口大小，避免性能问题
                    if (start - currentIndex > maxSearchWindow)
                        break;
                }

                if (string.IsNullOrWhiteSpace(temp))
                {
                    return string.Empty;
                }
            }

            return result;
        }

        /// <summary>
        /// 带编辑距离容错的顺序匹配，并返回匹配详情
        /// </summary>
        public static MatchResult CanMatchElementsFuzzyWithDetails(string A, string[] B, double similarityThreshold = 85.0)
        {
            var result = new MatchResult();
            int currentIndex = 0;

            foreach (string element in B)
            {
                if (string.IsNullOrEmpty(element))
                    continue;

                var matchInfo = new SingleMatch { Target = element };
                bool found = false;

                for (int start = currentIndex; start < A.Length; start++)
                {
                    int maxSubLength = Math.Min(element.Length + 5, A.Length - start);

                    for (int subLength = maxSubLength; subLength >= Math.Max(1, element.Length - 3); subLength--)
                    {
                        if (start + subLength > A.Length)
                            continue;

                        string substring = A.Substring(start, subLength);
                        double similarity = LevenshteinDistance.ComputeSimilarity(substring, element);

                        if (similarity >= similarityThreshold)
                        {
                            matchInfo.MatchedText = substring;
                            matchInfo.Similarity = similarity;
                            matchInfo.StartIndex = start;
                            matchInfo.Length = subLength;

                            currentIndex = start + subLength;
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        break;
                }

                result.Matches.Add(matchInfo);
                if (!found)
                {
                    result.AllMatched = false;
                    return result;
                }
            }

            result.AllMatched = true;
            return result;
        }


        public void Run(nint Handle, string model = "普通", int delay = 2000)
        {
            cancellationTokenSource = new CancellationTokenSource();
            string last = string.Empty;
            string foundResult = string.Empty;
            InitOcr();
            var task = Task.Run(() =>
            {
                try
                {
                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        var current = myGetProperties();
                        last = current;
                        var reslut = CheckProperties(current);
                        if (!string.IsNullOrWhiteSpace(reslut))
                        {
                            // 标记已找到并请求取消，避免继续模拟操作
                            foundResult = reslut;
                            cancellationTokenSource.Cancel();
                            break;
                        }

                        InputSimulator.ForceActivateWindow(Handle);
                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            break;
                        }

                        // 每次按键后都检查取消，以确保能在检测到后尽快停止
                        void PressSpaceSequence()
                        {
                            InputSimulator.KeyPress(Keys.Space);
                            InputSimulator.Delay(150);
                            if (cancellationTokenSource.IsCancellationRequested) return;
                            InputSimulator.KeyPress(Keys.Space);
                            InputSimulator.Delay(150);
                            if (cancellationTokenSource.IsCancellationRequested) return;
                            InputSimulator.KeyPress(Keys.Space);
                            InputSimulator.Delay(150);
                            if (cancellationTokenSource.IsCancellationRequested) return;
                            InputSimulator.KeyPress(Keys.Space);
                        }

                        PressSpaceSequence();

                        // 使用可取消的等待，替换 Thread.Sleep
                        for (int waited = 0; waited < delay; waited += 200)
                        {
                            if (cancellationTokenSource.IsCancellationRequested) break;
                            Thread.Sleep(Math.Min(200, delay - waited));
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // 正常取消，不需要处理
                }
                catch (Exception ex)
                {
                    // 保证异常不会被吞掉，记录或传递给订阅者
                    foundResult = "__ERROR__: " + ex.Message;
                }
            }, cancellationTokenSource.Token);

            task.ContinueWith((t) =>
            {
                // 避免未觀察異常
                if (t.IsFaulted)
                {
                    var _ = t.Exception; // 读取以标记为已观察
                }

                // 将完成通知交给订阅者（UI 端负责展示信息）
                ProgressCompeleted?.Invoke(foundResult);
            });
        }

        // 引入API
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        // 鼠标事件标志
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000; // 关键标志：使用绝对坐标
    }

    public class MatchResult
    {
        public bool AllMatched { get; set; }
        public List<SingleMatch> Matches { get; set; } = new List<SingleMatch>();
    }

    public class SingleMatch
    {
        public string Target { get; set; }      // 要匹配的目标字符串
        public string MatchedText { get; set; } // 实际匹配到的文本
        public double Similarity { get; set; }  // 相似度百分比
        public int StartIndex { get; set; }     // 在源字符串中的起始位置
        public int Length { get; set; }         // 匹配长度
    }
}
