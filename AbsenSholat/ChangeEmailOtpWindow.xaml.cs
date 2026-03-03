using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AbsenSholat
{
    public partial class ChangeEmailOtpWindow : Window
    {
        private readonly ApiClient _apiClient;
        private readonly string _newEmail;
        private DispatcherTimer _timer;
        private int _timeLeft = 60;

        public bool IsEmailChanged { get; private set; }

        public ChangeEmailOtpWindow(ApiClient apiClient, string newEmail)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _newEmail = newEmail;

            EmailDisplayText.Text = !string.IsNullOrEmpty(newEmail) ? newEmail : "@user.com";

            StartTimer();
            OtpBox1.Focus();
        }

        private void StartTimer()
        {
            _timeLeft = 60;
            ResendButton.IsEnabled = false;
            ResendButton.Opacity = 0.5;
            TimerText.Text = $"{_timeLeft} Detik";

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timeLeft--;
            TimerText.Text = $"{_timeLeft} Detik";

            if (_timeLeft <= 0)
            {
                _timer.Stop();
                TimerText.Text = "";
                ResendButton.IsEnabled = true;
                ResendButton.Opacity = 1.0;
            }
        }

        // --- OTP Input Logic ---

        private void OtpBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = sender as TextBox;
            if (box == null) return;

            if (!string.IsNullOrEmpty(box.Text) && !box.Text.All(char.IsDigit))
            {
                box.Text = string.Empty;
                return;
            }

            if (box.Text.Length == 1)
            {
                MoveFocusNext(box);
            }
        }

        private void OtpBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var box = sender as TextBox;
            if (box == null) return;

            if (e.Key == Key.Back && string.IsNullOrEmpty(box.Text))
            {
                MoveFocusPrevious(box);
                e.Handled = true;
            }
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void MoveFocusNext(TextBox current)
        {
            if (current == OtpBox1) OtpBox2.Focus();
            else if (current == OtpBox2) OtpBox3.Focus();
            else if (current == OtpBox3) OtpBox4.Focus();
            else if (current == OtpBox4) OtpBox5.Focus();
            else if (current == OtpBox5) OtpBox6.Focus();
        }

        private void MoveFocusPrevious(TextBox current)
        {
            if (current == OtpBox6) OtpBox5.Focus();
            else if (current == OtpBox5) OtpBox4.Focus();
            else if (current == OtpBox4) OtpBox3.Focus();
            else if (current == OtpBox3) OtpBox2.Focus();
            else if (current == OtpBox2) OtpBox1.Focus();
        }

        private string GetOtp()
        {
            return $"{OtpBox1.Text}{OtpBox2.Text}{OtpBox3.Text}{OtpBox4.Text}{OtpBox5.Text}{OtpBox6.Text}";
        }

        // --- Buttons ---

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string otp = GetOtp();
            if (otp.Length < 6)
            {
                MessageBox.Show("Mohon masukkan 6 digit kode OTP.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ConfirmButton.IsEnabled = false;
            ConfirmButton.Content = "Memverifikasi...";

            try
            {
                await _apiClient.VerifyChangeEmailAsync(_newEmail, otp);

                IsEmailChanged = true;
                MessageBox.Show("Email berhasil diubah!", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Verifikasi gagal: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                OtpBox1.Text = ""; OtpBox2.Text = ""; OtpBox3.Text = "";
                OtpBox4.Text = ""; OtpBox5.Text = ""; OtpBox6.Text = "";
                OtpBox1.Focus();
            }
            finally
            {
                ConfirmButton.IsEnabled = true;
                ConfirmButton.Content = "KONFIRMASI";
            }
        }

        private async void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ResendButton.IsEnabled = false;

                // Resend OTP by calling change-email again
                await _apiClient.ChangeEmailAsync(_newEmail);

                MessageBox.Show("Kode OTP baru telah dikirim ke email baru anda.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                StartTimer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengirim ulang OTP: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ResendButton.IsEnabled = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            base.OnClosed(e);
        }
    }
}
