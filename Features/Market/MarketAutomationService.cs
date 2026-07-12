using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;

namespace WinFormsApp1
{
    public class MarketAutomationService
    {
        public IMarketOcrEngine myocrService;
        private readonly ScreenCaptureManager _captureManager;
        private readonly IInputController _inputController;
        private readonly MarketAutomationOptions _options = new MarketAutomationOptions();
        public event Action<TradeItem> ProgressChanged;
        public event Action<string> postMessage;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public MarketAutomationService(ScreenCaptureManager captureManager, IInputController? inputController = null)
        {
            InitOcr();
            _captureManager = captureManager;
            _inputController = inputController ?? new Win32InputController();
        }

        public void Configure(MarketAutomationOptions options)
        {
            _options.IsLeiGod = options.IsLeiGod;
            _options.RollSize = options.RollSize;
            _options.RollNumber = options.RollNumber;
            _options.BorderColor = options.BorderColor;
            _options.SwipePause = options.SwipePause;
            _options.UpSwipeExtra = options.UpSwipeExtra;
            _options.MinItemWidth = options.MinItemWidth;
            _options.MinItemHeight = options.MinItemHeight;
        }

        public void SetScrollOptions(int rollNumber, int rollSize)
        {
            _options.RollNumber = rollNumber;
            _options.RollSize = rollSize;
        }

        public void SetIsLeiGod(bool isLeiGod)
        {
            _options.IsLeiGod = isLeiGod;
        }

        public void SetBorderColor(Color borderColor)
        {
            _options.BorderColor = borderColor;
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }


        private void InitOcr()
        {
            try
            {
                myocrService = OcrEngineFactory.CreateDefault(out var runtimeDescription);
                postMessage?.Invoke($"OCR运行时: {runtimeDescription}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("form2.init" + ex.Message);
                MessageBox.Show("form2.init" + ex.StackTrace);
            }

        }

        public void Run()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var task = new Task(() =>
            {
                int loopCounter = 0;
                while (!_cancellationTokenSource.IsCancellationRequested)
                {

                    #region 滑動

                    if (_cancellationTokenSource.IsCancellationRequested) return;

                    var sel = _captureManager.CurrentSelection;
                    if (!sel.IsEmpty && sel.Width > 50 && sel.Height > 50)
                    {

                        int centerX = sel.X + sel.Width / 2;

                        int startY = sel.Y + sel.Height - Math.Min(60, sel.Height / 8);
                        for (int i = 0; i < _options.RollNumber; i++)
                        {
                            CaptureAndRecognize();
                            if (_cancellationTokenSource.IsCancellationRequested) return;
                            _inputController.MouseWheel(centerX, startY, -1 * _options.RollSize);
                            Thread.Sleep(_options.SwipePause);
                        }


                        {
                            int upCount = _options.RollNumber * _options.RollSize + _options.UpSwipeExtra;
                            for (int u = 0; u < upCount; u++)
                            {
                                if (_cancellationTokenSource.IsCancellationRequested) return;

                                _inputController.MouseWheel(centerX, startY, 1);
                                Thread.Sleep(_options.IsLeiGod ? 400 : _options.SwipePause);
                            }
                            postMessage?.Invoke("执行向上滑动以回到顶部");
                        }
                    }
                    #endregion
                }

            });
            task.Start();
        }

        public void CaptureAndRecognize()
        {
            // CaptureSelectedArea / CaptureArea may return null if selection is invalid.
            // Make null-safe to avoid unexpected exceptions and ensure predictable flow.
            var bmp = _captureManager.CaptureSelectedArea();
            if (bmp == null) return;

            using (bmp)
            {
                var list = TradeBorderDetector.DetectBorders(bmp, _options.BorderColor)
                    .Where(IsRecognizableItemRectangle)
                    .ToList();
                if (list == null || list.Count == 0) return;

                foreach (var item in list)
                {
                    var x = item.X + _captureManager.CurrentSelection.X;
                    var y = item.Y + _captureManager.CurrentSelection.Y;
                    var rect = new Rectangle(x, y, item.Width, item.Height);
                    var tb = _captureManager.CaptureArea(rect);
                    if (tb == null) continue;
                    using (tb)
                    {
                            try
                            {
                                using Mat matImage = BitmapConverter.ToMat(tb);
                                if (matImage.Empty()) continue;

                                var result = myocrService.Recognize(matImage);
                                var trade = CheckProperties(result);
                                if (trade == null) continue;
                                trade.Rect = rect;
                                ProgressChanged?.Invoke(trade);
                            }
                            catch (Exception ex)
                            {
                                postMessage?.Invoke($"OCR识别失败，已跳过候选框 X={rect.X}, Y={rect.Y}, W={rect.Width}, H={rect.Height}。错误: {ex.Message}");
                            }
                    }
                }
            }
        }
        public void CaptureAndRecognizeTest()
        {
            {
                // CaptureSelectedArea / CaptureArea may return null if selection is invalid.
                // Make null-safe to avoid unexpected exceptions and ensure predictable flow.
                var bmp = _captureManager.CaptureSelectedArea();
                if (bmp == null) return;

                using (bmp)
                {
                    var list = TradeBorderDetector.DetectBorders(bmp, _options.BorderColor)
                        .Where(IsRecognizableItemRectangle)
                        .ToList();
                    if (list == null || list.Count == 0) return;

                    foreach (var item in list)
                    {
                        var x = item.X + _captureManager.CurrentSelection.X;
                        var y = item.Y + _captureManager.CurrentSelection.Y;
                        var rect = new Rectangle(x, y, item.Width, item.Height);
                        var tb = _captureManager.CaptureArea(rect);
                        if (tb == null) continue;
                        using (tb)
                        {
                                try
                                {
                                    using Mat matImage = BitmapConverter.ToMat(tb);
                                    if (matImage.Empty()) continue;

                                    var result = myocrService.Recognize(matImage);
                                    var trade = CheckProperties(result);
                                    if (trade == null) continue;
                                    trade.Rect = rect;
                                    ProgressChanged?.Invoke(trade);
                                }
                                catch (Exception ex)
                                {
                                    postMessage?.Invoke($"OCR识别失败，已跳过候选框 X={rect.X}, Y={rect.Y}, W={rect.Width}, H={rect.Height}。错误: {ex.Message}");
                                }
                        }
                    }
                }
            }
        }

        private bool IsRecognizableItemRectangle(Rectangle rectangle)
        {
            return rectangle.Width >= _options.MinItemWidth && rectangle.Height >= _options.MinItemHeight;
        }

        public TradeItem? CheckProperties(IReadOnlyList<OcrTextLine> result)
        {
            try
            {
                var res = result.ToList();
                res.RemoveAll(x => x.Score < 0.75);

                var itemName = res[0].Text;
                var itemPrice = res[1].Text;
                var All = string.Empty;
                var lowLeft = string.Empty;
                if (itemPrice != "售罄")
                {
                    All = res[2].Text;
                    lowLeft = res[3].Text;
                }
                else
                {
                    itemPrice = "100000000";
                    lowLeft = res[2].Text;
                }
                itemPrice = ExtractNumber(itemPrice);
                return new TradeItem
                {
                    Name = itemName,
                    Price = int.Parse(itemPrice),
                    All = All,
                    LowLeft = lowLeft,
                };
            }
            catch (Exception)
            {
                return null;
            }

        }

        public string ExtractNumber(string input)
        {
            input = input.Trim().Replace('，', ',').Replace('。', '.');

            var m = Regex.Match(input, @"[\d\.,]+$");
            if (m.Success)
            {
                string trailing = m.Value; // "1,234.56"
                return Regex.Replace(trailing, @"[^\d]", "");
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
