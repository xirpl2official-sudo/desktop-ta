// MainWindow.xaml.cs
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AbsenSholat.Models;
using AbsenSholat.Services;

namespace AbsenSholat
{
    public partial class MainWindow : Window
    {
        private readonly ApiClient _apiClient;
        private const string CredentialsFile = "user_credentials.json";
        private bool _isPasswordVisible = false;

        public MainWindow()
        {
            InitializeComponent();
            Logger.Info("MainWindow", "Login window initialized");
            _apiClient = new ApiClient();
            PasswordToggleButton.Click += PasswordToggleButton_Click;

            CheckAutoLogin();
        }

        private void PasswordToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;

                var eyePath = ((Canvas)((Viewbox)PasswordToggleButton.Content).Child).Children[0] as System.Windows.Shapes.Path;
                if (eyePath != null)
                    eyePath.Data = Geometry.Parse("M11.83,9L15.64,12.81C15.92,12.35 16,11.54 16,11a4,4 0 0,0 -8,0c0,1.04 0.23,2.05 0.64,2.95l2.55,-2.96M2.01,3.87L4.1,5.96l-0.02,0.02c-0.45,0.8 -0.7,1.71 -0.7,2.67C3.38,10.14 6.52,13.28 10.31,13.28c1.15,0 2.26,-0.25 3.26,-0.7l2.15,2.15c-1.28,0.65 -2.72,1.02 -4.23,1.02 -5,0 -9,-4 -9,-9c0,-1.5 0.37,-2.93 1.02,-4.17m13.83,-0.76L20,19.5L18.5,21L3,4.5L4.5,3l11.33,11.11Z");
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;

                var eyePath = ((Canvas)((Viewbox)PasswordToggleButton.Content).Child).Children[0] as System.Windows.Shapes.Path;
                if (eyePath != null)
                    eyePath.Data = Geometry.Parse("M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,17a5,5 0 0,1 -5,-5a5,5 0 0,1 5,-5a5,5 0 0,1 5,5a5,5 0 0,1 -5,5M12,4.5C7,4.5 2.73,7.61 1,12c1.73,4.39 6,7.5 11,7.5s9.27,-3.11 11,-7.5c-1.73,-4.39 -6,-7.5 -11,-7.5Z");
            }
        }

        private void CheckAutoLogin()
        {
            var saved = AuthService.LoadCredentials();
            if (saved != null && saved.RememberMe)
            {
                NisTextBox.Text = saved.Nis;
                PasswordBox.Password = saved.Password;
                RememberMeCheckBox.IsChecked = true;

                LoginAsync(saved.Nis, saved.Password, true);
            }
        }

        private async void LoginAsync(string nis, string password, bool rememberMe)
        {
            try
            {
                if (!await _apiClient.CheckApiStatusAsync())
                {
                    MessageBox.Show("Tidak dapat terhubung ke server API", "Kesalahan Koneksi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    AuthService.ClearCredentials();
                    return;
                }

                var loginResponse = await _apiClient.LoginAsync(nis, password);

                if (rememberMe)
                {
                    AuthService.SaveCredentials(nis, password, true);
                }

                var student = new Siswa
                {
                    Nis = loginResponse.Nis,
                    NamaSiswa = loginResponse.NamaSiswa,
                    jk = loginResponse.Jk,
                    Jurusan = loginResponse.Jurusan,
                    Kelas = loginResponse.Kelas
                };

                var dashboardWindow = new DashboardWindow(student, loginResponse.Email ?? "");
                dashboardWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                Logger.Error("MainWindow", "Auto login failed", ex);
                if (rememberMe)
                {
                     MessageBox.Show($"Auto login gagal: {ex.Message}", "Peringatan",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                AuthService.ClearCredentials();
                RememberMeCheckBox.IsChecked = false;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string nis = NisTextBox.Text.Trim();
            string password = _isPasswordVisible ? PasswordTextBox.Text : PasswordBox.Password;
            bool rememberMe = RememberMeCheckBox.IsChecked ?? false;

            Logger.Info("Auth", $"Login attempt for NIS: {nis}");

            if (string.IsNullOrEmpty(nis) || string.IsNullOrEmpty(password))
            {
                Logger.Warning("Auth", "Login failed: Empty NIS or password");
                MessageBox.Show("Mohon lengkapi NIS dan Kata Sandi", "Peringatan",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (nis == "admin_demo" && password == "Demo@12345")
                {
                    Logger.Success("Auth", "Admin login successful");
                    Application.Current.Properties["AuthToken"] = "admin_token";
                    var adminDashboard = new AdminDashboardWindow();
                    adminDashboard.Show();
                    Close();
                    return;
                }

                if (!await _apiClient.CheckApiStatusAsync())
                {
                    MessageBox.Show("Tidak dapat terhubung ke server API", "Kesalahan Koneksi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var loginResponse = await _apiClient.LoginAsync(nis, password);

                Logger.Success("Auth", $"Login successful for: {loginResponse.NamaSiswa}");
                
                if (rememberMe)
                {
                    AuthService.SaveCredentials(nis, password, true);
                }
                else
                {
                    AuthService.ClearCredentials();
                }

                var student = new Siswa
                {
                    Nis = loginResponse.Nis,
                    NamaSiswa = loginResponse.NamaSiswa,
                    jk = loginResponse.Jk,
                    Jurusan = loginResponse.Jurusan,
                    Kelas = loginResponse.Kelas
                };

                var dashboardWindow = new DashboardWindow(student, loginResponse.Email ?? "");
                dashboardWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Auth", "Login failed", ex);
                MessageBox.Show($"Login gagal: {ex.Message}", "Kesalahan",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            AuthService.ClearCredentials();

            var registerWindow = new RegisterWindow();
            registerWindow.Show();
            Close();
        }

        private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var forgotPasswordWindow = new ForgotPasswordWindow();
            forgotPasswordWindow.Show();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _apiClient?.Dispose();
            base.OnClosed(e);
        }
    }

}