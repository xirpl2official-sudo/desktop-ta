using System;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;

namespace AbsenSholat
{
    public partial class ChangeEmailWindow : Window
    {
        private readonly ApiClient _apiClient;
        private readonly string _currentEmail;

        // Set to true + NewEmail when email change is fully confirmed
        public bool EmailChanged { get; private set; }
        public string NewEmail { get; private set; }

        public ChangeEmailWindow(ApiClient apiClient, string currentEmail)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _currentEmail = currentEmail;

            CurrentEmailTextBox.Text = !string.IsNullOrEmpty(currentEmail) ? currentEmail : "-";
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == NewEmailTextBox)
            {
                NewEmailPlaceholder.Visibility = string.IsNullOrEmpty(tb.Text) ? Visibility.Visible : Visibility.Collapsed;
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
            string newEmail = NewEmailTextBox.Text.Trim();

            if (string.IsNullOrEmpty(newEmail))
            {
                MessageBox.Show("Mohon masukkan email baru.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidEmail(newEmail))
            {
                MessageBox.Show("Format email tidak valid.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newEmail.Equals(_currentEmail, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Email baru tidak boleh sama dengan email saat ini.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SubmitButton.IsEnabled = false;
            SubmitButton.Content = "Mengirim...";

            try
            {
                // Request OTP to new email
                await _apiClient.ChangeEmailAsync(newEmail);

                // Open OTP verification window
                var otpWindow = new ChangeEmailOtpWindow(_apiClient, newEmail);
                otpWindow.Owner = this;
                var result = otpWindow.ShowDialog();

                if (result == true && otpWindow.IsEmailChanged)
                {
                    EmailChanged = true;
                    NewEmail = newEmail;
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengirim OTP: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SubmitButton.IsEnabled = true;
                SubmitButton.Content = "KIRIM OTP";
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
