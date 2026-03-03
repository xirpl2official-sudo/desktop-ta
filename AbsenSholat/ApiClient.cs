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