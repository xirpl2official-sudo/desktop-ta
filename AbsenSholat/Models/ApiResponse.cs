using System.Text.Json.Serialization;

namespace AbsenSholat.Models
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }
}
    