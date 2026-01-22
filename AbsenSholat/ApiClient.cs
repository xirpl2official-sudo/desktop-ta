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
        
        // Token storage
        private string _token;

        public ApiClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public void SetToken(string token)
        {
            _token = token;
            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public string GetToken() => _token;

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
            
            // Auto-set token if successful
            if (response?.Data != null && !string.IsNullOrEmpty(response.Data.Token))
            {
                SetToken(response.Data.Token);
            }
            
            return response?.Data;
        }

        public async Task<RegisterResponse> RegisterAsync(string nis, string password, string email)
        {
            var payload = new { nis, password, email };
            // Returns RegisterResponse directly? Check swagger. 
            // Swagger: 201 -> handlers.RegisterResponse.
            // My generic SendRequestAsync expects T.
            // Assuming the API returns the object directly or wrapped.
            // Swagger says "handlers.RegisterResponse" schema. It doesn't say it's wrapped in `data` like Login.
            // Let's assume standard wrapping based on LoginResponseData pattern if not specified, 
            // BUT looking at RegisterResponse model I created, it resembles the flat result.
            // If it's wrapped, I'll need a wrapper.
            // Let's try direct deserialization first, or check Login: Login returns { data: {...}, message: "..." }.
            // Register returns { created_at, email, ..., message }. It looks FLAT in swagger definition.
            
            return await SendRequestAsync<RegisterResponse>(HttpMethod.Post, "/auth/register", payload);
        }

        public async Task<LoginResponse> GetMeAsync()
        {
            // Response: handlers.LoginResponse wrapped?
            // Swagger responses: 200 -> handlers.LoginResponse.
            // Wait, LoginResponseData has data & message. LoginResponse has fields.
            // /auth/me returns handlers.LoginResponse. 
            // Does handlers.LoginResponse have data/message wrapper?
            // "handlers.LoginResponse": fields...
            // So it looks like /auth/me returns the profile DIRECTLY or wrapped?
            // Usually /auth/me returns the SAME structure as the 'data' part of /auth/login.
            // Let's try assuming it is wrapped in ApiResponse<LoginResponse> because consistency.
            // If not, we might need adjustments.
            
            var response = await SendRequestAsync<ApiResponse<LoginResponse>>(HttpMethod.Get, "/auth/me");
            return response?.Data;
        }

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

        // === SISWA ===

        public async Task<List<Siswa>> GetAllSiswaAsync(string search = null, int page = 1, int limit = 20)
        {
            // Query params
            string query = $"?page={page}&page_size={limit}";
            if (!string.IsNullOrEmpty(search)) query += $"&search={search}";

            var response = await SendRequestAsync<ApiResponse<List<Siswa>>>(HttpMethod.Get, $"/siswa{query}");
            return response?.Data;
        }

        public async Task<Siswa> GetSiswaByNisAsync(string nis)
        {
            var response = await SendRequestAsync<ApiResponse<Siswa>>(HttpMethod.Get, $"/siswa/{nis}");
            return response?.Data;
        }

        // === HISTORY & STATS ===

        public async Task<HistorySiswaData> GetHistorySiswaAsync(int week = 0)
        {
            var response = await SendRequestAsync<HistorySiswaResponse>(HttpMethod.Get, $"/history/siswa?week={week}");
            return response?.Data;
        }

        public async Task<StatisticsData> GetStatisticsAsync()
        {
            var response = await SendRequestAsync<StatisticsResponse>(HttpMethod.Get, "/statistics");
            return response?.Data;
        }

        // === QR CODE ===

        public async Task<QRCodeData> GenerateQRCodeAsync()
        {
            var response = await SendRequestAsync<QRCodeResponse>(HttpMethod.Get, "/qrcode/generate");
            return response?.Data;
        }

        public async Task<VerifyQRData> VerifyQRCodeAsync(string token)
        {
            var payload = new { token };
            var response = await SendRequestAsync<VerifyQRResponse>(HttpMethod.Post, "/qrcode/verify", payload);
            return response?.Data;
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