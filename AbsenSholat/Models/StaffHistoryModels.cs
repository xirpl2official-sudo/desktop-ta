using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbsenSholat.Models
{
    public class HistoryStaffResponse
    {
        [JsonPropertyName("data")]
        public HistoryStaffData Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class HistoryStaffData
    {
        [JsonPropertyName("absensi")]
        public List<AbsensiStaffItem> Absensi { get; set; }

        [JsonPropertyName("filters")]
        public HistoryFilters Filters { get; set; }

        [JsonPropertyName("pagination")]
        public PaginationInfo Pagination { get; set; }

        [JsonPropertyName("statistik")]
        public LaporanStatistik Statistik { get; set; }
    }

    public class AbsensiStaffItem
    {
        [JsonPropertyName("deskripsi")]
        public string Deskripsi { get; set; }

        [JsonPropertyName("hari")]
        public string Hari { get; set; }

        [JsonPropertyName("id_absen")]
        public int IdAbsen { get; set; }

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
    }

    public class HistoryFilters
    {
        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public string EndDate { get; set; }

        [JsonPropertyName("kelas")]
        public string Kelas { get; set; }

        [JsonPropertyName("jurusan")]
        public string Jurusan { get; set; }

        [JsonPropertyName("nis")]
        public string Nis { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class LaporanStatistik
    {
        [JsonPropertyName("total_absensi")]
        public int TotalAbsensi { get; set; }

        [JsonPropertyName("total_hadir")]
        public int TotalHadir { get; set; }

        [JsonPropertyName("total_sakit")]
        public int TotalSakit { get; set; }

        [JsonPropertyName("total_izin")]
        public int TotalIzin { get; set; }

        [JsonPropertyName("total_alpha")]
        public int TotalAlpha { get; set; }

        [JsonPropertyName("total_siswa")]
        public int TotalSiswa { get; set; }

        [JsonPropertyName("persentase_hadir")]
        public double PersentaseHadir { get; set; }

        [JsonPropertyName("persentase_sakit")]
        public double PersentaseSakit { get; set; }

        [JsonPropertyName("persentase_izin")]
        public double PersentaseIzin { get; set; }

        [JsonPropertyName("persentase_alpha")]
        public double PersentaseAlpha { get; set; }

        [JsonPropertyName("rata_rata_kehadiran")]
        public double RataRataKehadiran { get; set; }
    }

    public class PaginationInfo
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("total_items")]
        public int TotalItems { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }
    }
}
