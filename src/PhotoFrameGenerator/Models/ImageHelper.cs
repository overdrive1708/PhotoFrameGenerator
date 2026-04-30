using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using SkiaSharp;
using System.Globalization;
using System.IO;

namespace PhotoFrameGenerator.Models
{
    /// <summary>
    /// 画像関連ヘルパークラス
    /// </summary>
    public class ImageHelper
    {
        /// <summary>
        /// フレームマージン(左)
        /// </summary>
        private const int FrameLeftMargin = 150;

        /// <summary>
        /// フレームマージン(右)
        /// </summary>
        private const int FrameRightMargin = 150;

        /// <summary>
        /// フレームマージン(上)
        /// </summary>
        private const int FrameTopMargin = 150;

        /// <summary>
        /// フレームマージン(下の画像に対する割合)
        /// </summary>
        private const float FrameBottomMarginRatio = 0.15f;

        /// <summary>
        /// 枠の線幅
        /// </summary>
        private const int BorderStrokeWidth = 1;

        /// <summary>
        /// 1行目のフォントサイズ(下のフレームに対する割合)
        /// </summary>
        private const float Line1FontSizeRatio = 0.18f;

        /// <summary>
        /// 2行目のフォントサイズ(下のフレームに対する割合)
        /// </summary>
        private const float Line2FontSizeRatio = 0.16f;

        /// <summary>
        /// 3行目のフォントサイズ(下のフレームに対する割合)
        /// </summary>
        private const float Line3FontSizeRatio = 0.14f;

        /// <summary>
        /// テキスト間隔
        /// </summary>
        private const int LineSpace = 30;

        /// <summary>
        /// エンコード品質
        /// </summary>
        private const int EncodeQuality = 95;

        /// <summary>
        /// フレーム表示設定クラス
        /// </summary>
        public class FrameDisplaySettings
        {
            /// <summary>
            /// 表示有無(カメラ名)
            /// </summary>
            public bool IsDisplayCameraModel { get; set; } = false;

            /// <summary>
            /// 表示有無(レンズ名)
            /// </summary>
            public bool IsDisplayLensModel { get; set; } = false;

            /// <summary>
            /// 表示有無(焦点距離)
            /// </summary>
            public bool IsDisplayFocalLength { get; set; } = false;

            /// <summary>
            /// 表示有無(露出モード)
            /// </summary>
            public bool IsDisplayExposureMode { get; set; } = false;

            /// <summary>
            /// 表示有無(絞り)
            /// </summary>
            public bool IsDisplayAperture { get; set; } = false;

            /// <summary>
            /// 表示有無(シャッタースピード)
            /// </summary>
            public bool IsDisplayShutterSpeed { get; set; } = false;

            /// <summary>
            /// 表示有無(露出補正)
            /// </summary>
            public bool IsDisplayExposureCompensation { get; set; } = false;

            /// <summary>
            /// 表示有無(ISO感度)
            /// </summary>
            public bool IsDisplayISOSensitivity { get; set; } = false;

            /// <summary>
            /// 表示有無(撮影日時)
            /// </summary>
            public bool IsDisplayShootingDateAndTime { get; set; } = false;
        }

        /// <summary>
        /// JPEG判定処理
        /// </summary>
        /// <param name="filePath">ファイルのフルパス</param>
        /// <returns>true：JPEG形式､false：その他の形式</returns>
        public static bool IsJpeg(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            try
            {
                using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (fs.Length < 2)
                {
                    return false;
                }

                // 先頭2バイトを読み込む
                byte[] buffer = new byte[2];
                fs.ReadExactly(buffer, 0, 2);

                // JPEGの開始マーカー (SOI): 0xFF, 0xD8
                return buffer[0] == 0xFF && buffer[1] == 0xD8;
            }
            catch (Exception)
            {
                // ファイルアクセス権限エラーなどの場合
                return false;
            }
        }

        /// <summary>
        /// フレーム付き画像生成処理
        /// </summary>
        /// <param name="filePath">ファイルのフルパス</param>
        /// <param name="settings">フレーム表示設定</param>
        public static void GenerateFramedImage(string filePath, FrameDisplaySettings settings)
        {
            // EXIF情報の取得
            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(filePath);
            ExifIfd0Directory? ifd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            ExifSubIfdDirectory? subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            string? model = ifd0Directory?.GetDescription(ExifDirectoryBase.TagModel);
            string? lensModel = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagLensModel);
            string? focalLength = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagFocalLength);
            string? fnumber = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagFNumber);
            string? exposureTime = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagExposureTime);
            string? isospeedRatings = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagIsoEquivalent);
            string? exposureProgram = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagExposureProgram);
            string? exposurebiasValue = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagExposureBias);
            string? dateTimeOriginal = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);

            // フレームに表示する文字列の生成
            string addLine1 = string.Empty;
            if(settings.IsDisplayCameraModel && !string.IsNullOrEmpty(model))
            {
                addLine1 += model;
            }
            string addLine2 = string.Empty;
            if (settings.IsDisplayLensModel && !string.IsNullOrEmpty(lensModel))
            {
                addLine2 += lensModel;
            }
            string addLine3 = string.Empty;
            if(settings.IsDisplayFocalLength && !string.IsNullOrEmpty(focalLength))
            {
                if (addLine3 != string.Empty)
                {
                    addLine3 += " | ";
                }
                addLine3 += focalLength;
            }
            if(settings.IsDisplayExposureMode && !string.IsNullOrEmpty(exposureProgram))
            {
                if (addLine3 != string.Empty)
                {
                    addLine3 += " | ";
                }
                addLine3 += exposureProgram;
            }
            if(settings.IsDisplayAperture && !string.IsNullOrEmpty(fnumber))
            {
                if (addLine3 != string.Empty)
                {
                    addLine3 += " | ";
                }
                addLine3 += fnumber;
            }
            if(settings.IsDisplayShutterSpeed && !string.IsNullOrEmpty(exposureTime))
            {
                if (addLine3 != string.Empty)
                {
                    addLine3 += " | ";
                }
                addLine3 += exposureTime;
            }
            if(settings.IsDisplayExposureCompensation && !string.IsNullOrEmpty(exposurebiasValue))
            {
                if (addLine3 != string.Empty)
                {
                    addLine3 += " | ";
                }
                addLine3 += $"Exposure Compensation {exposurebiasValue}";
            }
            if(settings.IsDisplayISOSensitivity && !string.IsNullOrEmpty(isospeedRatings))
            {
                if (addLine3 != string.Empty)
                {
                    addLine3 += " | ";
                }
                addLine3 += $"ISO {isospeedRatings}";
            }
            if (settings.IsDisplayShootingDateAndTime && !string.IsNullOrEmpty(dateTimeOriginal))
            {
                if (addLine3 != string.Empty)
                {
                    addLine3 += " | ";
                }
                addLine3 += DateTime.ParseExact(dateTimeOriginal, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy/MM/dd HH:mm:ss");
            }

            // 画像の読み込み
            using SKBitmap inputBitmap = SKBitmap.Decode(filePath);
            if (inputBitmap == null)
            {
                return;
            }

            // キャンバスのサイズ計算
            int frameBottomMargin = (int)(inputBitmap.Height * FrameBottomMarginRatio);
            int canvasWidth = inputBitmap.Width + FrameLeftMargin + FrameRightMargin;
            int canvasHeight = inputBitmap.Height + FrameTopMargin + frameBottomMargin;

            // 新しいキャンバスの作成
            SKImageInfo newInfo = new(canvasWidth, canvasHeight);
            using SKSurface surface = SKSurface.Create(newInfo);
            SKCanvas canvas = surface.Canvas;

            // キャンバスを白で塗りつぶす
            canvas.Clear(SKColors.White);

            // キャンバスの周りに枠を描画する
            using SKPaint paintBorder = new()
            {
                Color = SKColors.DimGray,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = BorderStrokeWidth
            };
            canvas.DrawRect(0, 0, newInfo.Width, newInfo.Height, paintBorder);

            // 元画像を描画する
            canvas.DrawBitmap(inputBitmap, FrameLeftMargin, FrameTopMargin);

            // テキストの描画設定(共通)
            float centerX = newInfo.Width / 2f;
            float marginCenterY = inputBitmap.Height + FrameTopMargin + (frameBottomMargin / 2f);

            // テキストの描画設定(1行目)
            using SKTypeface typefaceLine1 = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal);
            using SKFont fontLine1 = new(typefaceLine1, frameBottomMargin * Line1FontSizeRatio);
            using SKPaint paintLine1 = new()
            {
                IsAntialias = true,
                Color = SKColors.Black
            };
            float y1 = marginCenterY - fontLine1.Size - ((fontLine1.Metrics.Ascent + fontLine1.Metrics.Descent) / 2f);

            // テキストの描画(1行目)
            canvas.DrawText(addLine1, centerX, y1, SKTextAlign.Center, fontLine1, paintLine1);

            // テキストの描画設定(2行目)
            using SKTypeface typefaceLine2 = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal);
            using SKFont fontLine2 = new(typefaceLine2, frameBottomMargin * Line2FontSizeRatio);
            using SKPaint paintLine2 = new()
            {
                IsAntialias = true,
                Color = SKColors.Black
            };
            float y2 = y1 + fontLine2.Size + LineSpace;

            // テキストの描画(2行目)
            canvas.DrawText(addLine2, centerX, y2, SKTextAlign.Center, fontLine2, paintLine2);

            // テキストの描画設定(3行目)
            using SKTypeface typefaceLine3 = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal);
            using SKFont fontLine3 = new(typefaceLine3, frameBottomMargin * Line3FontSizeRatio);
            using SKPaint paintLine3 = new()
            {
                IsAntialias = true,
                Color = SKColors.DimGray
            };
            float y3 = y2 + fontLine3.Size + LineSpace;

            // テキストの描画(3行目)
            canvas.DrawText(addLine3, centerX, y3, SKTextAlign.Center, fontLine3, paintLine3);

            // 画像の保存
            string newFileName = $"framed-{Path.GetFileName(filePath)}";
            string newFilePath = Path.Combine(Path.GetDirectoryName(filePath) ?? System.IO.Directory.GetCurrentDirectory(), newFileName);
            using SKImage image = surface.Snapshot();
            using SKData data = image.Encode(SKEncodedImageFormat.Jpeg, EncodeQuality);
            using FileStream stream = File.OpenWrite(newFilePath);
            data.SaveTo(stream);
        }
    }
}
