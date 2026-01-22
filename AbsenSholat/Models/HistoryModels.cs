using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbsenSholat.Models
{
    public class HistorySiswaResponse
    {
        [JsonPropertyName("data")]
        public HistorySiswaData Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class HistorySiswaData
    {
        [JsonPropertyName("absensi")]
        public List<AbsensiHistoryItem> Absensi { get; set; }

        [JsonPropertyName("end_date")]
        public string EndDate { get; set; }

        [JsonPropertyName("periode")]
        public string Periode { get; set; }

        [JsonPropertyName("siswa")]
        public SiswaInfo Siswa { get; set; }

        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }

        [JsonPropertyName("statistik")]
        public HistoryStatistik Statistik { get; set; }
    }

    public class AbsensiHistoryItem
    {
        [JsonPropertyName("deskripsi")]
        public string Deskripsi { get; set; }

        [JsonPropertyName("hari")]
        public string Hari { get; set; }

        [JsonPropertyName("id_absen")]
        public int IdAbsen { get; set; }

        [JsonPropertyName("jenis_sholat")]
        public string JenisSholat { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("tanggal")]
        public string Tanggal { get; set; }
    }

    public class SiswaInfo
    {
        [JsonPropertyName("jurusan")]
        public string Jurusan { get; set; }

        [JsonPropertyName("kelas")]
        public string Kelas { get; set; }

        [JsonPropertyName("nama_siswa")]
        public string NamaSiswa { get; set; }

        [JsonPropertyName("nis")]
        public string Nis { get; set; }
    }

    public class HistoryStatistik
    {
        [JsonPropertyName("persentase_kehadiran")]
        public double PersentaseKehadiran { get; set; }

        [JsonPropertyName("total_absensi")]
        public int TotalAbsensi { get; set; }

        [JsonPropertyName("total_alpha")]
        public int TotalAlpha { get; set; }

        [JsonPropertyName("total_hadir")]
        public int TotalHadir { get; set; }

        [JsonPropertyName("total_izin")]
        public int TotalIzin { get; set; }

        [JsonPropertyName("total_sakit")]
        public int TotalSakit { get; set; }
    }
}
