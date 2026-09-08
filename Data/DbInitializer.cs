using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BaknusITCare.Models;

namespace BaknusITCare.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            // Ensure Primary Categories (Layanan Internet & Layanan BaknusID)
            var internetCat = await context.TicketCategories.FirstOrDefaultAsync(c => c.Name == "Layanan Internet" || c.Name == "Jaringan & Wi-Fi");
            if (internetCat == null)
            {
                internetCat = new TicketCategory
                {
                    Name = "Layanan Internet",
                    Description = "Internet tidak terkoneksi, WiFi / LAN tidak nyala",
                    Icon = "Wifi",
                    DefaultSlaHours = 2,
                    IsActive = true
                };
                await context.TicketCategories.AddAsync(internetCat);
            }
            else
            {
                internetCat.Name = "Layanan Internet";
                internetCat.Description = "Internet tidak terkoneksi, WiFi / LAN tidak nyala";
                internetCat.Icon = "Wifi";
                internetCat.IsActive = true;
            }

            var baknusIdCat = await context.TicketCategories.FirstOrDefaultAsync(c => c.Name == "Layanan BaknusID" || c.Name == "SIAKAD & Software");
            if (baknusIdCat == null)
            {
                baknusIdCat = new TicketCategory
                {
                    Name = "Layanan BaknusID",
                    Description = "Website Baknus, Baknus Mail, Baknus Attend, Baknus Drive, Baknus Class",
                    Icon = "AppGeneric",
                    DefaultSlaHours = 4,
                    IsActive = true
                };
                await context.TicketCategories.AddAsync(baknusIdCat);
            }
            else
            {
                baknusIdCat.Name = "Layanan BaknusID";
                baknusIdCat.Description = "Website Baknus, Baknus Mail, Baknus Attend, Baknus Drive, Baknus Class";
                baknusIdCat.Icon = "AppGeneric";
                baknusIdCat.IsActive = true;
            }

            await context.SaveChangesAsync();

            // Deactivate other categories so complaints are strictly focused on Internet and BaknusID
            var otherCategories = await context.TicketCategories
                .Where(c => c.Id != internetCat.Id && c.Id != baknusIdCat.Id && c.IsActive)
                .ToListAsync();
            foreach (var other in otherCategories)
            {
                other.IsActive = false;
            }
            await context.SaveChangesAsync();

            // Seed Admin & Master Staff Users if empty
            if (!await context.Users.AnyAsync())
            {
                var adminUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "admin@smk.baktinusantara666.sch.id",
                    Email = "admin@smk.baktinusantara666.sch.id",
                    FullName = "Administrator IT School",
                    RoleName = "Admin",
                    DepartmentOrClass = "Admin, StaffIT",
                    PhoneNumber = "081234567890",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var technicianUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "teknisi.it@smk.baktinusantara666.sch.id",
                    Email = "teknisi.it@smk.baktinusantara666.sch.id",
                    FullName = "Budi Teknisi IT",
                    RoleName = "Teknisi",
                    DepartmentOrClass = "Teknisi, StaffIT",
                    PhoneNumber = "081987654321",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var sampleTeacher = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "guru.pplg@smk.baktinusantara666.sch.id",
                    Email = "guru.pplg@smk.baktinusantara666.sch.id",
                    FullName = "Ahmad S.Kom (Guru PPLG)",
                    RoleName = "Pelapor",
                    DepartmentOrClass = "Guru, Pengajar PPLG",
                    PhoneNumber = "085612344321",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var sampleTuStaff = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "tu.staff@smk.baktinusantara666.sch.id",
                    Email = "tu.staff@smk.baktinusantara666.sch.id",
                    FullName = "Siti Aminah (Staf TU)",
                    RoleName = "Pelapor",
                    DepartmentOrClass = "TU, TataUsaha",
                    PhoneNumber = "085711223344",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var sampleStudent = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "siswa.pplg@smk.baktinusantara666.sch.id",
                    Email = "siswa.pplg@smk.baktinusantara666.sch.id",
                    FullName = "Rizky Pratama (Siswa PPLG)",
                    RoleName = "Pelapor",
                    DepartmentOrClass = "Siswa",
                    PhoneNumber = "085811223344",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddRangeAsync(adminUser, technicianUser, sampleTeacher, sampleTuStaff, sampleStudent);
                await context.SaveChangesAsync();
            }

            // Seed Knowledge Base Articles if empty
            if (!await context.KnowledgeArticles.AnyAsync())
            {
                var articles = new List<KnowledgeArticle>
                {
                    new KnowledgeArticle
                    {
                        Title = "Cara Menghubungkan Laptop ke Wi-Fi Resmi SMK Bakti Nusantara 666",
                        Category = "Jaringan & Wi-Fi",
                        Summary = "Panduan langkah demi langkah memilih SSID resmi sekolah dan input kredensial Mailcow.",
                        Content = @"### Langkah Koneksi Wi-Fi Sekolah:
1. Aktifkan Wi-Fi di perangkat Laptop/HP Anda.
2. Pilih SSID: **Baknus-Staff** (untuk Guru/TU) atau **Baknus-Siswa** (untuk Siswa).
3. Masukkan Username email sekolah (contoh: `nama@smk.baktinusantara666.sch.id`) dan Password email Anda.
4. Jika muncul peringatan Certificate, tekan **Trust / Hubungkan**.
5. Jika masih mengalami kendala RTO, coba lakukan *Forget Network* kemudian hubungkan kembali.",
                        Icon = "Wifi",
                        Views = 142,
                        HelpfulVotes = 38,
                        IsPublished = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    },
                    new KnowledgeArticle
                    {
                        Title = "Solusi Printer Jam (Kertas Macet) dan Sharing Printer Lab/TU",
                        Category = "Printer & Scanner",
                        Summary = "Tindakan awal saat kertas tersangkut di printer dan cara menghubungkan komputer ke printer shared.",
                        Content = @"### Mengatasi Paper Jam:
1. Matikan tombol Power printer terlebih dahulu.
2. Buka penutup printer dengan hati-hati.
3. Tarik kertas yang tersangkut secara perlahan menggunakan kedua tangan (jangan ditarik paksa satu sisi agar kertas tidak robek).
4. Tutup kembali penutup printer dan nyalakan Power.

### Menghubungkan Sharing Printer:
1. Buka File Explorer, ketik `\\192.168.10.25` (IP Server Printer TU).
2. Klik ganda pada nama printer (misal `Epson L3210 TU`).
3. Tunggu hingga driver terpasang otomatis.",
                        Icon = "Print",
                        Views = 98,
                        HelpfulVotes = 24,
                        IsPublished = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-7)
                    },
                    new KnowledgeArticle
                    {
                        Title = "Panduan Reset Password Email Mailcow Sekolah",
                        Category = "SIAKAD & Software",
                        Summary = "Instruksi pengajuan reset kata sandi email sekolah jika lupa password.",
                        Content = @"### Reset Password Email:
1. Jika Anda lupa kata sandi akun Mailcow sekolah, silakan buat tiket di BaknusITCare pada kategori **SIAKAD & Software**.
2. Cantumkan NIS/NIP, Nama Lengkap, dan Nomor Whatsapp aktif Anda.
3. Staf IT akan memverifikasi identitas Anda dan mengirimkan Password Sementara via WhatsApp resmi IT Center.",
                        Icon = "LockClosed",
                        Views = 210,
                        HelpfulVotes = 65,
                        IsPublished = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-14)
                    }
                };

                await context.KnowledgeArticles.AddRangeAsync(articles);
                await context.SaveChangesAsync();
            }
        }
    }
}
