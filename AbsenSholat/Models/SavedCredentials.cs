namespace AbsenSholat.Models
{
    public class SavedCredentials
    {
        public string Nis { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}