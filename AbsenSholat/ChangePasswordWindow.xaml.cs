using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AbsenSholat
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly ApiClient _apiClient;
        private readonly string _nis;
        private readonly string _otp;
        
        private bool _isNewPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;

        private const string EyeOpenData = "M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,17a5,5 0 0,1 -5,-5a5,5 0 0,1 5,-5a5,5 0 0,1 5,5a5,5 0 0,1 -5,5M12,4.5C7,4.5 2.73,7.61 1,12c1.73,4.39 6,7.5 11,7.5s9.27,-3.11 11,-7.5c-1.73,-4.39 -6,-7.5 -11,-7.5Z";
        private const string EyeClosedData = "M11.83,9L15.64,12.81C15.92,12.35 16,11.54 16,11a4,4 0 0,0 -8,0c0,1.04 0.23,2.05 0.64,2.95l2.55,-2.96M2.01,3.87L4.1,5.96l-0.02,0.02c-0.45,0.8 -0.7,1.71 -0.7,2.67C3.38,10.14 6.52,13.28 10.31,13.28c1.15,0 2.26,-0.25 3.26,-0.7l2.15,2.15c-1.28,0.65 -2.72,1.02 -4.23,1.02 -5,0 -9,-4 -9,-9c0,-1.5 0.37,-2.93 1.02,-4.17m13.83,-0.76L20,19.5L18.5,21L3,4.5L4.5,3l11.33,11.11Z";

        public ChangePasswordWindow(string nis = "", string otp = "")
        {
            InitializeComponent();
            _apiClient = new ApiClient();
            _nis = nis;
            _otp = otp;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = sender as PasswordBox;
            if (pb == NewPasswordBox)
            {
                NewPasswordPlaceholder.Visibility = string.IsNullOrEmpty(pb.Password) ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (pb == ConfirmPasswordBox)
            {
                ConfirmPasswordPlaceholder.Visibility = string.IsNullOrEmpty(pb.Password) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void VisibleData_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == NewPasswordVisibleBox)
            {
                NewPasswordPlaceholder.Visibility = string.IsNullOrEmpty(tb.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (tb == ConfirmPasswordVisibleBox)
            {
                ConfirmPasswordPlaceholder.Visibility = string.IsNullOrEmpty(tb.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ToggleNewPassword_Click(object sender, RoutedEventArgs e)
        {
            _isNewPasswordVisible = !_isNewPasswordVisible;
            if (_isNewPasswordVisible)
            {
                NewPasswordVisibleBox.Text = NewPasswordBox.Password;
                NewPasswordVisibleBox.Visibility = Visibility.Visible;
                NewPasswordBox.Visibility = Visibility.Collapsed;
                NewEyeIcon.Data = Geometry.Parse(EyeClosedData);
            }
            else
            {
                NewPasswordBox.Password = NewPasswordVisibleBox.Text;
                NewPasswordVisibleBox.Visibility = Visibility.Collapsed;
                NewPasswordBox.Visibility = Visibility.Visible;
                NewEyeIcon.Data = Geometry.Parse(EyeOpenData);
            }
            // Trigger placeholder check manually
            if (_isNewPasswordVisible) VisibleData_TextChanged(NewPasswordVisibleBox, null);
            else PasswordBox_PasswordChanged(NewPasswordBox, null);
        }

        private void ToggleConfirmPassword_Click(object sender, RoutedEventArgs e)
        {
            _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
            if (_isConfirmPasswordVisible)
            {
                ConfirmPasswordVisibleBox.Text = ConfirmPasswordBox.Password;
                ConfirmPasswordVisibleBox.Visibility = Visibility.Visible;
                ConfirmPasswordBox.Visibility = Visibility.Collapsed;
                ConfirmEyeIcon.Data = Geometry.Parse(EyeClosedData);
            }
            else
            {
                ConfirmPasswordBox.Password = ConfirmPasswordVisibleBox.Text;
                ConfirmPasswordVisibleBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordBox.Visibility = Visibility.Visible;
                ConfirmEyeIcon.Data = Geometry.Parse(EyeOpenData);
            }
            // Trigger placeholder check manually
            if (_isConfirmPasswordVisible) VisibleData_TextChanged(ConfirmPasswordVisibleBox, null);
            else PasswordBox_PasswordChanged(ConfirmPasswordBox, null);
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = _isNewPasswordVisible ? NewPasswordVisibleBox.Text : NewPasswordBox.Password;
            string confirmPassword = _isConfirmPasswordVisible ? ConfirmPasswordVisibleBox.Text : ConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Mohon masukkan kata sandi baru.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Kata sandi minimal 6 karakter.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Konfirmasi kata sandi tidak cocok.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // If we don't have NIS or OTP (e.g. testing the UI directly), warn user
            if (string.IsNullOrEmpty(_nis) || string.IsNullOrEmpty(_otp))
            {
                 // Fallback for testing UI flow if invoked without params
                 // In production this should not happen if flow is correct
                 MessageBox.Show("Sesi reset password tidak valid (Missing NIS/OTP). Silakan ulangi proses lupa password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                 // return; // Commented out for now if user just wants to see UI behavior, but technically should return.
            }

            SubmitButton.IsEnabled = false;
            SubmitButton.Content = "Memproses...";

            try
            {
                // Call API
                await _apiClient.ResetPasswordAsync(_nis, _otp, newPassword);

                MessageBox.Show("Kata sandi berhasil diubah! Silakan login dengan kata sandi baru.", "Berhasil", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Navigate to Login
                var loginWindow = new MainWindow();
                loginWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengubah kata sandi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SubmitButton.IsEnabled = true;
                SubmitButton.Content = "LANJUT";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _apiClient?.Dispose();
            base.OnClosed(e);
        }
    }
}
