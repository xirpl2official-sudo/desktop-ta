using System;
using System.Windows;
using System.Windows.Input;
using AbsenSholat.Models;

namespace AbsenSholat
{
    public partial class ProfileWindow : Window
    {
        private readonly ApiClient _apiClient;
        private Siswa _siswa;
        private string _email;
        private bool _isEditing = false;

        public ProfileWindow()
        {
            InitializeComponent();
        }

        public ProfileWindow(Siswa siswa, ApiClient apiClient, string email = "")
        {
            InitializeComponent();
            _siswa = siswa;
            _apiClient = apiClient;
            _email = email;
            
            LoadProfileData();
            UpdateEditState(false);
        }

        private void LoadProfileData()
        {
            if (_siswa != null)
            {
                // Header
                HeaderInitials.Text = !string.IsNullOrEmpty(_siswa.NamaSiswa) ? _siswa.NamaSiswa.Substring(0, 1).ToUpper() : "S";
                HeaderName.Text = _siswa.NamaSiswa?.ToUpper() ?? "SISWA";
                HeaderRole.Text = "Siswa";
                CurrentDateText.Text = DateTime.Now.ToString("dd MMMM yyyy").ToUpper();

                // Form
                TxtNis.Text = _siswa.Nis;
                TxtNama.Text = _siswa.NamaSiswa;
                
                // Set ComboBox based on JK
                if (_siswa.jk == "Laki-laki" || _siswa.jk == "L") CmbJk.SelectedIndex = 0;
                else CmbJk.SelectedIndex = 1;

                TxtKelas.Text = string.IsNullOrEmpty(_siswa.Jurusan) ? _siswa.Kelas : $"{_siswa.Kelas}/{_siswa.Jurusan}";
                
                // Email
                TxtEmail.Text = !string.IsNullOrEmpty(_email) ? _email : "email belum diatur";
            }
        }

        private void UpdateEditState(bool canEdit)
        {
            _isEditing = canEdit;
            
            // NIS always read-only
            TxtNis.IsReadOnly = true; 
            
            // Editable fields
            TxtNama.IsReadOnly = !canEdit;
            TxtKelas.IsReadOnly = !canEdit;
            TxtEmail.IsReadOnly = true; // Email always read-only here, changed via separate flow
            CmbJk.IsEnabled = canEdit;
            
            // Visual feedback
            TxtNama.Opacity = canEdit ? 1.0 : 0.7;
            TxtKelas.Opacity = canEdit ? 1.0 : 0.7;
            CmbJk.Opacity = canEdit ? 1.0 : 0.7;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            UpdateEditState(true);
            TxtNama.Focus();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Perubahan berhasil disimpan", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateEditState(false);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            // Revert changes
            LoadProfileData();
            UpdateEditState(false);
        }

        private void OnChangeEmailClick(object sender, MouseButtonEventArgs e)
        {
            if (_apiClient == null)
            {
                MessageBox.Show("Tidak dapat mengubah email. Silakan login ulang.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var changeEmailWindow = new ChangeEmailWindow(_apiClient, _email);
            changeEmailWindow.Owner = this;
            var result = changeEmailWindow.ShowDialog();

            if (result == true && changeEmailWindow.EmailChanged)
            {
                // Update the displayed email
                _email = changeEmailWindow.NewEmail;
                TxtEmail.Text = _email;
            }
        }
    }
}
