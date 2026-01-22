using System.Text.Json.Serialization;

namespace AbsenSholat.Models
{
    public class RegisterResponse
    {
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("is_google_acct")]
        public bool IsGoogleAcct { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("nis")]
        public string Nis { get; set; }
    }
}
