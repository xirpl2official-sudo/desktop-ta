// RegisterWindow.xaml.cs
using AbsenSholat.Models;
using System;
using System.Net.Http;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;

namespace AbsenSholat
{
    public partial class RegisterWindow : Window
    {
        private readonly ApiClient _apiClient;
        private bool _isKataKunciPasswordVisible = false;

        public RegisterWindow()
        {
            InitializeComponent();
            _apiClient = new ApiClient();
            LoginButton.Click += LoginButton_Click;
            RegisterButton.Click += RegisterButton_Click;
            KataKunciToggleButton.Click += KataKunciToggleButton_Click;
        }

        private void KataKunciToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isKataKunciPasswordVisible = !_isKataKunciPasswordVisible;

            if (_isKataKunciPasswordVisible)
            {
                KataKunciTextBox.Text = KataKunciPasswordBox.Password;
                KataKunciTextBox.Visibility = Visibility.Visible;
                KataKunciPasswordBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                KataKunciPasswordBox.Password = KataKunciTextBox.Text;
                KataKunciTextBox.Visibility = Visibility.Collapsed;
                KataKunciPasswordBox.Visibility = Visibility.Visible;
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string nis = NisTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string kataKunci = _isKataKunciPasswordVisible
                ? KataKunciTextBox.Text
                : KataKunciPasswordBox.Password;

            if (string.IsNullOrEmpty(nis))
            {
                MessageBox.Show("Mohon isi NIS", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
            {
                MessageBox.Show("Mohon isi email yang valid", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(kataKunci))
            {
                MessageBox.Show("Mohon isi Kata Kunci", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (kataKunci.Length < 6)
            {
                MessageBox.Show("Kata Kunci harus minimal 6 karakter", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(kataKunci, @"\d"))
            {
                MessageBox.Show("Kata Kunci harus mengandung setidaknya satu angka", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(kataKunci, @"[a-zA-Z]"))
            {
                MessageBox.Show("Kata Kunci harus mengandung setidaknya satu huruf", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RegisterButton.IsEnabled = false;
            var previousContent = RegisterButton.Content;
            RegisterButton.Content = "Mendaftar...";

            try
            {
                if (!await _apiClient.CheckApiStatusAsync())
                {
                    MessageBox.Show("Tidak dapat terhubung ke server API", "Kesalahan Koneksi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // ✅ INI YANG BENAR: RegisterAsync (bukan CreateSiswa!)
                var akun = await _apiClient.RegisterAsync(nis, kataKunci, email);

                MessageBox.Show("Pendaftaran berhasil! Silakan login.", "Berhasil",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                var loginWindow = new MainWindow();
                loginWindow.Show();
                Close();
            }
            catch (HttpRequestException httpEx)
            {
                string msg = httpEx.Message.Contains("Detail:")
                    ? httpEx.Message.Split(new[] { "Detail:" }, StringSplitOptions.None)[1].Trim()
                    : httpEx.Message;
                MessageBox.Show($"Gagal mendaftar: {msg}", "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi kesalahan: {ex.Message}", "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                RegisterButton.Content = previousContent;
                RegisterButton.IsEnabled = true;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _apiClient?.Dispose();
            base.OnClosed(e);
        }
    }
}