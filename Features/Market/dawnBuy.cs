using OpenCvSharp;
using OpenCvSharp.Extensions;
using Sdcb.PaddleInference;
using Sdcb.PaddleInference.Native;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace WinFormsApp1
{
    public class dawnBuy
    {
        public PaddleOcrAll myocrService;
        public bool _isLeiGod = false;
        private readonly ScreenCaptureManager _captureManager;
        // swipe configuration
        private readonly int _swipesPerLoop = 1; // 每次循环内滑动次数
        public int _rollSize = 2; // 循环多少次后，进行一次向上滑动
        public Color color = Color.FromArgb(102, 205, 213);
        private readonly int _upSwipeExtra = 1; // 到顶后多滑动次数
        public int _rollNumber = 1; //滑动次数
        private readonly int _swipePause = 5; // 每次滑动后暂停（ms）
        public event Action<TradeItem> ProgressChanged;
        public event Action<string> postMessage;
        public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public dawnBuy(ScreenCaptureManager captureManager)
        {
            InitOcr();
            _captureManager = captureManager;
        }


        private void InitOcr()
        {
            try
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
                       try
                       {
                           device.EnableUseGpu(500, 0);
                           var a = device.UseGpu;
                       }
                       catch (Exception ex)
                       {
                           MessageBox.Show($"启用 GPU 失败，回退到 CPU。错误：{ex.Message}");
                           // 如果 GPU 初始化失败（例如驱动版本不对），回退到 CPU
                           device.UseGpu = false;
                       }
                   });
                myocrService.AllowRotateDetection = false;
                myocrService.Enable180Classification = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show("form2.init" + ex.Message);
                MessageBox.Show("form2.init" + ex.StackTrace);
            }

        }

        public void Run()
        {
            cancellationTokenSource = new CancellationTokenSource();
            var task = new Task(() =>
            {
                int loopCounter = 0;
                while (!cancellationTokenSource.IsCancellationRequested)
                {

                    #region 滑動

                    if (cancellationTokenSource.IsCancellationRequested) return;

                    var sel = _captureManager.CurrentSelection;
                    if (!sel.IsEmpty && sel.Width > 50 && sel.Height > 50)
                    {

                        int centerX = sel.X + sel.Width / 2;

                        int startY = sel.Y + sel.Height - Math.Min(60, sel.Height / 8);
                        for (int i = 0; i < _rollNumber; i++)
                        {
                            CaptureAndRecognize();
                            if (cancellationTokenSource.IsCancellationRequested) return;
                            InputSimulator.MouseWheel(centerX, startY, -1 * _rollSize);
                            Thread.Sleep(_swipePause);
                        }


                        {
                            int upCount = _rollNumber * _rollSize + _upSwipeExtra;
                            for (int u = 0; u < upCount; u++)
                            {
                                if (cancellationTokenSource.IsCancellationRequested) return;

                                InputSimulator.MouseWheel(centerX, startY, 1);
                                Thread.Sleep(_isLeiGod ? 400 : _swipePause);
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
                var list = TradeBorderDetector.DetectBorders(bmp, color);
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
                        Mat matImage = BitmapConverter.ToMat(tb);
                        var result = myocrService.Run(matImage);
                        var trade = CheckProperties(result);
                        if (trade == null) continue;
                        trade.Rect = rect;
                        ProgressChanged?.Invoke(trade);
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
                    var list = TradeBorderDetector.DetectBorders(bmp, color);
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
                            Mat matImage = BitmapConverter.ToMat(tb);
                            var result = myocrService.Run(matImage);
                            var trade = CheckProperties(result);
                            if (trade == null) continue;
                            trade.Rect = rect;
                            ProgressChanged?.Invoke(trade);
                        }
                    }
                }
            }
        }

        public TradeItem? CheckProperties(PaddleOcrResult result)
        {
            try
            {
                var res = result.Regions.ToList();
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
