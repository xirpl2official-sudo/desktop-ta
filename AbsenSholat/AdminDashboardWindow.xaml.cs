using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AbsenSholat.Services;
using AbsenSholat.Models;

namespace AbsenSholat
{
    public partial class AdminDashboardWindow : Window
    {
        private BerandaPage _berandaPage;
        private JadwalPage _jadwalPage;
        
        private DispatcherTimer _qrCountdownTimer;
        private string _currentQrPrayer = "";
        private DateTime _qrExpireTime;
        private readonly CultureInfo _indonesiaCulture = new CultureInfo("id-ID");
        private readonly int QrValidMinutes = 5;

        public AdminDashboardWindow()
        {
            InitializeComponent();
            Logger.Info("AdminDashboard", "Admin dashboard shell initialized");
            
            InitializePages();
            UpdateNavigation("beranda");
        }

        private void InitializePages()
        {
            _berandaPage = new BerandaPage();
            _berandaPage.RequestQrModal += ShowQrModal;
            
            _jadwalPage = new JadwalPage();
            // _jadwalPage.RequestQrModal += ShowQrModal; // Add if JadwalPage gets QR buttons
        }

        private void ShowPage(string page)
        {
            if (page == "beranda")
            {
                MainContent.Content = _berandaPage;
            }
            else if (page == "jadwal")
            {
                MainContent.Content = _jadwalPage;
            }
        }

        private void ShowQrModal(string prayerName)
        {
            _currentQrPrayer = prayerName;
            var now = DateTime.Now;
            
            string dateStr = now.ToString("dd MMM yyyy", _indonesiaCulture);
            QrPrayerLabel.Text = $"Absensi: {prayerName} – {dateStr}";
            
            _qrExpireTime = now.AddMinutes(QrValidMinutes);
            GenerateQrCode(prayerName, now);
            StartQrCountdown();
            
            QrModalOverlay.Visibility = Visibility.Visible;
            Logger.Info("AdminDashboard", $"QR modal opened for {prayerName}");
        }

        private void GenerateQrCode(string prayerName, DateTime timestamp)
        {
            try
            {
                string jenisSalat = prayerName.ToUpper() switch
                {
                    "ZUHUR" => "DZUHUR",
                    "JUM'AT" => "JUMAT",
                    _ => prayerName.ToUpper()
                };

                var (qrImage, token, payload) = WpfQrRenderer.CreateAttendanceQr(
                    jenisSalat: jenisSalat,
                    timestamp: timestamp
                );
                
                QrCodeImage.Source = qrImage;
                QrCodeText.Visibility = Visibility.Collapsed;
                QrCodeImage.Visibility = Visibility.Visible;
                
                Logger.Info("AdminDashboard", $"QR generated - Salat: {jenisSalat}, Token: {token}");
            }
            catch (Exception ex)
            {
                Logger.Error("AdminDashboard", "Failed to generate QR code", ex);
                QrCodeText.Text = "📱";
                QrCodeText.Visibility = Visibility.Visible;
                QrCodeImage.Visibility = Visibility.Collapsed;
            }
        }

        private void StartQrCountdown()
        {
            _qrCountdownTimer?.Stop();
            _qrCountdownTimer = new DispatcherTimer();
            _qrCountdownTimer.Interval = TimeSpan.FromSeconds(1);
            _qrCountdownTimer.Tick += (s, e) =>
            {
                var remaining = _qrExpireTime - DateTime.Now;
                if (remaining.TotalSeconds <= 0)
                {
                    QrCountdownText.Text = "00:00 (Kadaluarsa)";
                    QrCountdownText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626"));
                    _qrCountdownTimer.Stop();
                }
                else
                {
                    QrCountdownText.Text = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
                    QrCountdownText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#92400E"));
                }
            };
            _qrCountdownTimer.Start();
        }

        private void OnCloseQrModal(object sender, MouseButtonEventArgs e)
        {
            QrModalOverlay.Visibility = Visibility.Collapsed;
            _qrCountdownTimer?.Stop();
        }

        private void OnRefreshQrClick(object sender, MouseButtonEventArgs e)
        {
            var now = DateTime.Now;
            _qrExpireTime = now.AddMinutes(QrValidMinutes);
            GenerateQrCode(_currentQrPrayer, now);
            
            string dateStr = now.ToString("dd MMM yyyy", _indonesiaCulture);
            QrPrayerLabel.Text = $"Absensi: {_currentQrPrayer} – {dateStr}";
            
            StartQrCountdown();
        }

        private void UpdateNavigation(string page)
        {
            NavBeranda.Tag = page == "beranda" ? "Active" : null;
            NavJadwal.Tag = page == "jadwal" ? "Active" : null;
            NavDataSiswa.Tag = page == "datasiswa" ? "Active" : null;
            NavPresensi.Tag = page == "presensi" ? "Active" : null;
            NavLaporan.Tag = page == "laporan" ? "Active" : null;

            ShowPage(page);
            Logger.Debug("AdminDashboard", $"Navigation updated to: {page}");
        }

        private void OnNavBerandaClick(object sender, RoutedEventArgs e) => UpdateNavigation("beranda");
        private void OnNavJadwalClick(object sender, RoutedEventArgs e) => UpdateNavigation("jadwal");

        private void OnNavDataSiswaClick(object sender, RoutedEventArgs e) 
        {
            UpdateNavigation("datasiswa");
            MessageBox.Show("Halaman Data Siswa akan segera tersedia.");
        }

        private void OnNavPresensiClick(object sender, RoutedEventArgs e)
        {
            UpdateNavigation("presensi");
            MessageBox.Show("Halaman Presensi akan segera tersedia.");
        }

        private void OnNavLaporanClick(object sender, RoutedEventArgs e)
        {
            UpdateNavigation("laporan");
            MessageBox.Show("Halaman Laporan akan segera tersedia.");
        }

        private void OnLogoutClick(object sender, MouseButtonEventArgs e)
        {
            var result = MessageBox.Show("Apakah Anda yakin ingin keluar?", "Konfirmasi", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Properties["AuthToken"] = null;
                new MainWindow().Show();
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _qrCountdownTimer?.Stop();
            _berandaPage?.StopTimers();
            base.OnClosed(e);
        }
    }
}
