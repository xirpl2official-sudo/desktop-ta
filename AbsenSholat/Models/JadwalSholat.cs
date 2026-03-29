using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbsenSholat.Models
{
    public class JadwalSholatData
    {
        [JsonPropertyName("id_jadwal")]
        public int Id { get; set; }

        [JsonPropertyName("hari")]
        public string Hari { get; set; }

        [JsonPropertyName("jenis_sholat")]
        public string JenisSholat { get; set; }

        [JsonPropertyName("waktu_mulai")]
        public string JamMulai { get; set; }

        [JsonPropertyName("waktu_selesai")]
        public string JamSelesai { get; set; }

        [JsonPropertyName("jurusan")]
        public string Jurusan { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
    }

    public class JadwalSholatListResponse
    {
        [JsonPropertyName("data")]
        public List<JadwalSholatData> Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class JadwalSholatUpdateRequest
    {
        [JsonPropertyName("jenis_sholat")]
        public string JenisSholat { get; set; }

        [JsonPropertyName("jam_mulai")]
        public string JamMulai { get; set; }

        [JsonPropertyName("jam_selesai")]
        public string JamSelesai { get; set; }

        [JsonPropertyName("hari")]
        public string Hari { get; set; }

        [JsonPropertyName("jurusan")]
        public string Jurusan { get; set; }
    }
}
