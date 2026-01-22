using System.Text.Json.Serialization;

namespace AbsenSholat.Models
{
    public class Absen
    {
        [JsonPropertyName("id_absen")]
        public int IdAbsen { get; set; }

        [JsonPropertyName("id_siswa")]
        public int IdSiswa { get; set; }

        [JsonPropertyName("id_sholat")]
        public int IdSholat { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("deskripsi")]
        public string Deskripsi { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
    }
}
