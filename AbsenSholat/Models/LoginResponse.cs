using System.Text.Json.Serialization;

namespace AbsenSholat.Models
{
    public class LoginResponse
    {
        [JsonPropertyName("nis")]
        public string Nis { get; set; }

        [JsonPropertyName("nama_siswa")]
        public string NamaSiswa { get; set; }

        [JsonPropertyName("jk")]
        public string Jk { get; set; }

        [JsonPropertyName("jurusan")]
        public string Jurusan { get; set; }

        [JsonPropertyName("kelas")]
        public string Kelas { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("is_google_acct")]
        public bool IsGoogleAcct { get; set; }

        // Staff fields
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("nip")]
        public string Nip { get; set; }

        [JsonPropertyName("id_staff")]
        public int? IdStaff { get; set; }
    }
}
