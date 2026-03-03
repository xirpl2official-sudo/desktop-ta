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
    public partial class BerandaPage : UserControl
    {
        private readonly ApiClient _apiClient;
        private DispatcherTimer _clockTimer;
        private readonly CultureInfo _indonesiaCulture = new CultureInfo("id-ID");

        // Prayer time schedule
        private readonly TimeSpan DhuhaStart = new TimeSpan(7, 0, 0);
        private readonly TimeSpan DhuhaEnd = new TimeSpan(8, 0, 0);
        private readonly TimeSpan ZuhurStart = new TimeSpan(12, 0, 0);
        private readonly TimeSpan ZuhurEnd = new TimeSpan(12, 30, 0);
        private readonly TimeSpan JumatStart = new TimeSpan(11, 20, 0);
        private readonly TimeSpan JumatEnd = new TimeSpan(12, 30, 0);

        // Events to communicate with parent
        public event Action<string> RequestQrModal;

        public BerandaPage()
        {
            InitializeComponent();
            
            _apiClient = new ApiClient();
            if (Application.Current.Properties.Contains("AuthToken"))
            {
                _apiClient.SetToken((string)Application.Current.Properties["AuthToken"]);
            }

            LoadStatistics();
            InitializeClockTimer();
            UpdateDateDisplay();
            UpdateNextPrayerIndicator();
            UpdateQrIconsVisibility();
        }

        private void InitializeClockTimer()
        {
            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(30);
            _clockTimer.Tick += (s, e) =>
            {
                UpdateDateDisplay();
                UpdateNextPrayerIndicator();
                UpdateQrIconsVisibility();
            };
            _clockTimer.Start();
        }

        private void UpdateDateDisplay()
        {
            var now = DateTime.Now;
            string dayName = now.ToString("dddd", _indonesiaCulture);
            string dateFormatted = now.ToString("dd MMM yyyy", _indonesiaCulture);
            DateHeaderText.Text = $"📆 {dayName}, {dateFormatted}";
        }

        private void UpdateNextPrayerIndicator()
        {
            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;
            bool isFriday = now.DayOfWeek == DayOfWeek.Friday;
            
            string nextPrayer = "";
            string nextTime = "";
            
            if (currentTime < DhuhaStart)
            {
                nextPrayer = "Dhuha";
                nextTime = "07:00";
            }
            else if (currentTime < DhuhaEnd)
            {
                nextPrayer = "Dhuha (sedang berlangsung)";
                nextTime = "s/d 08:00";
            }
            else if (isFriday && currentTime < JumatStart)
            {
                nextPrayer = "Jum'at";
                nextTime = "11:20";
            }
            else if (isFriday && currentTime >= JumatStart && currentTime < JumatEnd)
            {
                nextPrayer = "Jum'at (sedang berlangsung)";
                nextTime = "s/d 12:30";
            }
            else if (!isFriday && currentTime < ZuhurStart)
            {
                nextPrayer = "Zuhur";
                nextTime = "12:00";
            }
            else if (!isFriday && currentTime >= ZuhurStart && currentTime < ZuhurEnd)
            {
                nextPrayer = "Zuhur (sedang berlangsung)";
                nextTime = "s/d 12:30";
            }
            else
            {
                nextPrayer = "Selesai";
                nextTime = "Tidak ada jadwal lagi hari ini";
            }
            
            if (nextPrayer == "Selesai")
            {
                NextPrayerIndicator.Text = $"✅ {nextTime}";
                NextPrayerIndicator.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
            }
            else
            {
                NextPrayerIndicator.Text = $"⏰ Sholat berikutnya: {nextPrayer} – {nextTime}";
                NextPrayerIndicator.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
            }
        }

        private void UpdateQrIconsVisibility()
        {
            DhuhaQrIcon.Visibility = Visibility.Visible;
            ZuhurQrIcon.Visibility = Visibility.Visible;
            JumatQrIcon.Visibility = Visibility.Visible;
            
            bool isFriday = DateTime.Now.DayOfWeek == DayOfWeek.Friday;
            JumatCard.Visibility = isFriday ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void LoadStatistics()
        {
            try
            {
                var statsResponse = await _apiClient.GetStatisticsAsync();
                if (statsResponse?.Data != null)
                {
                    TotalSiswaText.Text = statsResponse.Data.TotalSiswa.ToString();
                    TotalHadirText.Text = statsResponse.Data.TotalKehadiranHariIni.ToString();
                    PersentaseText.Text = $"{statsResponse.Data.PersentaseKehadiran:0.0}%";
                    IzinSakitText.Text = statsResponse.Data.TotalTidakHadirHariIni.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("BerandaPage", "Failed to load statistics", ex);
            }
        }

        private void OnDhuhaQrClick(object sender, MouseButtonEventArgs e)
        {
            RequestQrModal?.Invoke("Dhuha");
        }

        private void OnZuhurQrClick(object sender, MouseButtonEventArgs e)
        {
            RequestQrModal?.Invoke("Zuhur");
        }

        private void OnJumatQrClick(object sender, MouseButtonEventArgs e)
        {
            RequestQrModal?.Invoke("Jum'at");
        }

        public void StopTimers()
        {
            _clockTimer?.Stop();
        }
    }
}
