using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using BaknusITCare.DTOs;

namespace BaknusITCare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetaController : ControllerBase
    {
        private static readonly List<string> LocationsList = new()
        {
            "1. Ruang Workshop",
            "2. Ruang Kepsek",
            "3. Ruang Bahagian",
            "4. Wifi lantai 1",
            "5. Ruang BK, Ruang Guru",
            "6. Wifi lantai 2 Timur (deket lab AKT) . Wifi lantai 2 Barat",
            "7. Wifi lantai 2 Selatan",
            "8. Lab Akuntansi",
            "9. Ruang TU Keuangan",
            "10. Ruang Perpustakaan",
            "11. Lab Animasi",
            "12. Lab DKV",
            "13. Ruang TU",
            "14. Lab PK lantai 1",
            "15. Lab PK lantai 2",
            "16. Lab PK Lantai 3",
            "17. Ruang Photografi",
            "18. Lab Pemasaran",
            "19. Lainnya"
        };

        private static readonly List<string> ServicesList = new()
        {
            "Internet",
            "BaknusID"
        };

        private static readonly List<string> InternetSubIssues = new()
        {
            "Internet tidak terkoneksi",
            "Wi-Fi / LAN tidak nyala",
            "Sinyal Wi-Fi Lemah / RTO"
        };

        private static readonly List<string> BaknusIdApps = new()
        {
            "WebsiteBaknus",
            "Baknusmail",
            "BaknusAttend",
            "BaknusDrive",
            "BaknusClass"
        };

        private static readonly List<string> Priorities = new()
        {
            "Rendah",
            "Sedang",
            "Tinggi",
            "Darurat"
        };

        private static readonly List<string> Statuses = new()
        {
            "Baru",
            "Diproses",
            "MenungguSparepart",
            "Selesai",
            "Ditutup"
        };

        /// <summary>
        /// Mengambil seluruh referensi data formulir (19 Lokasi Ruangan, Kategori Layanan, Daftar Aplikasi BaknusID, Prioritas, dan Status)
        /// </summary>
        [HttpGet("options")]
        public ActionResult<ApiResponse<object>> GetFormOptions()
        {
            var data = new
            {
                locations = LocationsList,
                services = ServicesList,
                internetSubIssues = InternetSubIssues,
                baknusIdApps = BaknusIdApps,
                priorities = Priorities,
                statuses = Statuses
            };

            return Ok(ApiResponse<object>.Ok(data, "Daftar opsi formulir berhasil diambil."));
        }
    }
}
