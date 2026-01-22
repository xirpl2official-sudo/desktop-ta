using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbsenSholat.Models
{
    public class AkunLogin
    {
        public int Id { get; set; }
        public string Nis { get; set; }
        public string NamaSiswa { get; set; }
        public string JenisKelamin { get; set; }
        public string Jurusan { get; set; }
        public string Kelas { get; set; }
        public string Email { get; set; }
        public string CreatedAt { get; set; }
    }
}
