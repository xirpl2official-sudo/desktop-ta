using System;
using System.IO;
using System.Text.Json;
using AbsenSholat.Models;

namespace AbsenSholat.Services
{
    public static class AuthService
    {
        private const string CredentialsFile = "user_credentials.json";

        public static void SaveCredentials(string nis, string password, bool rememberMe)
        {
            try
            {
                var credentials = new SavedCredentials
                {
                    Nis = nis,
                    Password = password,
                    RememberMe = rememberMe
                };

                var json = JsonSerializer.Serialize(credentials, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(CredentialsFile, json);
            }
            catch (Exception ex)
            {
                Logger.Error("AuthService", "Gagal menyimpan kredensial", ex);
            }
        }

        public static SavedCredentials? LoadCredentials()
        {
            if (File.Exists(CredentialsFile))
            {
                try
                {
                    var json = File.ReadAllText(CredentialsFile);
                    return JsonSerializer.Deserialize<SavedCredentials>(json);
                }
                catch (Exception ex)
                {
                    Logger.Error("AuthService", "Gagal memuat kredensial", ex);
                    ClearCredentials();
                }
            }
            return null;
        }

        public static void ClearCredentials()
        {
            try
            {
                if (File.Exists(CredentialsFile))
                {
                    File.Delete(CredentialsFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AuthService", "Gagal menghapus kredensial", ex);
            }
        }
    }
}
