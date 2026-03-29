using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AbsenSholat.Models;
using AbsenSholat.Services;
using Microsoft.Win32;

namespace AbsenSholat
{
    public partial class LaporanPage : UserControl
    {
        private readonly ApiClient _apiClient;
        private List<LaporanViewModel> _allLaporan;

        public LaporanPage()
        {
            InitializeComponent();

            _apiClient = new ApiClient();
            
            InitializeDefaults();
            LoadDataFromApi();
        }

        private void InitializeDefaults()
        {
            DateFrom.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTo.SelectedDate = DateTime.Now;
        }

        private string GetFilterJurusan()
        {
            if (FilterJurusan?.SelectedItem is ComboBoxItem item)
            {
                string val = item.Content?.ToString() ?? "";
                if (val != "Semua Jurusan") return val;
            }
            return null;
        }

        private string GetFilterKelas()
        {
            if (FilterKelas?.SelectedItem is ComboBoxItem item)
            {
                string val = item.Content?.ToString() ?? "";
                if (val != "Semua Kelas") return val;
            }
            return null;
        }

        private async void LoadDataFromApi()
        {
            if (_apiClient == null) return;
            try
            {
                string startDate = DateFrom?.SelectedDate?.ToString("yyyy-MM-dd");
                string endDate = DateTo?.SelectedDate?.ToString("yyyy-MM-dd");
                string jurusan = GetFilterJurusan();
                string kelas = GetFilterKelas();

                var response = await _apiClient.GetHistoryStaffAsync(
                    startDate: startDate, endDate: endDate,
                    kelas: kelas, jurusan: jurusan);

                if (response?.Data?.Absensi != null)
                {
                    _allLaporan = response.Data.Absensi.Select(a => new LaporanViewModel
                    {
                        Tanggal = a.Tanggal ?? "",
                        Nis = a.Nis ?? "",
                        NamaSiswa = a.NamaSiswa ?? "",
                        Kelas = a.Kelas ?? "",
                        Jurusan = a.Jurusan ?? "",
                        JenisSalat = a.JenisSholat ?? "",
                        Status = a.Status ?? ""
                    }).ToList();
                }
                else
                {
                    _allLaporan = new List<LaporanViewModel>();
                }

                // Use API statistik if available
                if (response?.Data?.Statistik != null)
                {
                    var stats = response.Data.Statistik;
                    StatKehadiranText.Text = stats.TotalHadir.ToString();
                    StatSakitText.Text = stats.TotalSakit.ToString();
                    StatIzinText.Text = stats.TotalIzin.ToString();
                    StatAlphaText.Text = stats.TotalAlpha.ToString();
                }

                ApplyVisuals();
            }
            catch (Exception ex)
            {
                Logger.Error("LaporanPage", "Failed to load laporan data", ex);
                _allLaporan = new List<LaporanViewModel>();
                ApplyVisuals();
            }
        }

        private void ApplyVisuals()
        {
            if (_allLaporan == null) return;

            var result = _allLaporan;
            int hadirCount = 0, sakitCount = 0, izinCount = 0, alphaCount = 0;

            for (int i = 0; i < result.Count; i++)
            {
                result[i].No = $"{i + 1}.";
                result[i].RowBackground = i % 2 == 0
                    ? new SolidColorBrush(Colors.White)
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFBFC"));

                switch (result[i].Status?.ToLower())
                {
                    case "hadir":
                        result[i].StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCFCE7"));
                        result[i].StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A"));
                        hadirCount++;
                        break;
                    case "sakit":
                        result[i].StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));
                        result[i].StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB"));
                        sakitCount++;
                        break;
                    case "izin":
                        result[i].StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7"));
                        result[i].StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D97706"));
                        izinCount++;
                        break;
                    case "alpha":
                        result[i].StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                        result[i].StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626"));
                        alphaCount++;
                        break;
                    default:
                        result[i].StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"));
                        result[i].StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
                        break;
                }
            }

            LaporanTable.ItemsSource = result;

            // Fallback: only update from local count if API statistik wasn't available
            // (the stats are set from API in LoadDataFromApi)
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            LoadDataFromApi();
        }

        private async void OnUnduhLaporanClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string startDate = DateFrom?.SelectedDate?.ToString("yyyy-MM-dd");
                string endDate = DateTo?.SelectedDate?.ToString("yyyy-MM-dd");
                string jurusan = GetFilterJurusan();
                string kelas = GetFilterKelas();

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"Laporan_Presensi_{DateTime.Now:yyyyMMdd}.xlsx",
                    Title = "Simpan Laporan Excel"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var bytes = await _apiClient.DownloadLaporanExcelAsync(startDate, endDate, kelas, jurusan);
                    File.WriteAllBytes(saveDialog.FileName, bytes);
                    MessageBox.Show($"Laporan berhasil disimpan di:\n{saveDialog.FileName}", "Berhasil",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("LaporanPage", "Failed to download laporan", ex);
                MessageBox.Show($"Gagal mengunduh laporan: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnExportExcelClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string startDate = DateFrom?.SelectedDate?.ToString("yyyy-MM-dd");
                string endDate = DateTo?.SelectedDate?.ToString("yyyy-MM-dd");
                string jurusan = GetFilterJurusan();
                string kelas = GetFilterKelas();

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"Export_Absensi_{DateTime.Now:yyyyMMdd}.xlsx",
                    Title = "Export Data Absensi ke Excel"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var bytes = await _apiClient.DownloadExportExcelAsync(startDate, endDate, kelas, jurusan);
                    File.WriteAllBytes(saveDialog.FileName, bytes);
                    MessageBox.Show($"Data berhasil di-export ke:\n{saveDialog.FileName}", "Berhasil",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("LaporanPage", "Failed to export excel", ex);
                MessageBox.Show($"Gagal export excel: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// ViewModel for displaying laporan data in the table
    /// </summary>
    public class LaporanViewModel
    {
        public string No { get; set; }
        public string Tanggal { get; set; }
        public string Nis { get; set; }
        public string NamaSiswa { get; set; }
        public string Kelas { get; set; }
        public string Jurusan { get; set; }
        public string JenisSalat { get; set; }
        public string Status { get; set; }
        public Brush RowBackground { get; set; } = Brushes.White;
        public Brush StatusBackground { get; set; }
        public Brush StatusForeground { get; set; }
    }
}
