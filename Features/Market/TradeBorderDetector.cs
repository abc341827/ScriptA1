using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WinFormsApp1
{
    /// <summary>
    /// Detect rectangular regions drawn in the same color (thin borders or filled areas)
    /// using a color-threshold mask and connected-component extraction.
    /// This implementation uses only System.Drawing so it won't add external deps.
    /// </summary>
    public static class TradeBorderDetector
    {
        /// <summary>
        /// Detect candidate rectangles painted with a color similar to <paramref name="targetColor"/>.
        /// </summary>
        /// <param name="bmp">Source bitmap to analyze.</param>
        /// <param name="targetColor">Representative color of the border to find.</param>
        /// <param name="tolerance">Allowed Euclidean distance in RGB (0-255).</param>
        /// <param name="minArea">Minimum connected area (in pixels) to keep a region.</param>
        /// <param name="mergeGap">Merge boxes closer than this gap (pixels).</param>
        /// <returns>List of bounding rectangles sorted top-to-bottom left-to-right.</returns>
        public static List<Rectangle> DetectBorders(Bitmap bmp, Color targetColor, int tolerance = 20, int minArea = 20, int mergeGap = 3)
        {
            if (bmp == null) throw new ArgumentNullException(nameof(bmp));

            int w = bmp.Width;
            int h = bmp.Height;
            var visited = new bool[w, h];
            var mask = new bool[w, h];
            int tol2 = tolerance * tolerance;

            // Build mask where pixels close to targetColor are true
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    int dr = c.R - targetColor.R;
                    int dg = c.G - targetColor.G;
                    int db = c.B - targetColor.B;
                    int d2 = dr * dr + dg * dg + db * db;
                    mask[x, y] = d2 <= tol2;
                }
            }

            var boxes = new List<Rectangle>();

            // Flood-fill connected components on mask
            var stack = new Stack<Point>();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (visited[x, y] || !mask[x, y]) continue;

                    int minX = x, minY = y, maxX = x, maxY = y;
                    int area = 0;
                    stack.Push(new Point(x, y));
                    visited[x, y] = true;

                    while (stack.Count > 0)
                    {
                        var p = stack.Pop();
                        area++;
                        if (p.X < minX) minX = p.X;
                        if (p.Y < minY) minY = p.Y;
                        if (p.X > maxX) maxX = p.X;
                        if (p.Y > maxY) maxY = p.Y;

                        // 4-neighborhood
                        if (p.X > 0)
                        {
                            int nx = p.X - 1, ny = p.Y;
                            if (!visited[nx, ny] && mask[nx, ny]) { visited[nx, ny] = true; stack.Push(new Point(nx, ny)); }
                        }
                        if (p.X + 1 < w)
                        {
                            int nx = p.X + 1, ny = p.Y;
                            if (!visited[nx, ny] && mask[nx, ny]) { visited[nx, ny] = true; stack.Push(new Point(nx, ny)); }
                        }
                        if (p.Y > 0)
                        {
                            int nx = p.X, ny = p.Y - 1;
                            if (!visited[nx, ny] && mask[nx, ny]) { visited[nx, ny] = true; stack.Push(new Point(nx, ny)); }
                        }
                        if (p.Y + 1 < h)
                        {
                            int nx = p.X, ny = p.Y + 1;
                            if (!visited[nx, ny] && mask[nx, ny]) { visited[nx, ny] = true; stack.Push(new Point(nx, ny)); }
                        }
                    }

                    int width = maxX - minX + 1;
                    int height = maxY - minY + 1;
                    if (area >= minArea && width > 2 && height > 2)
                    {
                        boxes.Add(new Rectangle(minX, minY, width, height));
                    }
                }
            }

            // Merge nearby/overlapping rects (repeat until stable)
            boxes = MergeNearbyRects(boxes, mergeGap);

            // sort top-to-bottom then left-to-right
            boxes = boxes.OrderBy(r => r.Top).ThenBy(r => r.Left).ToList();
            return boxes;
        }

        private static List<Rectangle> MergeNearbyRects(List<Rectangle> rects, int gap)
        {
            if (rects == null || rects.Count == 0) return new List<Rectangle>();

            var list = rects.OrderBy(r => r.Left).ThenBy(r => r.Top).ToList();
            bool mergedAny = true;
            while (mergedAny)
            {
                mergedAny = false;
                var result = new List<Rectangle>();
                var used = new bool[list.Count];

                for (int i = 0; i < list.Count; i++)
                {
                    if (used[i]) continue;
                    var a = list[i];
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        if (used[j]) continue;
                        var b = list[j];
                        if (RectsNearbyOrOverlap(a, b, gap))
                        {
                            a = Rectangle.Union(a, b);
                            used[j] = true;
                            mergedAny = true;
                        }
                    }
                    result.Add(a);
                    used[i] = true;
                }

                list = result.OrderBy(r => r.Left).ThenBy(r => r.Top).ToList();
            }

            return list;
        }

        private static bool RectsNearbyOrOverlap(Rectangle a, Rectangle b, int gap)
        {
            // Consider overlap or within gap horizontally/vertically
            var expanded = Rectangle.Inflate(a, gap, gap);
            return expanded.IntersectsWith(b);
        }

        /// <summary>
        /// Helper for testing: run detection on the provided bitmap and return
        /// a new bitmap with detected rectangles drawn in <paramref name="drawColor"/>.
        /// </summary>
        /// <param name="source">Source image to analyze (will not be modified).</param>
        /// <param name="targetColor">Representative color of the border to find.</param>
        /// <param name="tolerance">Color tolerance.</param>
        /// <param name="minArea">Minimum connected area.</param>
        /// <param name="mergeGap">Gap used when merging nearby rects.</param>
        /// <param name="drawColor">Color used to draw rectangles for visualization.</param>
        /// <param name="thickness">Line thickness for drawing.</param>
        /// <returns>Annotated bitmap (caller must dispose).</returns>
        public static Bitmap AnnotateDetectedBorders(Bitmap source, Color targetColor, int tolerance = 20, int minArea = 20, int mergeGap = 3, Color? drawColor = null, int thickness = 2)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Color penColor = drawColor ?? Color.Red;

            // Run detection
            var rects = DetectBorders(source, targetColor, tolerance, minArea, mergeGap);

            // Create a copy to draw on
            var annotated = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(annotated))
            {
                g.DrawImage(source, 0, 0, source.Width, source.Height);
                using (var pen = new Pen(penColor, thickness))
                {
                    pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
                    foreach (var r in rects)
                    {
                        // Expand slightly so thin borders are clearly visible
                        var drawRect = Rectangle.Inflate(r, thickness, thickness);
                        g.DrawRectangle(pen, drawRect);
                    }
                }
            }

            return annotated;
        }

        /// <summary>
        /// Group detected rectangles into rows based on their vertical proximity.
        /// Returns a list of rows; each row is a list of rectangles sorted left-to-right.
        /// </summary>
        /// <param name="rects">Detected rectangles in the image coordinate space.</param>
        /// <param name="yTolerance">Maximum allowed gap (pixels) between a rect and the current row's bottom to consider them the same row.</param>
        /// <returns>Rows of rectangles (each row sorted by Left).</returns>
        public static List<List<Rectangle>> GroupRectanglesIntoRows(List<Rectangle> rects, int yTolerance = 10)
        {
            var rows = new List<List<Rectangle>>();
            if (rects == null || rects.Count == 0) return rows;

            // Sort by top then left to process top-to-bottom
            var list = rects.OrderBy(r => r.Top).ThenBy(r => r.Left).ToList();

            List<Rectangle> currentRow = null;
            int currentMinY = 0, currentMaxY = 0;

            foreach (var r in list)
            {
                if (currentRow == null)
                {
                    currentRow = new List<Rectangle> { r };
                    currentMinY = r.Top;
                    currentMaxY = r.Bottom;
                    continue;
                }

                // If rectangle top is close enough to current row bottom, consider same row
                if (r.Top <= currentMaxY + yTolerance)
                {
                    currentRow.Add(r);
                    if (r.Top < currentMinY) currentMinY = r.Top;
                    if (r.Bottom > currentMaxY) currentMaxY = r.Bottom;
                }
                else
                {
                    // finalize current row
                    currentRow = currentRow.OrderBy(rr => rr.Left).ToList();
                    rows.Add(currentRow);

                    // start new row
                    currentRow = new List<Rectangle> { r };
                    currentMinY = r.Top;
                    currentMaxY = r.Bottom;
                }
            }

            if (currentRow != null && currentRow.Count > 0)
            {
                currentRow = currentRow.OrderBy(rr => rr.Left).ToList();
                rows.Add(currentRow);
            }

            return rows;
        }

        /// <summary>
        /// Get center points for each rectangle grouped by rows.
        /// Useful to obtain per-row positions (e.g. click targets) in image coordinates.
        /// </summary>
        /// <param name="rects">Detected rectangles.</param>
        /// <param name="yTolerance">Vertical tolerance for grouping into rows.</param>
        /// <returns>List of rows where each row is a list of center points (same ordering as grouped rectangles).</returns>
        public static List<List<Point>> GetRowCenters(List<Rectangle> rects, int yTolerance = 10)
        {
            var rows = GroupRectanglesIntoRows(rects, yTolerance);
            var result = new List<List<Point>>();
            foreach (var row in rows)
            {
                var centers = row.Select(r => new Point(r.Left + r.Width / 2, r.Top + r.Height / 2)).ToList();
                result.Add(centers);
            }
            return result;
        }

        /// <summary>
        /// Filter grouped rows and keep only rows that appear "complete".
        /// Useful to discard rows that are partially visible (cut off at image border) or contain fewer than expected columns.
        /// </summary>
        /// <param name="rects">Detected rectangles.</param>
        /// <param name="imageHeight">Height of the source image (pixels) so we can detect rectangles touching top/bottom edges.</param>
        /// <param name="expectedColumns">Expected number of columns per complete row (e.g. 2).</param>
        /// <param name="yTolerance">Vertical tolerance used for initial grouping.</param>
        /// <param name="heightRatioThreshold">Minimum row-average-height relative to global median height to consider complete (0..1).</param>
        /// <param name="rejectIfTouchEdge">If true, rows with any rectangle touching the top or bottom image border are rejected.</param>
        /// <returns>Rows considered complete.</returns>
        public static List<List<Rectangle>> GetCompleteRows(List<Rectangle> rects, int imageHeight, int expectedColumns = 2, int yTolerance = 10, double heightRatioThreshold = 0.7, bool rejectIfTouchEdge = true)
        {
            var rows = GroupRectanglesIntoRows(rects, yTolerance);
            var result = new List<List<Rectangle>>();
            if (rects == null || rects.Count == 0) return result;

            // compute a robust reference height (median)
            var heights = rects.Select(r => r.Height).OrderBy(h => h).ToArray();
            int medianHeight = heights[heights.Length / 2];

            foreach (var row in rows)
            {
                // must have at least expected number of columns
                if (row.Count < expectedColumns) continue;

                // reject rows with rects touching top/bottom borders if requested
                if (rejectIfTouchEdge)
                {
                    bool touchesEdge = row.Any(r => r.Top <= 1 || r.Bottom >= imageHeight - 1);
                    if (touchesEdge) continue;
                }

                // average height check: if row's average height is too small compared to median, it's likely partial
                double avgHeight = row.Average(r => r.Height);
                if (avgHeight < medianHeight * heightRatioThreshold) continue;

                // optionally: ensure columns are roughly aligned by comparing horizontal centers spacing
                // compute centers and check monotonic increasing X
                var centersX = row.Select(r => r.Left + r.Width / 2).OrderBy(x => x).ToArray();
                bool strictlyIncreasing = true;
                for (int i = 1; i < centersX.Length; i++) if (centersX[i] <= centersX[i - 1]) { strictlyIncreasing = false; break; }
                if (!strictlyIncreasing) continue;

                result.Add(row);
            }

            return result;
        }

        /// <summary>
        /// Convenience: get centers for only complete rows.
        /// </summary>
        public static List<List<Point>> GetCompleteRowCenters(List<Rectangle> rects, int imageHeight, int expectedColumns = 2, int yTolerance = 10, double heightRatioThreshold = 0.7, bool rejectIfTouchEdge = true)
        {
            var rows = GetCompleteRows(rects, imageHeight, expectedColumns, yTolerance, heightRatioThreshold, rejectIfTouchEdge);
            var res = new List<List<Point>>();
            foreach (var row in rows)
            {
                res.Add(row.Select(r => new Point(r.Left + r.Width / 2, r.Top + r.Height / 2)).ToList());
            }
            return res;
        }
    }
}
