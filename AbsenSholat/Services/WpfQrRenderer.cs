using System;
using System.Windows;
using System.Windows.Media;
using QRCoder;

namespace AbsenSholat.Services
{
    /// <summary>
    /// Vector-based QR Code renderer for WPF using QRCoder library.
    /// Creates lossless, scalable QR codes perfect for HiDPI displays.
    /// </summary>
    public static class WpfQrRenderer
    {
        private const string MASJID_DEFAULT = "Masjid Al-Hikmah SMKN2";

        /// <summary>
        /// Creates a vector-based DrawingImage of a QR code.
        /// </summary>
        public static DrawingImage CreateQrDrawing(
            string payload, 
            int pixelsPerModule = 10, 
            Brush? darkBrush = null, 
            Brush? lightBrush = null)
        {
            darkBrush ??= Brushes.Black;
            lightBrush ??= Brushes.White;

            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
            var moduleCount = qrCodeData.ModuleMatrix.Count;
            var size = moduleCount * pixelsPerModule;

            var drawingGroup = new DrawingGroup();

            // Background
            var backgroundRect = new RectangleGeometry(new Rect(0, 0, size, size));
            drawingGroup.Children.Add(new GeometryDrawing(lightBrush, null, backgroundRect));

            // Draw each module
            for (int row = 0; row < moduleCount; row++)
            {
                for (int col = 0; col < moduleCount; col++)
                {
                    if (qrCodeData.ModuleMatrix[row][col])
                    {
                        var rect = new RectangleGeometry(new Rect(
                            col * pixelsPerModule,
                            row * pixelsPerModule,
                            pixelsPerModule,
                            pixelsPerModule));
                        
                        drawingGroup.Children.Add(new GeometryDrawing(darkBrush, null, rect));
                    }
                }
            }

            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze(); // Optimize for performance
            
            return drawingImage;
        }

        /// <summary>
        /// Generates a unique 12-character token for QR code.
        /// </summary>
        public static string GenerateToken()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 12);
        }

        /// <summary>
        /// Creates a QR code with the standard attendance payload format.
        /// Format: SALAT:{jenis}|TANGGAL:{YYYY-MM-DD}|JAM:{HH:mm}|TOKEN:{12char}|MASJID:{lokasi}
        /// </summary>
        /// <param name="jenisSalat">Type of prayer (DHUHA, DZUHUR, JUMAT)</param>
        /// <param name="timestamp">Timestamp for the attendance</param>
        /// <param name="masjid">Mosque location name</param>
        /// <returns>Tuple of (DrawingImage, Token, Payload)</returns>
        public static (DrawingImage Image, string Token, string Payload) CreateAttendanceQr(
            string jenisSalat, 
            DateTime timestamp,
            string? masjid = null)
        {
            masjid ??= MASJID_DEFAULT;
            
            // Generate unique token
            string token = GenerateToken();
            
            // Build payload with specified format
            string payload = BuildPayload(jenisSalat, timestamp, token, masjid);
            
            // Generate QR image
            var image = CreateQrDrawing(payload, pixelsPerModule: 8);
            
            return (image, token, payload);
        }

        /// <summary>
        /// Builds the attendance payload string.
        /// </summary>
        public static string BuildPayload(string jenisSalat, DateTime timestamp, string token, string masjid)
        {
            return $"SALAT:{jenisSalat.ToUpper()}|TANGGAL:{timestamp:yyyy-MM-dd}|JAM:{timestamp:HH:mm}|TOKEN:{token}|MASJID:{masjid}";
        }

        /// <summary>
        /// Creates a QR code for legacy compatibility (simple format).
        /// </summary>
        public static DrawingImage CreatePrayerQr(
            string prayerType, 
            DateTime date, 
            string? classInfo = null,
            string? token = null)
        {
            var (image, _, _) = CreateAttendanceQr(prayerType, date);
            return image;
        }

        /// <summary>
        /// Creates a themed QR code matching the app's color scheme.
        /// </summary>
        public static DrawingImage CreateThemedQr(
            string payload,
            bool isDarkMode = false)
        {
            var darkBrush = isDarkMode 
                ? new SolidColorBrush(Color.FromRgb(248, 250, 252))
                : new SolidColorBrush(Color.FromRgb(30, 41, 59));
            
            var lightBrush = isDarkMode
                ? new SolidColorBrush(Color.FromRgb(30, 41, 59))
                : Brushes.White;

            return CreateQrDrawing(payload, 8, darkBrush, lightBrush);
        }
    }
}
