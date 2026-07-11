
using NAudio.CoreAudioApi;
using NAudio.Utils;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Sdcb.PaddleOCR;

using System.Diagnostics;
using System.Media;
using System.Xml.Linq;

namespace WinFormsApp1
{
    public class CubeAutomationService
    {
        private ScreenCaptureManager _captureManager;
        private readonly IInputController _inputController;
        private readonly IRecordWriter _recordWriter;
        private readonly List<List<string>> _targetProperties = new List<List<string>>();
        public event Action<string, string, Bitmap> ProgressChanged;
        public event Action<string> ProgressCompeleted;
        public PaddleOcrAll myocrService;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
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

        public CubeAutomationService(ScreenCaptureManager captureManager, IInputController? inputController = null, IRecordWriter? recordWriter = null)
        {

            InitOcr();
            _captureManager = captureManager;
            _inputController = inputController ?? new Win32InputController();
            _recordWriter = recordWriter ?? new FileRecordWriter();
        }

        private void InitOcr()
        {
            myocrService = PaddleOcrFactory.CreateDefaultChineseV5();
        }

        public void SetTargetProperties(IEnumerable<IEnumerable<string>> targetProperties)
        {
            _targetProperties.Clear();
            foreach (var targetProperty in targetProperties)
            {
                _targetProperties.Add(targetProperty.ToList());
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
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
            foreach (var src in _targetProperties)
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
            _cancellationTokenSource = new CancellationTokenSource();
            string last = string.Empty;
            string foundResult = string.Empty;
            InitOcr();
            var task = Task.Run(() =>
            {
                try
                {
                    while (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        var current = myGetProperties();
                        last = current;
                        _recordWriter.WriteLine("cube", $"模式: {model}; 识别结果: {current.Trim().Replace(Environment.NewLine, " ").Replace("\n", " ")}");
                        var reslut = CheckProperties(current);
                        if (!string.IsNullOrWhiteSpace(reslut))
                        {
                            _recordWriter.WriteLine("cube", $"匹配成功: {reslut}");
                            // 标记已找到并请求取消，避免继续模拟操作
                            foundResult = reslut;
                            _cancellationTokenSource.Cancel();
                            break;
                        }

                        _inputController.ForceActivateWindow(Handle);
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            break;
                        }

                        // 每次按键后都检查取消，以确保能在检测到后尽快停止
                        void PressSpaceSequence()
                        {
                            _inputController.KeyPress(Keys.Space);
                            _inputController.Delay(150);
                            if (_cancellationTokenSource.IsCancellationRequested) return;
                            _inputController.KeyPress(Keys.Space);
                            _inputController.Delay(150);
                            if (_cancellationTokenSource.IsCancellationRequested) return;
                            _inputController.KeyPress(Keys.Space);
                            _inputController.Delay(150);
                            if (_cancellationTokenSource.IsCancellationRequested) return;
                            _inputController.KeyPress(Keys.Space);
                        }

                        PressSpaceSequence();

                        // 使用可取消的等待，替换 Thread.Sleep
                        for (int waited = 0; waited < delay; waited += 200)
                        {
                            if (_cancellationTokenSource.IsCancellationRequested) break;
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
            }, _cancellationTokenSource.Token);

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

    }
}
