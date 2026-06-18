using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;


namespace OpenCV_TemplateMatching
{
    public partial class Form1 : Form
    {
        private Mat sourceImage;
        private Dictionary<int, TemplateData> templates;
        private List<DetectionResult> allDetections;
        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            templates = new Dictionary<int, TemplateData>();
            allDetections = new List<DetectionResult>();

        }
        private class TemplateData
        {
            public List<Mat> Images { get; set; }
            public Mat BestImage { get; set; }
            public string FilePath { get; set; }
            public string TemplateName { get; set; }
            public Point CalibrationPoint { get; set; }
            public bool IsCalibrated { get; set; }
            public Rectangle DetectedRect { get; set; }
            public Point Offset { get; set; }
            public int TemplateId { get; set; }
        }
        private class DetectionResult
        {
            public int TemplateId { get; set; }
            public string TemplateName { get; set; }
            public Point Location { get; set; }
            public Point Offset { get; set; }
            public double Confidence { get; set; }
            public Rectangle BoundingBox { get; set; }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void Load_Source_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    sourceImage = new Mat(ofd.FileName, ImreadModes.Color);
                    pictureBox_Source.Image = BitmapConverter.ToBitmap(sourceImage);

                    // Reset semua deteksi ketika source berubah
                    ClearAllDetections();
                }
            }
        }

        private void Load_Template1_Click(object sender, EventArgs e)
        {
            LoadTemplate(1, pictureBox1);
        }

        private void Load_Template2_Click(object sender, EventArgs e)
        {
            LoadTemplate(2, pictureBox2);
        }

        private void Load_Template3_Click(object sender, EventArgs e)
        {
            LoadTemplate(3, pictureBox3);
        }
        private void LoadTemplate(int templateId, PictureBox targetPictureBox)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Mat templateMat = new Mat(ofd.FileName, ImreadModes.Color);
                    string templateName = Path.GetFileNameWithoutExtension(ofd.FileName);

                    // Normalisasi nama: hapus _ jika ada
                    if (templateName.EndsWith("_"))
                        templateName = templateName.TrimEnd('_');

                    // Buat list dengan 1 varian
                    List<Mat> variantImages = new List<Mat> { templateMat };

                    // BestImage = templateMat (karena cuma 1)
                    Mat bestImage = templateMat;

                    templates[templateId] = new TemplateData
                    {
                        Images = variantImages,
                        BestImage = bestImage,             // Perbaikan
                        FilePath = ofd.FileName,           // Perbaikan
                        TemplateName = templateName,       //  Perbaikan
                        IsCalibrated = false,
                        TemplateId = templateId
                    };

                    targetPictureBox.Image = BitmapConverter.ToBitmap(templateMat);
                    targetPictureBox.Tag = templateId;

                    ClearDetectionsForTemplate(templateId);
                }
            }
        }


        private void ApplyOverlapFilter()
        {
            if (allDetections.Count <= 1) return;

            // URUTKAN: Template BESAR dulu, baru confidence
            var sorted = allDetections
                .OrderByDescending(d => d.BoundingBox.Width * d.BoundingBox.Height)
                .ThenByDescending(d => d.Confidence)
                .ToList();

            List<DetectionResult> filtered = new List<DetectionResult>();

            for (int i = 0; i < sorted.Count; i++)
            {
                bool keep = true;
                Rect currentRect = new Rect(
                    sorted[i].BoundingBox.X,
                    sorted[i].BoundingBox.Y,
                    sorted[i].BoundingBox.Width,
                    sorted[i].BoundingBox.Height
                );

                double currentArea = currentRect.Width * currentRect.Height;

                // Cek overlap dengan yang SUDAH di filtered
                for (int j = filtered.Count - 1; j >= 0; j--) // Loop terbalik untuk menghindari masalah index
                {
                    var existing = filtered[j];
                    Rect existingRect = new Rect(
                        existing.BoundingBox.X,
                        existing.BoundingBox.Y,
                        existing.BoundingBox.Width,
                        existing.BoundingBox.Height
                    );

                    Rect intersection = currentRect.Intersect(existingRect);

                    if (intersection.Width > 0 && intersection.Height > 0)
                    {
                        double overlapArea = intersection.Width * intersection.Height;
                        double existingArea = existingRect.Width * existingRect.Height;
                        double smallerArea = Math.Min(currentArea, existingArea);

                        // Overlap > 10% dari area terkecil
                        if (overlapArea / smallerArea > 0.3)
                        {
                            // Hitung rasio ukuran
                            double sizeRatio = Math.Max(currentArea, existingArea) / Math.Min(currentArea, existingArea);

                            // Jika ukuran sangat berbeda (rasio > 3x)
                            if (sizeRatio > 3.0)
                            {
                                if (currentArea > existingArea)
                                {
                                    // Current lebih besar → hapus existing
                                    filtered.RemoveAt(j);
                                    Console.WriteLine($"[OVERLAP SIZE] {existing.TemplateName} (C:{existing.Confidence:F3}, area:{existingArea:F0}) " +
                                        $"replaced by {sorted[i].TemplateName} (C:{sorted[i].Confidence:F3}, area:{currentArea:F0}) - size ratio {sizeRatio:F1}x");
                                }
                                else
                                {
                                    // Existing lebih besar → skip current
                                    keep = false;
                                    Console.WriteLine($"[OVERLAP SIZE] {sorted[i].TemplateName} (C:{sorted[i].Confidence:F3}, area:{currentArea:F0}) " +
                                        $"skipped - {existing.TemplateName} is {sizeRatio:F1}x larger");
                                    break;
                                }
                            }
                            else
                            {
                                // Ukuran mirip → gunakan confidence
                                if (existing.Confidence >= sorted[i].Confidence)
                                {
                                    keep = false;
                                    Console.WriteLine($"[OVERLAP CONF] {sorted[i].TemplateName} (C:{sorted[i].Confidence:F3}) " +
                                        $"removed - overlaps with {existing.TemplateName} (C:{existing.Confidence:F3})");
                                    break;
                                }
                                else
                                {
                                    filtered.RemoveAt(j);
                                    Console.WriteLine($"[OVERLAP CONF] {existing.TemplateName} (C:{existing.Confidence:F3}) " +
                                        $"replaced by {sorted[i].TemplateName} (C:{sorted[i].Confidence:F3})");
                                }
                            }
                        }
                    }
                }

                if (keep)
                    filtered.Add(sorted[i]);
            }

            allDetections = filtered;
        }

        private void Process_Click(object sender, EventArgs e)
        {
            if (sourceImage == null || sourceImage.Empty())
            {
                MessageBox.Show("Source image belum di-load!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (templates.Count == 0)
            {
                MessageBox.Show("Minimal 1 template harus di-load!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                // === DETEKSI QR CODE ===
                List<Rect> qrAreas = DetectQRCodeAreas();

                Mat displayImage = sourceImage.Clone();
                allDetections.Clear();

                var sortedTemplates = templates.Values
                    .OrderByDescending(t => {
                        if (t.Images.Count > 0)
                            return t.Images[0].Width * t.Images[0].Height;
                        return 0;
                    })
                    .ToList();

                // === MULTITHREADING ===
                object lockObj = new object();
                List<DetectionResult> parallelDetections = new List<DetectionResult>();

                Parallel.ForEach(sortedTemplates, new ParallelOptions
                {
                    MaxDegreeOfParallelism = 5 //Environment.ProcessorCount
                },
                template =>
                {
                    List<DetectionResult> localDetections = new List<DetectionResult>();
                    ProcessTemplateParallel(template, localDetections);

                    lock (lockObj)
                    {
                        parallelDetections.AddRange(localDetections);
                    }
                });

                allDetections = parallelDetections;

                // === POST-PROCESSING (ganti overlap yang hilang saat paralel) ===
                ApplyOverlapFilter();      // ← Hapus overlap (MTA & A)
                ApplyDominanceFilter();    // ← Hapus yang lebih lemah

                // === FILTER QR: Hapus deteksi yang di dalam area QR ===
                if (qrAreas.Count > 0)
                {
                    int beforeCount = allDetections.Count;
                    allDetections = allDetections
                        .Where(d => !IsInsideQRArea(
                            new Rect(d.BoundingBox.X, d.BoundingBox.Y,
                                     d.BoundingBox.Width, d.BoundingBox.Height),
                            qrAreas))
                        .ToList();

                    Console.WriteLine($"[QR FILTER] Removed {beforeCount - allDetections.Count} detections inside QR area");
                }

                // Gambar area QR (kotak kuning)
                foreach (var qrArea in qrAreas)
                {
                    Cv2.Rectangle(displayImage, qrArea, Scalar.Yellow, 2);
                    Cv2.PutText(displayImage, "QR EXCLUDED",
                        new Point(qrArea.X, qrArea.Y - 5),
                        HersheyFonts.HersheySimplex, 0.6, Scalar.Yellow, 2);
                }


                // === GAMBAR BOUNDING BOX ===
                foreach (var detection in allDetections)
                {
                    var rect = new Rect(detection.BoundingBox.X, detection.BoundingBox.Y,
                                        detection.BoundingBox.Width, detection.BoundingBox.Height);
                    Scalar color = GetColorForTemplate(detection.TemplateId);
                    Cv2.Rectangle(displayImage, rect, color, 2);

                    double truncatedConfidence = Math.Truncate(detection.Confidence * 100) / 100;
                    string label = $"{detection.TemplateName} | C:{truncatedConfidence:F2}";
                    Cv2.PutText(displayImage, label,
                        new Point(detection.Location.X, detection.Location.Y - 5),
                        HersheyFonts.HersheySimplex, 0.5, color, 1);
                }

                // === SORTING & GROUPING ===
                int rowTolerance = 25;
                var sortedByY = allDetections.OrderBy(d => d.Location.Y).ToList();

                List<List<DetectionResult>> rows = new List<List<DetectionResult>>();
                foreach (var detection in sortedByY)
                {
                    bool addedToRow = false;
                    foreach (var row in rows)
                    {
                        if (Math.Abs(row[0].Location.Y - detection.Location.Y) <= rowTolerance)
                        {
                            row.Add(detection);
                            addedToRow = true;
                            break;
                        }
                    }
                    if (!addedToRow)
                        rows.Add(new List<DetectionResult> { detection });
                }

                foreach (var row in rows)
                    row.Sort((a, b) => a.Location.X.CompareTo(b.Location.X));

                allDetections = rows.SelectMany(r => r).ToList();

                pictureBox_Source.Image = BitmapConverter.ToBitmap(displayImage);
                ShowDetectionSummary();
                BuildFinalTextResult();

                sw.Stop();
                Console.WriteLine($"");
                Console.WriteLine($"========================================");
                Console.WriteLine($"  TOTAL PROCESS TIME: {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Templates processed: {templates.Count}");
                Console.WriteLine($"  Detections found: {allDetections.Count}");
                Console.WriteLine($"========================================");

                displayImage.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat proses: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Versi paralel dari ProcessTemplate - hanya deteksi, tanpa menggambar di displayImage
        /// </summary>
        private void ProcessTemplateParallel(TemplateData template, List<DetectionResult> localDetections)
        {
            // ============================================
            // TAHAP 1: PRE-SCREENING (varian terbaik saja)
            // ============================================
            Mat bestVariant = template.BestImage ?? template.Images[0];

            int preCols = sourceImage.Cols - bestVariant.Cols + 1;
            int preRows = sourceImage.Rows - bestVariant.Rows + 1;

            if (preCols <= 0 || preRows <= 0)
                return;

            Mat preResult = new Mat(preRows, preCols, MatType.CV_32FC1);
            Cv2.MatchTemplate(sourceImage, bestVariant, preResult, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(preResult, out _, out double preBestConf, out _, out _);
            preResult.Dispose();

            // Jika varian terbaik saja sudah rendah → SKIP SEMUA
            if (preBestConf < 0.75)
                return;

            // ============================================
            // TAHAP 2: FULL MATCHING (semua varian)
            // ============================================
            List<(Point location, double confidence, Mat bestTemplate)> allCandidates =
                new List<(Point, double, Mat)>();

            // Jika pre-screening sudah SANGAT TINGGI (>0.97), cukup pakai 2 varian terbaik
            int maxVariants = (preBestConf > 0.97) ? Math.Min(2, template.Images.Count) : template.Images.Count;

            foreach (Mat variantImage in template.Images.Take(maxVariants))
            {
                if (variantImage.Width > sourceImage.Width || variantImage.Height > sourceImage.Height)
                    continue;

                int resultCols = sourceImage.Cols - variantImage.Cols + 1;
                int resultRows = sourceImage.Rows - variantImage.Rows + 1;

                if (resultCols <= 0 || resultRows <= 0)
                    continue;

                Mat result = new Mat(resultRows, resultCols, MatType.CV_32FC1);
                Cv2.MatchTemplate(sourceImage, variantImage, result, TemplateMatchModes.CCoeffNormed);

                double threshold = 0.85;

                float[] resultArray = new float[result.Rows * result.Cols];
                result.GetArray<float>(out resultArray);

                for (int idx = 0; idx < resultArray.Length; idx++)
                {
                    if (resultArray[idx] >= threshold)
                    {
                        int y = idx / result.Cols;
                        int x = idx % result.Cols;
                        allCandidates.Add((new Point(x, y), (double)resultArray[idx], variantImage));
                    }
                }

                result.Dispose();
            }

            // === FALLBACK ===
            if (allCandidates.Count == 0)
            {
                double bestConfidence = 0;
                Point bestLocation = new Point();
                Mat bestVariantFallback = null;

                foreach (Mat variantImage in template.Images)
                {
                    int resultCols = sourceImage.Cols - variantImage.Cols + 1;
                    int resultRows = sourceImage.Rows - variantImage.Rows + 1;

                    if (resultCols <= 0 || resultRows <= 0)
                        continue;

                    Mat result = new Mat(resultRows, resultCols, MatType.CV_32FC1);
                    Cv2.MatchTemplate(sourceImage, variantImage, result, TemplateMatchModes.CCoeffNormed);
                    Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out Point maxLoc);

                    if (maxVal > bestConfidence)
                    {
                        bestConfidence = maxVal;
                        bestLocation = maxLoc;
                        bestVariantFallback = variantImage;
                    }

                    result.Dispose();
                }

                if (bestConfidence >= 0.90)
                {
                    allCandidates.Add((bestLocation, bestConfidence, bestVariantFallback));
                }
                else
                {
                    return;
                }
            }

            // === NMS ===
            allCandidates.Sort((a, b) => b.confidence.CompareTo(a.confidence));

            List<Point> matchLocations = new List<Point>();
            List<double> matchConfidences = new List<double>();

            while (allCandidates.Count > 0)
            {
                var best = allCandidates[0];

                matchLocations.Add(best.location);
                matchConfidences.Add(best.confidence);

                int minDistance = Math.Min(best.bestTemplate.Width, best.bestTemplate.Height) / 2;
                allCandidates.RemoveAll(c =>
                    Math.Abs(c.location.X - best.location.X) < minDistance &&
                    Math.Abs(c.location.Y - best.location.Y) < minDistance
                );
            }

            // === SIMPAN HASIL ===
            int matchCount = 1;
            for (int i = 0; i < matchLocations.Count; i++)
            {
                Point matchLoc = matchLocations[i];
                double confidence = matchConfidences[i];
                Mat bestTemplate = template.Images[0];

                if (!ValidateMatch(template, matchLoc, bestTemplate, confidence))
                    continue;

                if (!template.IsCalibrated)
                {
                    template.CalibrationPoint = matchLoc;
                    template.IsCalibrated = true;
                }

                Point offset = new Point(matchLoc.X - template.CalibrationPoint.X,
                                          matchLoc.Y - template.CalibrationPoint.Y);

                localDetections.Add(new DetectionResult
                {
                    TemplateId = template.TemplateId,
                    TemplateName = template.TemplateName,
                    Location = matchLoc,
                    Offset = offset,
                    Confidence = confidence,
                    BoundingBox = new Rectangle(matchLoc.X, matchLoc.Y,
                                                bestTemplate.Width, bestTemplate.Height)
                });

                matchCount++;
            }
        }

        #region old
        //private void ProcessTemplate(TemplateData template, Mat displayImage)
        //{
        //    // === COBA SEMUA VARIAN TEMPLATE, AMBIL CONFIDENCE TERTINGGI ===

        //    List<(Point location, double confidence, Mat bestTemplate)> allCandidates = new List<(Point, double, Mat)>();

        //    foreach (Mat variantImage in template.Images)
        //    {
        //        // Validasi ukuran
        //        if (variantImage.Width > sourceImage.Width || variantImage.Height > sourceImage.Height)
        //            continue;

        //        // Template matching untuk varian ini
        //        int resultCols = sourceImage.Cols - variantImage.Cols + 1;
        //        int resultRows = sourceImage.Rows - variantImage.Rows + 1;
        //        Mat result = new Mat(resultRows, resultCols, MatType.CV_32FC1);

        //        Cv2.MatchTemplate(sourceImage, variantImage, result, TemplateMatchModes.CCoeffNormed);

        //        double threshold = 0.85;

        //        for (int y = 0; y < result.Rows; y++)
        //        {
        //            for (int x = 0; x < result.Cols; x++)
        //            {
        //                double confidence = result.At<float>(y, x);
        //                if (confidence >= threshold)
        //                {
        //                    allCandidates.Add((new Point(x, y), confidence, variantImage));
        //                }
        //            }
        //        }

        //        result.Dispose();
        //    }

        //    // Jika tidak ada kandidat dari varian manapun, fallback ke best match dari semua varian
        //    if (allCandidates.Count == 0)
        //    {
        //        double bestConfidence = 0;
        //        Point bestLocation = new Point();
        //        Mat bestVariant = null;

        //        foreach (Mat variantImage in template.Images)
        //        {
        //            int resultCols = sourceImage.Cols - variantImage.Cols + 1;
        //            int resultRows = sourceImage.Rows - variantImage.Rows + 1;

        //            Mat result = new Mat(resultRows, resultCols, MatType.CV_32FC1);

        //            Cv2.MatchTemplate(
        //                sourceImage,
        //                variantImage,
        //                result,
        //                TemplateMatchModes.CCoeffNormed
        //            );

        //            Cv2.MinMaxLoc(
        //                result,
        //                out double minVal,
        //                out double maxVal,
        //                out Point minLoc,
        //                out Point maxLoc
        //            );

        //            // simpan hanya yang terbaik
        //            if (maxVal > bestConfidence)
        //            {
        //                bestConfidence = maxVal;
        //                bestLocation = maxLoc;
        //                bestVariant = variantImage;
        //            }

        //            result.Dispose();
        //        }

        //        Console.WriteLine(
        //            $"{template.TemplateName}: No match above threshold. " +
        //            $"Best confidence = {bestConfidence:F4}"
        //        );

        //        // ======================================
        //        // HARD MINIMUM CONFIDENCE
        //        // ======================================
        //        double hardMinimumConfidence = 0.90;

        //        if (bestConfidence >= hardMinimumConfidence)
        //        {
        //            Console.WriteLine(
        //                $"[FALLBACK ACCEPTED] {template.TemplateName} " +
        //                $"Confidence: {bestConfidence:F4}"
        //            );

        //            allCandidates.Add((
        //                bestLocation,
        //                bestConfidence,
        //                bestVariant
        //            ));
        //        }
        //        else
        //        {
        //            Console.WriteLine(
        //                $"[FALLBACK REJECTED] {template.TemplateName} " +
        //                $"Confidence too low: {bestConfidence:F4}"
        //            );
        //        }
        //    }

        //    // Non-Maximum Suppression (NMS)
        //    allCandidates.Sort((a, b) => b.confidence.CompareTo(a.confidence));

        //    List<Point> matchLocations = new List<Point>();
        //    List<double> matchConfidences = new List<double>();
        //    List<Mat> bestTemplates = new List<Mat>();

        //    while (allCandidates.Count > 0)
        //    {
        //        var best = allCandidates[0];

        //        // Cek overlap dengan existing detections
        //        Rect newRect = new Rect(best.location.X, best.location.Y,
        //                                best.bestTemplate.Width, best.bestTemplate.Height);

        //        bool isOverlapping = false;
        //        foreach (var existingDetection in allDetections)
        //        {
        //            Rect existingRect = new Rect(
        //                existingDetection.BoundingBox.X,
        //                existingDetection.BoundingBox.Y,
        //                existingDetection.BoundingBox.Width,
        //                existingDetection.BoundingBox.Height
        //            );

        //            Rect intersection = newRect.Intersect(existingRect);

        //            if (intersection.Width > 0 && intersection.Height > 0)
        //            {
        //                double overlapArea = intersection.Width * intersection.Height;
        //                double newRectArea = newRect.Width * newRect.Height;
        //                double existingRectArea = existingRect.Width * existingRect.Height;
        //                double smallerArea = Math.Min(newRectArea, existingRectArea);

        //                if (overlapArea / smallerArea > 0.1)
        //                {
        //                    isOverlapping = true;
        //                    break;
        //                }
        //            }
        //        }

        //        if (!isOverlapping)
        //        {
        //            matchLocations.Add(best.location);
        //            matchConfidences.Add(best.confidence);
        //            bestTemplates.Add(best.bestTemplate);
        //        }

        //        // Hapus kandidat ini dan yang terlalu dekat
        //        int minDistance = Math.Min(best.bestTemplate.Width, best.bestTemplate.Height) / 2;
        //        allCandidates.RemoveAll(c =>
        //            Math.Abs(c.location.X - best.location.X) < minDistance &&
        //            Math.Abs(c.location.Y - best.location.Y) < minDistance
        //        );
        //    }

        //    // Proses setiap lokasi yang terdeteksi
        //    for (int i = 0; i < matchLocations.Count; i++)
        //    {
        //        Point matchLoc = matchLocations[i];
        //        double confidence = matchConfidences[i];
        //        Mat bestTemplate = bestTemplates[i];

        //        // ========== FINAL VALIDATION ==========
        //        if (!ValidateMatch(template, matchLoc, bestTemplate, confidence))
        //        {
        //            Console.WriteLine($"{template.TemplateName} - Match #{i + 1}: FAILED final validation, skipped");
        //            continue;
        //        }
        //        // =====================================

        //        // Kalibrasi
        //        if (!template.IsCalibrated)
        //        {
        //            template.CalibrationPoint = matchLoc;
        //            template.IsCalibrated = true;
        //            Console.WriteLine($"{template.TemplateName} calibrated at: ({matchLoc.X}, {matchLoc.Y})");
        //        }

        //        Point offset = new Point(matchLoc.X - template.CalibrationPoint.X,
        //                                  matchLoc.Y - template.CalibrationPoint.Y);

        //        Console.WriteLine($"{template.TemplateName} - Match #{i + 1}:");
        //        Console.WriteLine($"  Position: ({matchLoc.X}, {matchLoc.Y})");
        //        Console.WriteLine($"  Confidence: {confidence:F4}");
        //        Console.WriteLine($"  Offset: ({offset.X}, {offset.Y})");

        //        var detection = new DetectionResult
        //        {
        //            TemplateId = template.TemplateId,
        //            TemplateName = template.TemplateName,
        //            Location = matchLoc,
        //            Offset = offset,
        //            Confidence = confidence,
        //            BoundingBox = new Rectangle(matchLoc.X, matchLoc.Y, bestTemplate.Width, bestTemplate.Height)
        //        };
        //        allDetections.Add(detection);

        //        // Gambar bounding box
        //        var rect = new Rect(matchLoc.X, matchLoc.Y, bestTemplate.Width, bestTemplate.Height);
        //        Scalar color = GetColorForTemplate(template.TemplateId);
        //        Cv2.Rectangle(displayImage, rect, color, 2);

        //        using (Mat roiOriginal = new Mat(sourceImage, rect))
        //        using (Mat roiPreprocessed = PreprocessForMatching(roiOriginal))
        //        {
        //            // Resize kembali ke ukuran asli kalau perlu
        //            if (roiPreprocessed.Size() != roiOriginal.Size())
        //            {
        //                Cv2.Resize(roiPreprocessed, roiPreprocessed, roiOriginal.Size());
        //            }

        //            // Copy hasil preprocessing ke displayImage
        //            //roiPreprocessed.CopyTo(new Mat(displayImage, rect));
        //        }

        //        // Tulis label di atas bounding box
        //        double truncatedConfidence = Math.Truncate(confidence * 100) / 100;
        //        string label = $"{template.TemplateName}-{i + 1} | C:{truncatedConfidence:F2}";
        //        Cv2.PutText(displayImage, label,
        //            new Point(matchLoc.X, matchLoc.Y - 5),
        //            HersheyFonts.HersheySimplex, 0.5, color, 1);

        //    }
        //}

        private Mat PreprocessForMatching(Mat image)
        {
            Mat gray = new Mat();
            Mat binary = new Mat();
            Mat result = new Mat();

            // Convert ke grayscale
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

            // Gaussian blur
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(3, 3), 0);

            // Adaptive threshold
            Cv2.AdaptiveThreshold(gray, binary, 255,
                AdaptiveThresholdTypes.GaussianC,
                ThresholdTypes.Binary, 11, 2);

            // Morphological closing
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(2, 2));
            Cv2.MorphologyEx(binary, binary, MorphTypes.Close, kernel);

            // Kembalikan ke BGR
            Cv2.CvtColor(binary, result, ColorConversionCodes.GRAY2BGR);

            gray.Dispose();
            binary.Dispose();
            kernel.Dispose();

            return result;
        }
        #endregion

        private bool ValidateMatch(TemplateData template, Point matchLoc, Mat bestTemplate, double confidence)
        {
            // === CHECK 1: Confidence threshold ADAPTIF ===
            // Template besar (width > 100px) punya threshold lebih rendah
            double minConfidence = bestTemplate.Width > 100 ? 0.60 : 0.80;

            Console.WriteLine($"  [VALIDATION THRESHOLD] {template.TemplateName}: MinConfidence = {minConfidence:F2} " + $"(template width: {bestTemplate.Width}px)");

            if (confidence < minConfidence)
            {
                Console.WriteLine($"  [VALIDATION FAIL] {template.TemplateName}: Confidence too low ({confidence:F4} < {minConfidence:F2})");
                return false;
            }

            // === CHECK 2: Cek apakah area match dalam batas gambar ===
            Rect matchRect = new Rect(matchLoc.X, matchLoc.Y, bestTemplate.Width, bestTemplate.Height);

            if (matchRect.X < 0 || matchRect.Y < 0 ||
                matchRect.X + matchRect.Width > sourceImage.Width ||
                matchRect.Y + matchRect.Height > sourceImage.Height)
            {
                Console.WriteLine($"  [VALIDATION FAIL] {template.TemplateName}: Match out of bounds");
                return false;
            }

            using (Mat roi = new Mat(sourceImage, matchRect))
            {
                // === CHECK 3: Pixel Difference - SKIP untuk template besar ===
                if (bestTemplate.Width <= 100) // Hanya untuk karakter kecil
                {
                    Mat roiGray = new Mat();
                    Mat templateGray = new Mat();
                    Cv2.CvtColor(roi, roiGray, ColorConversionCodes.BGR2GRAY);
                    Cv2.CvtColor(bestTemplate, templateGray, ColorConversionCodes.BGR2GRAY);

                    Mat diff = new Mat();
                    Cv2.Absdiff(roiGray, templateGray, diff);

                    Scalar meanDiff = Cv2.Mean(diff);
                    double avgPixelDiff = meanDiff.Val0 / 255.0;

                    roiGray.Dispose();
                    templateGray.Dispose();
                    diff.Dispose();

                    if (avgPixelDiff > 0.35)
                    {
                        Console.WriteLine($"  [VALIDATION FAIL] {template.TemplateName}: Pixel difference too high ({avgPixelDiff:F3} > 0.35)");
                        return false;
                    }
                }

                // === CHECK 4: Cross-validation - SKIP untuk template besar ===
                if (bestTemplate.Width <= 100 && IsSimilarCharacter(template.TemplateName))
                {
                    var competitors = GetSimilarCharacters(template.TemplateName);

                    foreach (var otherTemplate in templates.Values)
                    {
                        if (!competitors.Contains(otherTemplate.TemplateName))
                            continue;

                        foreach (Mat otherVariant in otherTemplate.Images)
                        {
                            if (Math.Abs(otherVariant.Width - bestTemplate.Width) > 5 ||
                                Math.Abs(otherVariant.Height - bestTemplate.Height) > 5)
                                continue;

                            if (matchLoc.X + otherVariant.Width > sourceImage.Width ||
                                matchLoc.Y + otherVariant.Height > sourceImage.Height)
                                continue;

                            using (Mat checkRoi = new Mat(sourceImage,
                                new Rect(matchLoc.X, matchLoc.Y, otherVariant.Width, otherVariant.Height)))
                            {
                                int resCols = checkRoi.Cols - otherVariant.Cols + 1;
                                int resRows = checkRoi.Rows - otherVariant.Rows + 1;

                                if (resCols <= 0 || resRows <= 0)
                                    continue;

                                Mat crossResult = new Mat(resRows, resCols, MatType.CV_32FC1);
                                Cv2.MatchTemplate(checkRoi, otherVariant, crossResult, TemplateMatchModes.CCoeffNormed);
                                Cv2.MinMaxLoc(crossResult, out _, out double otherConf, out _, out _);
                                crossResult.Dispose();

                                if (otherConf > confidence + 0.02)
                                {
                                    Console.WriteLine($"  [VALIDATION FAIL] {template.TemplateName}: More likely '{otherTemplate.TemplateName}' ({otherConf:F4} > {confidence:F4})");
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        private bool IsSimilarCharacter(string templateName)
        {
            Dictionary<string, List<string>> similarChars = new Dictionary<string, List<string>>
    {
        { "B", new List<string> { "8" } },
    { "8", new List<string> { "B", "E", "6" } },  //  Tambah 6
    { "6", new List<string> { "C", "0", "8" } },  //  Tambah 8
    { "D", new List<string> { "0" } },
    { "0", new List<string> { "D" } },
    { "E", new List<string> { "F", "8" } },
    { "F", new List<string> { "E" } },
    { "C", new List<string> { "0", "6" } },
    { "5", new List<string> { "6" } },
    };

            return similarChars.ContainsKey(templateName);
        }

        /// <summary>
        /// Dapatkan daftar karakter mirip untuk cross-validation
        /// </summary>
        private List<string> GetSimilarCharacters(string templateName)
        {
            Dictionary<string, List<string>> similarChars = new Dictionary<string, List<string>>
    {
        { "B", new List<string> { "8" } },
    { "8", new List<string> { "B", "E", "6" } },  //  Tambah 6
    { "6", new List<string> { "C", "0", "8" } },  //  Tambah 8
    { "D", new List<string> { "0" } },
    { "0", new List<string> { "D" } },
    { "E", new List<string> { "F", "8" } },
    { "F", new List<string> { "E" } },
    { "C", new List<string> { "0", "6" } },
    { "5", new List<string> { "6" } },
    };

            if (similarChars.ContainsKey(templateName))
                return similarChars[templateName];

            return new List<string>(); // Return empty list jika tidak ada
        }

        private Scalar GetColorForTemplate(int templateId)
        {
            switch (templateId)
            {
                case 1: return Scalar.Red;      // Template 1
                case 2: return Scalar.Red;    // Template 2
                case 3: return Scalar.Red;     // Template 3
                default: return Scalar.Red;
            }
        }
        private void ShowDetectionSummary()
        {
            string summary = "=== DETECTION SUMMARY ===\n\n";

            foreach (var detection in allDetections)
            {
                summary += $"Template: {detection.TemplateName} (ID:{detection.TemplateId})\n";
                summary += $"  Position: ({detection.Location.X}, {detection.Location.Y})\n";
                summary += $"  Offset from calibration: ({detection.Offset.X}, {detection.Offset.Y})\n";
                summary += $"  Confidence: {detection.Confidence:F3}\n";
                summary += $"  Bounding Box: {detection.BoundingBox.Width}x{detection.BoundingBox.Height}\n\n";
            }

            Console.WriteLine(summary);
        }

        private void ClearAllDetections()
        {
            allDetections.Clear();
            foreach (var template in templates.Values)
            {
                template.IsCalibrated = false;
            }
        }

        private void ClearDetectionsForTemplate(int templateId)
        {
            if (templates.ContainsKey(templateId))
            {
                templates[templateId].IsCalibrated = false;
            }
            allDetections.RemoveAll(d => d.TemplateId == templateId);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            sourceImage?.Dispose();
            foreach (var template in templates.Values)
            {
                foreach (var img in template.Images)
                    img?.Dispose();
            }
            base.OnFormClosing(e);
        }

        private void Clear_Template_Click(object sender, EventArgs e)
        {
            // Hapus semua template
            foreach (var template in templates.Values)
            {
                foreach (var img in template.Images)
                    img?.Dispose();
            }
            templates.Clear();

            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;

            allDetections.Clear();

            if (sourceImage != null && !sourceImage.Empty())
            {
                pictureBox_Source.Image = BitmapConverter.ToBitmap(sourceImage);
            }

            Console.WriteLine("All templates have been cleared");
        }

        private void Load_Template_All_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Pilih folder yang berisi sub-folder template";
                fbd.ShowNewFolderButton = false;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string rootFolder = fbd.SelectedPath;

                    // Cari semua sub-folder
                    string[] subFolders = Directory.GetDirectories(rootFolder);

                    if (subFolders.Length == 0)
                    {
                        MessageBox.Show("Tidak ada sub-folder ditemukan! Buat folder untuk setiap karakter (A, B, C, dll.)",
                            "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Hapus semua template yang ada
                    foreach (var template in templates.Values)
                    {
                        foreach (var img in template.Images)
                            img?.Dispose();
                    }
                    templates.Clear();
                    allDetections.Clear();

                    // Clear pictureBox 1,2,3
                    pictureBox1.Image = null;
                    pictureBox2.Image = null;
                    pictureBox3.Image = null;

                    int templateId = 1;

                    foreach (string subFolder in subFolders)
                    {
                        string folderName = Path.GetFileName(subFolder);
                        string displayName = folderName.EndsWith("_")? folderName.TrimEnd('_') : folderName;

                        // Cari semua file gambar di sub-folder
                        string[] imageFiles = Directory.GetFiles(subFolder, "*.*", SearchOption.TopDirectoryOnly)
                            .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                           file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                           file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                           file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                            .ToArray();

                        if (imageFiles.Length == 0)
                        {
                            Console.WriteLine($"Sub-folder '{folderName}' kosong, dilewati.");
                            continue;
                        }

                        // Load semua varian gambar
                        List<Mat> variantImages = new List<Mat>();
                        foreach (string imageFile in imageFiles)
                        {
                            variantImages.Add(new Mat(imageFile, ImreadModes.Color));
                        }

                        // Varian terbaik = resolusi tertinggi
                        Mat bestImage = variantImages.OrderByDescending(m => m.Width * m.Height).First();

                        templates[templateId] = new TemplateData
                        {
                            Images = variantImages,
                            BestImage = bestImage, // TAMBAH INI
                            FilePath = subFolder,
                            TemplateName = displayName,
                            IsCalibrated = false,
                            TemplateId = templateId
                        };

                        Console.WriteLine($"Loaded template {templateId}: '{folderName}' → display as '{displayName}' dengan {variantImages.Count} varian");
                        templateId++;
                    }

                    // Tampilkan varian pertama di pictureBox jika <= 3 template
                    if (templates.ContainsKey(1) && templates[1].Images.Count > 0)
                        pictureBox1.Image = BitmapConverter.ToBitmap(templates[1].Images[0]);
                    if (templates.ContainsKey(2) && templates[2].Images.Count > 0)
                        pictureBox2.Image = BitmapConverter.ToBitmap(templates[2].Images[0]);
                    if (templates.ContainsKey(3) && templates[3].Images.Count > 0)
                        pictureBox3.Image = BitmapConverter.ToBitmap(templates[3].Images[0]);

                    // Refresh source image
                    if (sourceImage != null && !sourceImage.Empty())
                    {
                        pictureBox_Source.Image = BitmapConverter.ToBitmap(sourceImage);
                    }

                    MessageBox.Show($"Berhasil memuat {templates.Count} template dari {subFolders.Length} sub-folder.",
                        "Load Templates", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }


        private List<List<DetectionResult>> GroupRows(List<DetectionResult> detections, int tolerance = 20)
        {
            var sorted = detections
                .OrderBy(d => d.Location.Y + d.BoundingBox.Height / 2) // Pakai titik tengah Y
                .ToList();

            List<List<DetectionResult>> rows = new List<List<DetectionResult>>();

            foreach (var detection in sorted)
            {
                bool added = false;
                int detectMidY = detection.Location.Y + detection.BoundingBox.Height / 2;

                foreach (var row in rows)
                {
                    int rowMidY = row[0].Location.Y + row[0].BoundingBox.Height / 2;

                    if (Math.Abs(rowMidY - detectMidY) <= tolerance)
                    {
                        row.Add(detection);
                        added = true;
                        break;
                    }
                }

                if (!added)
                {
                    rows.Add(new List<DetectionResult> { detection });
                }
            }

            // urutkan kiri -> kanan
            foreach (var row in rows)
            {
                row.Sort((a, b) => a.Location.X.CompareTo(b.Location.X));
            }

            return rows;
        }

        private void BuildFinalTextResult()
        {
            var rows = GroupRows(allDetections);

            Console.WriteLine();
            Console.WriteLine("=========== FINAL SORT ORDER RESULT ===========");

            // Threshold jarak untuk dianggap SPACE (dalam pixel)
            int spaceThreshold = 15; // Sesuaikan dengan lebar karakter Anda

            foreach (var row in rows)
            {
                // ambil anchor text
                DetectionResult anchor = row.FirstOrDefault(r =>
                    r.TemplateName.ToLower() == "sn" ||
                    r.TemplateName.ToLower() == "cmmac" ||
                    r.TemplateName.ToLower() == "mtamac" ||
                    r.TemplateName.ToLower() == "wanmac" ||
                    r.TemplateName.ToLower() == "madein" ||
                    r.TemplateName.ToLower() == "indonesia"
                );

                // jika ada anchor
                if (anchor != null)
                {
                    // ambil semua char setelah anchor
                    var chars = row
                        .Where(r => r.Location.X > anchor.Location.X + anchor.BoundingBox.Width)
                        .OrderBy(r => r.Location.X)
                        .ToList();

                    string result = "";
                    for (int i = 0; i < chars.Count; i++)
                    {
                        result += chars[i].TemplateName;

                        // Cek gap dengan karakter berikutnya
                        if (i < chars.Count - 1)
                        {
                            int currentRight = chars[i].Location.X + chars[i].BoundingBox.Width;
                            int nextLeft = chars[i + 1].Location.X;
                            int gap = nextLeft - currentRight;

                            if (gap > spaceThreshold)
                            {
                                result += " "; // Tambah spasi
                            }
                        }
                    }

                    Console.WriteLine($"{anchor.TemplateName} {result}");
                }
                else
                {
                    // row biasa tanpa anchor
                    string rowText = "";
                    for (int i = 0; i < row.Count; i++)
                    {
                        rowText += row[i].TemplateName;

                        // Cek gap dengan karakter berikutnya
                        if (i < row.Count - 1)
                        {
                            int currentRight = row[i].Location.X + row[i].BoundingBox.Width;
                            int nextLeft = row[i + 1].Location.X;
                            int gap = nextLeft - currentRight;

                            if (gap > spaceThreshold)
                            {
                                rowText += " "; // Tambah spasi
                            }
                        }
                    }

                    Console.WriteLine(rowText);
                }
            }

            Console.WriteLine("========================================");
        }

        /// <summary>
        /// Deteksi QR Code di source image, return list area yang harus dikecualikan
        /// </summary>
        private List<Rect> DetectQRCodeAreas()
        {
            List<Rect> qrAreas = new List<Rect>();

            try
            {
                Mat gray = new Mat();
                Cv2.CvtColor(sourceImage, gray, ColorConversionCodes.BGR2GRAY);

                QRCodeDetector qrDetector = new QRCodeDetector();
                bool found = qrDetector.Detect(gray, out Point2f[] points);

                if (found && points != null && points.Length == 4)
                {
                    int minX = (int)points.Min(p => p.X);
                    int minY = (int)points.Min(p => p.Y);
                    int maxX = (int)points.Max(p => p.X);
                    int maxY = (int)points.Max(p => p.Y);

                    Rect qrRect = new Rect(minX, minY, maxX - minX, maxY - minY);

                    qrRect.X = Math.Max(0, qrRect.X - 10);
                    qrRect.Y = Math.Max(0, qrRect.Y - 10);
                    qrRect.Width = Math.Min(sourceImage.Width - qrRect.X, qrRect.Width + 20);
                    qrRect.Height = Math.Min(sourceImage.Height - qrRect.Y, qrRect.Height + 20);

                    qrAreas.Add(qrRect);
                    Console.WriteLine($"[QR DETECTED] Area: ({qrRect.X}, {qrRect.Y}) {qrRect.Width}x{qrRect.Height}");
                }

                gray.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QR DETECTION ERROR] {ex.Message}");
            }

            return qrAreas;
        }

        /// <summary>
        /// Cek apakah suatu rectangle berada di dalam area QR
        /// </summary>
        private bool IsInsideQRArea(Rect rect, List<Rect> qrAreas)
        {
            foreach (var qrArea in qrAreas)
            {
                Rect intersection = rect.Intersect(qrArea);
                if (intersection.Width > 0 && intersection.Height > 0)
                {
                    double overlapArea = intersection.Width * intersection.Height;
                    double rectArea = rect.Width * rect.Height;

                    if (overlapArea / rectArea > 0.5)
                        return true;
                }
            }
            return false;
        }
        private void ApplyDominanceFilter()
        {
            List<DetectionResult> filtered = new List<DetectionResult>();

            // sudah dipakai
            HashSet<int> removedIndexes = new HashSet<int>();

            // radius area dianggap object sama
            int distanceThreshold = 15;

            // selisih confidence minimum agar dianggap dominant
            double confidenceGap = 0.05;

            for (int i = 0; i < allDetections.Count; i++)
            {
                if (removedIndexes.Contains(i))
                    continue;

                var current = allDetections[i];

                for (int j = i + 1; j < allDetections.Count; j++)
                {
                    if (removedIndexes.Contains(j))
                        continue;

                    var other = allDetections[j];

                    // =========================
                    // CEK JARAK POSISI
                    // =========================
                    double dx = current.Location.X - other.Location.X;
                    double dy = current.Location.Y - other.Location.Y;

                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= distanceThreshold)
                    {
                        // =========================
                        // CEK CONFIDENCE DOMINANCE
                        // =========================
                        if (current.Confidence > other.Confidence + confidenceGap)
                        {
                            removedIndexes.Add(j);

                            Console.WriteLine(
                                $"[DOMINANCE FILTER] REMOVE {other.TemplateName} ({other.Confidence:F3}) " +
                                $"because weaker than {current.TemplateName} ({current.Confidence:F3})"
                            );
                        }
                        else if (other.Confidence > current.Confidence + confidenceGap)
                        {
                            removedIndexes.Add(i);

                            Console.WriteLine(
                                $"[DOMINANCE FILTER] REMOVE {current.TemplateName} ({current.Confidence:F3}) " +
                                $"because weaker than {other.TemplateName} ({other.Confidence:F3})"
                            );

                            break;
                        }
                    }
                }
            }

            // rebuild detection list
            for (int i = 0; i < allDetections.Count; i++)
            {
                if (!removedIndexes.Contains(i))
                {
                    filtered.Add(allDetections[i]);
                }
            }

            allDetections = filtered;
        }

        private void Save_Bbox_Click(object sender, EventArgs e)
        {
            try
            {
                if (sourceImage == null || sourceImage.Empty())
                {
                    MessageBox.Show("Source image belum ada!");
                    return;
                }

                if (allDetections.Count == 0)
                {
                    MessageBox.Show("Belum ada detection result!");
                    return;
                }

                string saveFolder = @"E:\TemplateMatching\CroppedBbox";

                // Buat folder jika belum ada
                if (!Directory.Exists(saveFolder))
                {
                    Directory.CreateDirectory(saveFolder);
                }

                // Counter untuk nama duplicate
                Dictionary<string, int> currentSessionCounters = new Dictionary<string, int>();

                foreach (var detection in allDetections)
                {
                    Rect rect = new Rect(
                        detection.BoundingBox.X,
                        detection.BoundingBox.Y,
                        detection.BoundingBox.Width,
                        detection.BoundingBox.Height
                    );

                    // Safety check
                    if (rect.X < 0 || rect.Y < 0 ||
                        rect.X + rect.Width > sourceImage.Width ||
                        rect.Y + rect.Height > sourceImage.Height)
                    {
                        continue;
                    }

                    // Crop ROI
                    using (Mat cropped = new Mat(sourceImage, rect))
                    {
                        string templateName = detection.TemplateName;

                        // ambil index awal dari existing file
                        if (!currentSessionCounters.ContainsKey(templateName))
                        {
                            currentSessionCounters[templateName] =
                                GetNextFileIndex(saveFolder, templateName);
                        }

                        int index = currentSessionCounters[templateName];

                        string fileName = $"{templateName}_{index}.jpg";

                        string fullPath = Path.Combine(saveFolder, fileName);

                        // save
                        Cv2.ImWrite(fullPath, cropped);

                        Console.WriteLine($"Saved: {fullPath}");

                        // increment untuk detection berikutnya
                        currentSessionCounters[templateName]++;
                    }
                }

                MessageBox.Show("Semua bounding box berhasil disimpan!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private int GetNextFileIndex(string folderPath, string templateName)
        {
            string[] existingFiles = Directory.GetFiles(
                folderPath,
                $"{templateName}_*.jpg"
            );

            int maxIndex = 0;

            foreach (string file in existingFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                // contoh:
                // PN_1
                // PN_25

                string[] parts = fileName.Split('_');

                if (parts.Length >= 2)
                {
                    if (int.TryParse(parts.Last(), out int index))
                    {
                        if (index > maxIndex)
                            maxIndex = index;
                    }
                }
            }

            return maxIndex + 1;
        }
    }
}
