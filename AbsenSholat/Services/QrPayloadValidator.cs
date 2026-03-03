using System;
using System.Collections.Generic;

namespace AbsenSholat.Services
{
    /// <summary>
    /// Validates QR code payloads for attendance scanning.
    /// Format: SALAT:{jenis}|TANGGAL:{YYYY-MM-DD}|JAM:{HH:mm}|TOKEN:{12char}|MASJID:{lokasi}
    /// </summary>
    public static class QrPayloadValidator
    {
        // Maximum time difference allowed (in minutes)
        private const int MAX_TIME_DIFF_MINUTES = 5;

        /// <summary>
        /// Result of QR payload validation.
        /// </summary>
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public string? JenisSalat { get; set; }
            public string? Tanggal { get; set; }
            public string? Jam { get; set; }
            public string? Token { get; set; }
            public string? Masjid { get; set; }
            public string? RawPayload { get; set; }
        }

        /// <summary>
        /// Parses and validates a QR payload string.
        /// </summary>
        /// <param name="payload">The raw QR code content</param>
        /// <returns>Validation result with parsed fields</returns>
        public static ValidationResult Validate(string payload)
        {
            var result = new ValidationResult
            {
                RawPayload = payload,
                IsValid = false
            };

            if (string.IsNullOrWhiteSpace(payload))
            {
                result.ErrorMessage = "QR code kosong atau tidak terbaca.";
                return result;
            }

            // Parse payload into key-value pairs
            var fields = ParsePayload(payload);

            // Check required fields
            if (!fields.ContainsKey("SALAT"))
            {
                result.ErrorMessage = "Format QR tidak valid: field SALAT tidak ditemukan.";
                return result;
            }

            if (!fields.ContainsKey("TANGGAL"))
            {
                result.ErrorMessage = "Format QR tidak valid: field TANGGAL tidak ditemukan.";
                return result;
            }

            if (!fields.ContainsKey("JAM"))
            {
                result.ErrorMessage = "Format QR tidak valid: field JAM tidak ditemukan.";
                return result;
            }

            if (!fields.ContainsKey("TOKEN"))
            {
                result.ErrorMessage = "Format QR tidak valid: field TOKEN tidak ditemukan.";
                return result;
            }

            // Extract fields
            result.JenisSalat = fields["SALAT"];
            result.Tanggal = fields["TANGGAL"];
            result.Jam = fields["JAM"];
            result.Token = fields["TOKEN"];
            result.Masjid = fields.ContainsKey("MASJID") ? fields["MASJID"] : "Unknown";

            // Validate date format
            if (!DateTime.TryParse(result.Tanggal, out DateTime qrDate))
            {
                result.ErrorMessage = "Format tanggal QR tidak valid.";
                return result;
            }

            // Validate time format and check if within allowed window
            var timeValidation = ValidateTimeWindow(result.Tanggal, result.Jam);
            if (!timeValidation.isValid)
            {
                result.ErrorMessage = timeValidation.errorMessage;
                return result;
            }

            // Validate token format (should be 12 alphanumeric characters)
            if (result.Token.Length != 12)
            {
                result.ErrorMessage = "Token QR tidak valid.";
                return result;
            }

            // Validate salat type
            var validSalatTypes = new[] { "DHUHA", "DZUHUR", "ZUHUR", "JUMAT" };
            if (!Array.Exists(validSalatTypes, s => s.Equals(result.JenisSalat, StringComparison.OrdinalIgnoreCase)))
            {
                result.ErrorMessage = $"Jenis salat '{result.JenisSalat}' tidak dikenali.";
                return result;
            }

            // All validations passed
            result.IsValid = true;
            return result;
        }

        /// <summary>
        /// Parses the payload string into key-value pairs.
        /// </summary>
        private static Dictionary<string, string> ParsePayload(string payload)
        {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var parts = payload.Split('|');
            foreach (var part in parts)
            {
                var colonIndex = part.IndexOf(':');
                if (colonIndex > 0)
                {
                    var key = part.Substring(0, colonIndex).Trim();
                    var value = part.Substring(colonIndex + 1).Trim();
                    fields[key] = value;
                }
            }

            return fields;
        }

        /// <summary>
        /// Validates that the current time is within ±5 minutes of the QR time.
        /// </summary>
        private static (bool isValid, string? errorMessage) ValidateTimeWindow(string tanggal, string jam)
        {
            try
            {
                // Parse QR timestamp
                var qrDateTimeStr = $"{tanggal} {jam}";
                if (!DateTime.TryParse(qrDateTimeStr, out DateTime qrDateTime))
                {
                    return (false, "Format waktu QR tidak valid.");
                }

                // Get current time
                var now = DateTime.Now;

                // Check date first (must be today)
                if (qrDateTime.Date != now.Date)
                {
                    return (false, "QR code ini untuk tanggal berbeda.");
                }

                // Calculate time difference
                var timeDiff = Math.Abs((now - qrDateTime).TotalMinutes);

                if (timeDiff > MAX_TIME_DIFF_MINUTES)
                {
                    return (false, $"QR code kedaluwarsa. Waktu scan melebihi {MAX_TIME_DIFF_MINUTES} menit dari waktu generate.");
                }

                return (true, null);
            }
            catch (Exception)
            {
                return (false, "Gagal memvalidasi waktu QR.");
            }
        }

        /// <summary>
        /// Builds the API request payload for attendance submission.
        /// </summary>
        public static object BuildApiRequest(ValidationResult validation, string nis, double? latitude = null, double? longitude = null)
        {
            return new
            {
                nis = nis,
                payload = validation.RawPayload,
                jenis_salat = validation.JenisSalat,
                token = validation.Token,
                timestamp_scan = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                latitude = latitude,
                longitude = longitude
            };
        }
    }
}
