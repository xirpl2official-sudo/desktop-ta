using System.Text.Json.Serialization;

namespace AbsenSholat.Models
{
    public class SesiSholat
    {
        [JsonPropertyName("id_sholat")]
        public int IdSholat { get; set; }

        [JsonPropertyName("nama_sholat")]
        public string NamaSholat { get; set; }

        [JsonPropertyName("waktu_mulai")]
        public string WaktuMulai { get; set; }

        [JsonPropertyName("waktu_selesai")]
        public string WaktuSelesai { get; set; }

        [JsonPropertyName("tanggal")]
        public string Tanggal { get; set; }
    }
}
