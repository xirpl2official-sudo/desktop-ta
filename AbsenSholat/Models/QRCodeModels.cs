using System.Text.Json.Serialization;

namespace AbsenSholat.Models
{
    public class QRCodeResponse
    {
        [JsonPropertyName("data")]
        public QRCodeData Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class QRCodeData
    {
        [JsonPropertyName("expires_at")]
        public string ExpiresAt { get; set; }

        [JsonPropertyName("id_jadwal")]
        public int IdJadwal { get; set; }

        [JsonPropertyName("jenis_sholat")]
        public string JenisSholat { get; set; }

        [JsonPropertyName("qr_code")]
        public string QrCode { get; set; } // Base64 string

        [JsonPropertyName("token")]
        public string Token { get; set; }
    }

    public class VerifyQRRequest
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }

    public class VerifyQRResponse
    {
        [JsonPropertyName("data")]
        public VerifyQRData Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class VerifyQRData
    {
        [JsonPropertyName("jenis_sholat")]
        public string JenisSholat { get; set; }

        [JsonPropertyName("jurusan")]
        public string Jurusan { get; set; }

        [JsonPropertyName("kelas")]
        public string Kelas { get; set; }

        [JsonPropertyName("nama_siswa")]
        public string NamaSiswa { get; set; }

        [JsonPropertyName("nis")]
        public string Nis { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("tanggal")]
        public string Tanggal { get; set; }

        [JsonPropertyName("valid")]
        public bool Valid { get; set; }
    }
}
