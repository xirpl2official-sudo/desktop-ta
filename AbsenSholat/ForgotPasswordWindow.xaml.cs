using System;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;

namespace AbsenSholat
{
    public partial class ForgotPasswordWindow : Window
    {
        private readonly ApiClient _apiClient;

        public ForgotPasswordWindow()
        {
            InitializeComponent();
            _apiClient = new ApiClient();
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == NisTextBox)
            {
                NisPlaceholder.Visibility = string.IsNullOrEmpty(tb.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (tb == EmailTextBox)
            {
                EmailPlaceholder.Visibility = string.IsNullOrEmpty(tb.Text) ? Visibility.Visible : Visibility.Collapsed;
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

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string nis = NisTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();

            if (string.IsNullOrEmpty(nis))
            {
                MessageBox.Show("Mohon masukkan NIS/NIP.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
            {
                MessageBox.Show("Mohon masukkan email yang valid.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SubmitButton.IsEnabled = false;
            SubmitButton.Content = "Mengirim...";

            try
            {
                // Call API to request OTP
                await _apiClient.ForgotPasswordAsync(nis, email);
                
                // Navigate to OTP Verification
                var otpWindow = new OtpVerificationWindow(nis, email);
                otpWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memproses permintaan: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SubmitButton.IsEnabled = true;
                SubmitButton.Content = "LANJUT";
            }
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
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
