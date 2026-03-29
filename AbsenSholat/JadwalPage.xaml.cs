using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AbsenSholat.Models;
using AbsenSholat.Services;

namespace AbsenSholat
{
    public partial class JadwalPage : UserControl
    {
        private readonly ApiClient _apiClient;
        private List<JadwalSholatData> _allJadwal;
        private List<DhuhaRowViewModel> _dhuhaRows;
        private bool _isEditMode = false;
        private string _userRole = "guest";

        // Click-to-Swap state
        private DhuhaRowViewModel _selectedRow;
        private int _selectedSlot = 0; // 1 or 2

        public JadwalPage()
        {
            InitializeComponent();
            _apiClient = new ApiClient();

            // Fetch role from global properties
            if (Application.Current.Properties.Contains("UserRole"))
            {
                _userRole = Application.Current.Properties["UserRole"] as string;
            }
            
            CheckRolePermissions();
            LoadDataFromApi();
        }

        private void CheckRolePermissions()
        {
            if (_userRole != "admin")
            {
                BtnEditDhuha.Visibility = Visibility.Collapsed;
                // Add more permission checks as needed
            }
        }

        private async void LoadDataFromApi()
        {
            try
            {
                _allJadwal = await _apiClient.GetJadwalSholatAsync();
                if (_allJadwal == null) return;

                UpdateSummaryCards();
                UpdateDhuhaTable();
            }
            catch (Exception ex)
            {
                Logger.Error("JadwalPage", "Failed to load schedules", ex);
            }
        }

        private void UpdateSummaryCards()
        {
            var dhuha = _allJadwal.FirstOrDefault(j => j.JenisSholat.Equals("DHUHA", StringComparison.OrdinalIgnoreCase));
            var zuhur = _allJadwal.FirstOrDefault(j => j.JenisSholat.Equals("DZUHUR", StringComparison.OrdinalIgnoreCase));
            var jumat = _allJadwal.FirstOrDefault(j => j.JenisSholat.Equals("JUMAT", StringComparison.OrdinalIgnoreCase));

            if (dhuha != null) TxtDhuhaTime.Text = $"{dhuha.JamMulai} - {dhuha.JamSelesai}";
            if (zuhur != null) TxtZuhurTime.Text = $"{zuhur.JamMulai} - {zuhur.JamSelesai}";
            if (jumat != null) TxtJumatTime.Text = $"{jumat.JamMulai} - {jumat.JamSelesai}";
        }

        private void UpdateDhuhaTable()
        {
            var dhuhaList = _allJadwal.Where(j => j.JenisSholat.Equals("DHUHA", StringComparison.OrdinalIgnoreCase)).ToList();
            var days = new[] { "Senin", "Selasa", "Rabu", "Kamis", "Jumat" };
            
            _dhuhaRows = new List<DhuhaRowViewModel>();
            int index = 0;
            foreach (var day in days)
            {
                var dayJadwal = dhuhaList.Where(j => j.Hari.Equals(day, StringComparison.OrdinalIgnoreCase)).OrderBy(j => j.Id).ToList();
                
                var row = new DhuhaRowViewModel
                {
                    Day = day,
                    RowBackground = index % 2 == 0 ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFBFC")),
                    Jurusan1 = dayJadwal.Count > 0 ? dayJadwal[0].Jurusan : "-",
                    Id1 = dayJadwal.Count > 0 ? dayJadwal[0].Id : -1,
                    Jurusan2 = dayJadwal.Count > 1 ? dayJadwal[1].Jurusan : "-",
                    Id2 = dayJadwal.Count > 1 ? dayJadwal[1].Id : -1
                };
                
                row.Slot1Tag = row;
                row.Slot2Tag = row;
                
                _dhuhaRows.Add(row);
                index++;
            }

            DhuhaTableItems.ItemsSource = _dhuhaRows;
        }

        private void OnEditDhuhaClick(object sender, RoutedEventArgs e)
        {
            _isEditMode = !_isEditMode;
            BtnSaveDhuha.Visibility = _isEditMode ? Visibility.Visible : Visibility.Collapsed;
            BtnEditDhuhaBorder.Background = _isEditMode ? new SolidColorBrush(Colors.LightSalmon) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));

            if (!_isEditMode)
            {
                // Reset selection if turned off
                ResetSelection();
            }
        }

        private void OnSlotClick(object sender, MouseButtonEventArgs e)
        {
            if (!_isEditMode || _userRole != "admin") return;

            var border = sender as Border;
            var row = border?.Tag as DhuhaRowViewModel;
            if (row == null) return;

            int slot = Grid.GetColumn(border) == 1 ? 1 : 2;

            if (_selectedRow == null)
            {
                // Select first slot
                _selectedRow = row;
                _selectedSlot = slot;
                if (slot == 1) _selectedRow.Color1 = Brushes.Blue;
                else _selectedRow.Color2 = Brushes.Blue;
            }
            else
            {
                // Swap with second slot
                SwapSlots(_selectedRow, _selectedSlot, row, slot);
                ResetSelection();
            }
        }

        private void SwapSlots(DhuhaRowViewModel row1, int slot1, DhuhaRowViewModel row2, int slot2)
        {
            string tempJurusan = slot1 == 1 ? row1.Jurusan1 : row1.Jurusan2;
            
            if (slot1 == 1)
            {
                row1.Jurusan1 = slot2 == 1 ? row2.Jurusan1 : row2.Jurusan2;
            }
            else
            {
                row1.Jurusan2 = slot2 == 1 ? row2.Jurusan1 : row2.Jurusan2;
            }

            if (slot2 == 1)
            {
                row2.Jurusan1 = tempJurusan;
            }
            else
            {
                row2.Jurusan2 = tempJurusan;
            }
        }

        private void ResetSelection()
        {
            foreach (var r in _dhuhaRows)
            {
                r.Color1 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
                r.Color2 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
            }
            _selectedRow = null;
            _selectedSlot = 0;
        }

        private async void OnSaveDhuhaClick(object sender, RoutedEventArgs e)
        {
            if (!_isEditMode) return;

            try
            {
                BtnSaveDhuha.IsEnabled = false;
                
                // Update all items in the Dhuha table
                foreach (var row in _dhuhaRows)
                {
                    if (row.Id1 != -1)
                    {
                        await _apiClient.UpdateJadwalSholatAsync(row.Id1, new JadwalSholatUpdateRequest { Jurusan = row.Jurusan1 });
                    }
                    if (row.Id2 != -1)
                    {
                        await _apiClient.UpdateJadwalSholatAsync(row.Id2, new JadwalSholatUpdateRequest { Jurusan = row.Jurusan2 });
                    }
                }

                MessageBox.Show("Jadwal Dhuha berhasil disimpan.", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Turn off edit mode
                _isEditMode = false;
                BtnSaveDhuha.Visibility = Visibility.Collapsed;
                BtnEditDhuhaBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));
                ResetSelection();
                LoadDataFromApi(); // Refresh strictly from server
            }
            catch (Exception ex)
            {
                Logger.Error("JadwalPage", "Failed to save Dhuha schedule", ex);
                MessageBox.Show($"Gagal menyimpan: {ex.Message}", "Error");
            }
            finally
            {
                BtnSaveDhuha.IsEnabled = true;
            }
        }

        private void OnPrayerCardClick(object sender, MouseButtonEventArgs e)
        {
            if (_userRole != "admin") return;
            // Feature: Edit time/day via dialog (similar to mobile)
            // Implementation delayed for now
        }
    }

    public class DhuhaRowViewModel : INotifyPropertyChanged
    {
        public string Day { get; set; }
        public Brush RowBackground { get; set; }
        
        private string _jurusan1;
        public string Jurusan1
        {
            get => _jurusan1;
            set { _jurusan1 = value; OnPropertyChanged(); }
        }

        private string _jurusan2;
        public string Jurusan2
        {
            get => _jurusan2;
            set { _jurusan2 = value; OnPropertyChanged(); }
        }

        public int Id1 { get; set; }
        public int Id2 { get; set; }

        private Brush _color1 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
        public Brush Color1
        {
            get => _color1;
            set { _color1 = value; OnPropertyChanged(); }
        }

        private Brush _color2 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
        public Brush Color2
        {
            get => _color2;
            set { _color2 = value; OnPropertyChanged(); }
        }

        public object Slot1Tag { get; set; }
        public object Slot2Tag { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
