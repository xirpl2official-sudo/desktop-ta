// ApiClient.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AbsenSholat.Models;

namespace AbsenSholat
{
    public class ApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        // Removed trailing slash as per previous fix
        private const string BaseUrl = "https://absensholat-api.vercel.app/api"; 
        private bool _disposed = false;
        
        // Shared token storage
        private static string _sharedToken;

        public ApiClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // Apply shared token if available
            if (!string.IsNullOrEmpty(_sharedToken))
            {
                ApplyTokenToHeader(_sharedToken);
            }
        }

        public void SetToken(string token)
        {
            _sharedToken = token;
            ApplyTokenToHeader(_sharedToken);
        }

        private void ApplyTokenToHeader(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                // Clean token from "Bearer " prefix if it already exists to avoid double prefixing
                string cleanedToken = token;
                if (cleanedToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    cleanedToken = cleanedToken.Substring(7).Trim();
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cleanedToken);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public string GetToken() => _sharedToken;
        public static string SharedToken => _sharedToken;

        private async Task<T> SendRequestAsync<T>(HttpMethod method, string endpoint, object payload = null)
        {
            var request = new HttpRequestMessage(method, $"{BaseUrl}{endpoint}");

            if (payload != null)
            {
                var json = JsonSerializer.Serialize(payload);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (!response.IsSuccessStatusCode)
            {
                // Try to parse error message
                string errorMessage = $"Request failed. Status: {response.StatusCode}.";
                try
                {
                    // Try to deserialize generic error response
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, options);
                    if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                    {
                        errorMessage += $" Message: {errorResponse.Message}";
                    }
                    else
                    {
                        errorMessage += $" Detail: {content}";
                    }
                }
                catch
                {
                    errorMessage += $" Detail: {content}";
                }
                
                throw new HttpRequestException(errorMessage);
            }

            // For void/empty responses (e.g. 200 OK with minimal JSON object)
            if (typeof(T) == typeof(bool)) 
            {
                return (T)(object)true;
            }

            // Generic deserialization
            var apiResponse = JsonSerializer.Deserialize<T>(content, options);
            return apiResponse;
        }

        // === AUTHENTICATION ===

        public async Task<LoginResponse> LoginAsync(string identifier, string password)
        {
            var payload = new { identifier, password };
            // Response wrapper is handlers.LoginResponseData which has data: LoginResponse
            var response = await SendRequestAsync<ApiResponse<LoginResponse>>(HttpMethod.Post, "/auth/login", payload);
            if (response?.Data != null)
            {
                SetToken(response.Data.Token);
                return response.Data;
            }
            return null;
        }

        public async Task<HistorySiswaResponse> GetHistorySiswaAsync(int week = 0)
        {
            return await SendRequestAsync<HistorySiswaResponse>(HttpMethod.Get, $"/history/siswa?week={week}");
        }
        
        public async Task<StatisticsResponse> GetStatisticsAsync()
        {
             return await SendRequestAsync<StatisticsResponse>(HttpMethod.Get, "/statistics");
        }

        public async Task<VerifyQRResponse> VerifyQrAsync(string token)
        {
            var payload = new { token };
            return await SendRequestAsync<VerifyQRResponse>(HttpMethod.Post, "/qrcode/verify", payload);
        }

        public async Task<RegisterResponse> RegisterAsync(string nis, string password, string email)
        {
            var payload = new { nis, password, email };
            return await SendRequestAsync<RegisterResponse>(HttpMethod.Post, "/auth/register", payload);
        }

        public async Task<LoginResponse> GetMeAsync()
        {
            var response = await SendRequestAsync<ApiResponse<LoginResponse>>(HttpMethod.Get, "/auth/me");
            return response?.Data;
        }

        public async Task<bool> ForgotPasswordAsync(string nis, string email)
        {
            var payload = new { nis, email };
            await SendRequestAsync<object>(HttpMethod.Post, "/auth/forgot-password", payload);
            return true;
        }

        public async Task<bool> VerifyOtpAsync(string nis, string otp)
        {
            var payload = new { nis, otp };
            await SendRequestAsync<object>(HttpMethod.Post, "/auth/verify-otp", payload);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string nis, string otp, string newPassword)
        {
            var payload = new { nis, otp, new_password = newPassword };
            await SendRequestAsync<object>(HttpMethod.Post, "/auth/reset-password", payload);
            return true;
        }
        
        // === CHANGE EMAIL ===

        public async Task<bool> ChangeEmailAsync(string newEmail)
        {
            var payload = new { new_email = newEmail };
            await SendRequestAsync<object>(HttpMethod.Post, "/auth/change-email", payload);
            return true;
        }

        public async Task<bool> VerifyChangeEmailAsync(string newEmail, string otp)
        {
            var payload = new { new_email = newEmail, otp };
            await SendRequestAsync<object>(HttpMethod.Post, "/auth/verify-change-email", payload);
            return true;
        }

        // === SISWA CRUD ===

        public async Task<SiswaListResponse> GetSiswaListAsync(string search = null, string kelas = null, string jurusan = null, int page = 1, int pageSize = 100)
        {
            var query = $"/siswa?page={page}&page_size={pageSize}";
            if (!string.IsNullOrEmpty(search)) query += $"&search={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrEmpty(kelas)) query += $"&kelas={Uri.EscapeDataString(kelas)}";
            if (!string.IsNullOrEmpty(jurusan)) query += $"&jurusan={Uri.EscapeDataString(jurusan)}";
            return await SendRequestAsync<SiswaListResponse>(HttpMethod.Get, query);
        }

        public async Task<ApiResponse<Siswa>> CreateSiswaAsync(Siswa siswa)
        {
            return await SendRequestAsync<ApiResponse<Siswa>>(HttpMethod.Post, "/siswa", siswa);
        }

        public async Task<ApiResponse<Siswa>> UpdateSiswaAsync(string nis, Siswa siswa)
        {
            return await SendRequestAsync<ApiResponse<Siswa>>(HttpMethod.Put, $"/siswa/{Uri.EscapeDataString(nis)}", siswa);
        }

        public async Task<bool> DeleteSiswaAsync(string nis)
        {
            await SendRequestAsync<object>(HttpMethod.Delete, $"/siswa/{Uri.EscapeDataString(nis)}");
            return true;
        }

        // === STAFF HISTORY (Presensi & Laporan) ===

        public async Task<HistoryStaffResponse> GetHistoryStaffAsync(
            string startDate = null, string endDate = null, 
            string kelas = null, string jurusan = null,
            string nis = null, string status = null,
            int page = 1, int limit = 100)
        {
            var query = $"/history/staff?page={page}&limit={limit}";
            if (!string.IsNullOrEmpty(startDate)) query += $"&start_date={startDate}";
            if (!string.IsNullOrEmpty(endDate)) query += $"&end_date={endDate}";
            if (!string.IsNullOrEmpty(kelas)) query += $"&kelas={Uri.EscapeDataString(kelas)}";
            if (!string.IsNullOrEmpty(jurusan)) query += $"&jurusan={Uri.EscapeDataString(jurusan)}";
            if (!string.IsNullOrEmpty(nis)) query += $"&nis={Uri.EscapeDataString(nis)}";
            if (!string.IsNullOrEmpty(status)) query += $"&status={Uri.EscapeDataString(status)}";
            return await SendRequestAsync<HistoryStaffResponse>(HttpMethod.Get, query);
        }

        // === QR CODE GENERATION ===

        public async Task<QRCodeResponse> GenerateQrCodeAsync(bool force = false, string jenisSholat = null, int? idJadwal = null)
        {
            var query = "/qrcode/generate";
            var queryParams = new List<string>();
            if (force) queryParams.Add("force=true");
            if (!string.IsNullOrEmpty(jenisSholat)) queryParams.Add($"jenis_sholat={Uri.EscapeDataString(jenisSholat)}");
            if (idJadwal.HasValue) queryParams.Add($"id_jadwal={idJadwal.Value}");
            if (queryParams.Count > 0) query += "?" + string.Join("&", queryParams);
            return await SendRequestAsync<QRCodeResponse>(HttpMethod.Get, query);
        }

        // === EXPORT (Excel / CSV) ===

        public async Task<byte[]> DownloadExportExcelAsync(string startDate = null, string endDate = null, string kelas = null, string jurusan = null)
        {
            var query = "/export/absensi/excel?";
            if (!string.IsNullOrEmpty(startDate)) query += $"start_date={startDate}&";
            if (!string.IsNullOrEmpty(endDate)) query += $"end_date={endDate}&";
            if (!string.IsNullOrEmpty(kelas)) query += $"kelas={Uri.EscapeDataString(kelas)}&";
            if (!string.IsNullOrEmpty(jurusan)) query += $"jurusan={Uri.EscapeDataString(jurusan)}&";
            query = query.TrimEnd('&', '?');

            var response = await _httpClient.GetAsync($"{BaseUrl}{query}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<byte[]> DownloadLaporanExcelAsync(string startDate = null, string endDate = null, string kelas = null, string jurusan = null)
        {
            var query = "/export/laporan/excel?";
            if (!string.IsNullOrEmpty(startDate)) query += $"start_date={startDate}&";
            if (!string.IsNullOrEmpty(endDate)) query += $"end_date={endDate}&";
            if (!string.IsNullOrEmpty(kelas)) query += $"kelas={Uri.EscapeDataString(kelas)}&";
            if (!string.IsNullOrEmpty(jurusan)) query += $"jurusan={Uri.EscapeDataString(jurusan)}&";
            query = query.TrimEnd('&', '?');

            var response = await _httpClient.GetAsync($"{BaseUrl}{query}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        // === JADWAL SHOLAT ===

        public async Task<List<JadwalSholatData>> GetJadwalSholatAsync()
        {
            var response = await SendRequestAsync<JadwalSholatListResponse>(HttpMethod.Get, "/jadwal-sholat");
            return response?.Data;
        }

        public async Task<JadwalSholatData> GetJadwalSholatByIdAsync(int id)
        {
            var response = await SendRequestAsync<ApiResponse<JadwalSholatData>>(HttpMethod.Get, $"/jadwal-sholat/{id}");
            return response?.Data;
        }

        public async Task<bool> UpdateJadwalSholatAsync(int id, JadwalSholatUpdateRequest request)
        {
            await SendRequestAsync<object>(HttpMethod.Put, $"/jadwal-sholat/{id}", request);
            return true;
        }

        // === UTILS ===

        public async Task<bool> CheckApiStatusAsync()
        {
            // We use /statistics as a ping since /auth/status is gone.
            // Does not require auth (based on swagger tags, but might fail if auth needed).
            // Or we can just assume true if no exception.
            try
            {
               // Using HttpClient directly to avoid throwing exception on 401
               // Just checking connectivity
               var response = await _httpClient.GetAsync($"{BaseUrl}/statistics");
               return true; // If we got a response (even 401 or 404), the server is reachable.
            }
            catch
            {
                return false;
            }
        }

        // === DISPOSAL ===

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}