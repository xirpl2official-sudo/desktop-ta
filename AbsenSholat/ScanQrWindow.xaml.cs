using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using AbsenSholat.Models;

namespace AbsenSholat
{
    public partial class ScanQrWindow : Window
    {
        private readonly ApiClient _apiClient;
        private readonly Siswa _siswa;
        private bool _isScanning = false;
        private DispatcherTimer _scanAnimationTimer;

        public ScanQrWindow()
        {
            InitializeComponent();
        }

        public ScanQrWindow(ApiClient apiClient, Siswa siswa)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _siswa = siswa;
            
            DisplayStudentInfo();
        }

        private void DisplayStudentInfo()
        {
            if (_siswa != null)
            {
                StudentNameBlock.Text = _siswa.NamaSiswa?.ToUpper() ?? "SISWA";
                StudentRoleBlock.Text = "Siswa";
                ProfileInitialText.Text = !string.IsNullOrEmpty(_siswa.NamaSiswa) ? _siswa.NamaSiswa.Substring(0, 1).ToUpper() : "S";
            }
            CurrentDateText.Text = DateTime.Now.ToString("dd MMMM yyyy").ToUpper();
        }
        
        private void OnStartScanClick(object sender, RoutedEventArgs e)
        {
            if (_isScanning) return;

            // Simulate Camera Permission Request
            var result = MessageBox.Show("Izinkan aplikasi mengakses kamera?", 
                                         "Izin Kamera", 
                                         MessageBoxButton.YesNo, 
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                StartScanning();
            }
            else
            {
                MessageBox.Show("Izin kamera diperlukan untuk melakukan absensi.", "Akses Ditolak", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StartScanning()
        {
            _isScanning = true;
            ScanButton.Content = "Scanning...";
            ScanButton.IsEnabled = false;
            
            // Show "Camera" view (Simulated)
            CameraViewport.Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)); // Dark Gray
            CameraIcon.Visibility = Visibility.Collapsed;
            ScanLine.Visibility = Visibility.Visible;

            // Start animation
            _scanAnimationTimer = new DispatcherTimer();
            _scanAnimationTimer.Interval = TimeSpan.FromMilliseconds(20);
            double y = 0;
            bool goingDown = true;
            
            _scanAnimationTimer.Tick += (s, ev) =>
            {
                if (goingDown) y += 5; else y -= 5;
                if (y > 280) goingDown = false;
                if (y < 0) goingDown = true;
                
                ScanLine.Margin = new Thickness(0, y, 0, 0);
            };
            _scanAnimationTimer.Start();
        }

        private void StopScanning()
        {
            _isScanning = false;
            _scanAnimationTimer?.Stop();
            ScanButton.Content = "Mulai Scan";
            ScanButton.IsEnabled = true;
            ScanLine.Visibility = Visibility.Collapsed;
            CameraViewport.Background = Brushes.Black;
            CameraIcon.Visibility = Visibility.Visible;
        }

        private void OnCameraClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_isScanning) return;

            // Simulate finding a QR code - Open Input Dialog
            var inputDialog = new InputDialog("Simulasi Scan QR", "QR Code Terdeteksi! Masukkan Token:");
            if (inputDialog.ShowDialog() == true)
            {
                VerifyToken(inputDialog.Answer);
            }
        }

        private async void VerifyToken(string token)
        {
            try
            {
                ScanButton.Content = "Verifikasi...";
                
                var response = await _apiClient.VerifyQrAsync(token);
                if (response != null)
                {
                    MessageBox.Show($"Absensi Berhasil!\nStatus: {response.Data.Status}\nSiswa: {response.Data.NamaSiswa}", 
                                    "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Close this window and return to dashboard
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                 MessageBox.Show($"Gagal Validasi QR: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                 StopScanning();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _scanAnimationTimer?.Stop();
            base.OnClosed(e);
        }
    }
}
