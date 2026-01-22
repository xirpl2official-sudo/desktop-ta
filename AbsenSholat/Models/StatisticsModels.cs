using System.Text.Json.Serialization;

namespace AbsenSholat.Models
{
    public class StatisticsResponse
    {
        [JsonPropertyName("data")]
        public StatisticsData Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class StatisticsData
    {
        [JsonPropertyName("persentase_kehadiran")]
        public double PersentaseKehadiran { get; set; }

        [JsonPropertyName("rata_rata_kehadiran")]
        public double RataRataKehadiran { get; set; }

        [JsonPropertyName("tanggal")]
        public string Tanggal { get; set; } // YYYY-MM-DD

        [JsonPropertyName("total_absen_hari_ini")]
        public int TotalAbsenHariIni { get; set; }

        [JsonPropertyName("total_kehadiran_hari_ini")]
        public int TotalKehadiranHariIni { get; set; }

        [JsonPropertyName("total_siswa")]
        public int TotalSiswa { get; set; }

        [JsonPropertyName("total_tidak_hadir_hari_ini")]
        public int TotalTidakHadirHariIni { get; set; }
    }
}
