using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbsenSholat.Models
{
    public class Siswa
    {
        [JsonPropertyName("nis")]
        public string Nis { get; set; }

        [JsonPropertyName("nama_siswa")]
        public string NamaSiswa { get; set; }

        [JsonPropertyName("jk")]
        public string jk { get; set; }

        [JsonPropertyName("jurusan")]
        public string Jurusan { get; set; }

        [JsonPropertyName("kelas")]
        public string Kelas { get; set; }
    }

    public class SiswaListResponse
    {
        [JsonPropertyName("data")]
        public List<Siswa> Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("pagination")]
        public SiswaPaginationMeta Pagination { get; set; }
    }

    public class SiswaPaginationMeta
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        [JsonPropertyName("total_items")]
        public int TotalItems { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }
    }
}
