using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
