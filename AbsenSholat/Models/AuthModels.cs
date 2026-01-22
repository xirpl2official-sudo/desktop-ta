using System.Text.Json.Serialization;

namespace AbsenSholat.Models
{
    public class ChangeEmailRequest
    {
        [JsonPropertyName("new_email")]
        public string NewEmail { get; set; }
    }

    public class ForgotPasswordRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("nis")]
        public string Nis { get; set; }
    }

    public class ResetPasswordRequest
    {
        [JsonPropertyName("new_password")]
        public string NewPassword { get; set; }

        [JsonPropertyName("nis")]
        public string Nis { get; set; }

        [JsonPropertyName("otp")]
        public string Otp { get; set; }
    }

    public class VerifyOTPRequest
    {
        [JsonPropertyName("nis")]
        public string Nis { get; set; }

        [JsonPropertyName("otp")]
        public string Otp { get; set; }
    }

    public class VerifyEmailOTPRequest
    {
        [JsonPropertyName("new_email")]
        public string NewEmail { get; set; }

        [JsonPropertyName("otp")]
        public string Otp { get; set; }
    }
}
