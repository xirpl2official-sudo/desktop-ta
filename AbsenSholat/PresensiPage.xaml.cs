using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AbsenSholat.Models;
using AbsenSholat.Services;

namespace AbsenSholat
{
    public partial class PresensiPage : UserControl
    {
        private readonly ApiClient _apiClient;
        private List<PresensiViewModel> _allPresensi;

        public PresensiPage()
        {
            InitializeComponent();

            _apiClient = new ApiClient();
            
            LoadDataFromApi();
        }

        private async void LoadDataFromApi()
        {
            if (_apiClient == null) return;
            try
            {
                // Read filter values
                string jurusan = null;
                string kelas = null;

                if (FilterJurusan?.SelectedItem is ComboBoxItem jurusanItem)
                {
                    string val = jurusanItem.Content?.ToString() ?? "";
                    if (val != "Semua Jurusan") jurusan = val;
                }
                if (FilterKelas?.SelectedItem is ComboBoxItem kelasItem)
                {
                    string val = kelasItem.Content?.ToString() ?? "";
                    if (val != "Semua Kelas") kelas = val;
                }

                // Get today's date range
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                var response = await _apiClient.GetHistoryStaffAsync(
                    startDate: today, endDate: today,
                    kelas: kelas, jurusan: jurusan);

                if (response?.Data?.Absensi != null)
                {
                    _allPresensi = response.Data.Absensi.Select(a => new PresensiViewModel
                    {
                        Nis = a.Nis ?? "",
                        NamaSiswa = a.NamaSiswa ?? "",
                        Kelas = a.Kelas ?? "",
                        Jurusan = a.Jurusan ?? "",
                        Jam = a.Tanggal ?? "",
                        Status = a.Status ?? ""
                    }).ToList();
                }
                else
                {
                    _allPresensi = new List<PresensiViewModel>();
                }

                ApplyVisuals();
            }
            catch (Exception ex)
            {
                Logger.Error("PresensiPage", "Failed to load presensi data", ex);
                _allPresensi = new List<PresensiViewModel>();
                ApplyVisuals();
            }
        }

        private void ApplyVisuals()
        {
            if (_allPresensi == null) return;

            // Client-side search filter
            var filtered = _allPresensi.AsEnumerable();
            string searchText = SearchBox?.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(s =>
                    s.Nis.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    s.NamaSiswa.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            var result = filtered.ToList();

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
                        break;
                    case "izin":
                    case "sakit":
                        result[i].StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7"));
                        result[i].StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D97706"));
                        break;
                    case "alpha":
                        result[i].StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                        result[i].StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626"));
                        break;
                    default:
                        result[i].StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"));
                        result[i].StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
                        break;
                }
            }

            PresensiTable.ItemsSource = result;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyVisuals(); // Client-side search within loaded data
        }

        private void OnFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadDataFromApi(); // Re-fetch with new filters
        }

        private void OnTambahPresensiClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Fitur Tambah Presensi akan segera tersedia.", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnDetailClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string nis)
            {
                MessageBox.Show($"Detail presensi untuk NIS: {nis}", "Detail",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnEditPresensiClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string nis)
            {
                MessageBox.Show($"Fitur Edit Presensi (NIS: {nis}) akan segera tersedia.", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    /// <summary>
    /// ViewModel for displaying presensi data in the table
    /// </summary>
    public class PresensiViewModel
    {
        public string No { get; set; }
        public string Nis { get; set; }
        public string NamaSiswa { get; set; }
        public string Kelas { get; set; }
        public string Jurusan { get; set; }
        public string Jam { get; set; }
        public string Status { get; set; }
        public Brush RowBackground { get; set; } = Brushes.White;
        public Brush StatusBackground { get; set; }
        public Brush StatusForeground { get; set; }
    }
}
