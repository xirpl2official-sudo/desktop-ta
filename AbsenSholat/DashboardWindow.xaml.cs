using System;
using System.Windows;
using AbsenSholat.Models;

namespace AbsenSholat
{
    public partial class DashboardWindow : Window
    {
        private readonly Siswa _siswa;

        public DashboardWindow()
        {
            InitializeComponent();
            _siswa = null;
            LoadStats();
        }

        public DashboardWindow(Siswa siswa)
        {
            InitializeComponent();
            _siswa = siswa;
            LoadStudentInfo();
            LoadStats();
        }

        private void LoadStudentInfo()
        {
            if (_siswa != null)
            {
                StudentNameBlock.Text = _siswa.NamaSiswa ?? "Nama Siswa";
                StudentClassBlock.Text = _siswa.Kelas ?? "Kelas Tidak Diketahui";
            }
            else
            {
                StudentNameBlock.Text = "Nama Siswa";
                StudentClassBlock.Text = "Kelas Tidak Diketahui";
            }
        }

        private void LoadStats()
        {
            // Sample data - in a real app, this would come from the API
            TotalAbsensiText.Text = "45";
            KehadiranText.Text = "32";
            PersentaseText.Text = "71%";
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }
}
