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
    public partial class DataSiswaPage : UserControl
    {
        private readonly ApiClient _apiClient;
        private List<SiswaViewModel> _allSiswa;

        public DataSiswaPage()
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
                // Read current filter values
                string search = SearchBox?.Text?.Trim();
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

                var response = await _apiClient.GetSiswaListAsync(search, kelas, jurusan);
                if (response?.Data != null)
                {
                    _allSiswa = response.Data.Select(s => new SiswaViewModel
                    {
                        Nis = s.Nis ?? "",
                        NamaSiswa = s.NamaSiswa ?? "",
                        Kelas = s.Kelas ?? "",
                        Jurusan = s.Jurusan ?? ""
                    }).ToList();
                }
                else
                {
                    _allSiswa = new List<SiswaViewModel>();
                }

                ApplyVisuals();
            }
            catch (Exception ex)
            {
                Logger.Error("DataSiswaPage", "Failed to load siswa data", ex);
                _allSiswa = new List<SiswaViewModel>();
                ApplyVisuals();
            }
        }

        private void ApplyVisuals()
        {
            if (_allSiswa == null) return;

            for (int i = 0; i < _allSiswa.Count; i++)
            {
                _allSiswa[i].No = $"{i + 1}.";
                _allSiswa[i].RowBackground = i % 2 == 0
                    ? new SolidColorBrush(Colors.White)
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFBFC"));
            }

            SiswaTable.ItemsSource = _allSiswa;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            LoadDataFromApi();
        }

        private void OnFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadDataFromApi();
        }

        private void OnTambahSiswaClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Fitur Tambah Siswa akan segera tersedia.", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void OnDeleteSiswaClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string nis)
            {
                var result = MessageBox.Show($"Apakah Anda yakin ingin menghapus siswa dengan NIS {nis}?",
                    "Konfirmasi Hapus", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _apiClient.DeleteSiswaAsync(nis);
                        LoadDataFromApi(); // Refresh
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("DataSiswaPage", $"Failed to delete siswa {nis}", ex);
                        MessageBox.Show($"Gagal menghapus siswa: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void OnEditSiswaClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string nis)
            {
                MessageBox.Show($"Fitur Edit Siswa (NIS: {nis}) akan segera tersedia.", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    /// <summary>
    /// ViewModel for displaying student data in the table
    /// </summary>
    public class SiswaViewModel
    {
        public string No { get; set; }
        public string Nis { get; set; }
        public string NamaSiswa { get; set; }
        public string Kelas { get; set; }
        public string Jurusan { get; set; }
        public Brush RowBackground { get; set; } = Brushes.White;
    }
}
