using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BaknusITCare.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace BaknusITCare.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendTicketCreatedConfirmationAsync(Ticket ticket)
        {
            string subject = $"[BaknusITCare] Konfirmasi Penerimaan Tiket #{ticket.TicketCode} - {ticket.Title}";
            
            string bodyHtml = $@"
<div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; padding: 30px 15px;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.08); border-top: 5px solid #0078d4;"">
        <div style=""padding: 24px; background-color: #0078d4; color: #ffffff;"">
            <h2 style=""margin: 0; font-size: 22px; font-weight: 600;"">BaknusITCare - IT Support Center</h2>
            <p style=""margin: 5px 0 0 0; font-size: 14px; opacity: 0.9;"">Layanan Terpadu IT SMK Bakti Nusantara 666</p>
        </div>
        <div style=""padding: 24px; color: #333333;"">
            <p style=""font-size: 16px;"">Halo <strong>{ticket.RequesterName}</strong>,</p>
            <p>Laporan masalah IT Anda telah kami terima dan terdaftar secara otomatis di sistem BaknusITCare.</p>
            
            <div style=""background-color: #f0f4f8; border-left: 4px solid #0078d4; padding: 15px; margin: 20px 0; border-radius: 4px;"">
                <table style=""width: 100%; border-collapse: collapse;"">
                    <tr><td style=""padding: 4px 0; font-weight: 600; width: 130px;"">Nomor Tiket:</td><td><span style=""background: #0078d4; color: #fff; padding: 2px 8px; border-radius: 4px; font-weight: bold;"">#{ticket.TicketCode}</span></td></tr>
                    <tr><td style=""padding: 4px 0; font-weight: 600;"">Judul Masalah:</td><td>{ticket.Title}</td></tr>
                    <tr><td style=""padding: 4px 0; font-weight: 600;"">Kategori:</td><td>{ticket.Category?.Name ?? "Umum"}</td></tr>
                    <tr><td style=""padding: 4px 0; font-weight: 600;"">Lokasi Ruangan:</td><td>{ticket.Location}</td></tr>
                    <tr><td style=""padding: 4px 0; font-weight: 600;"">Tingkat Urgensi:</td><td><strong style=""color: #d13438;"">{ticket.Priority}</strong></td></tr>
                    <tr><td style=""padding: 4px 0; font-weight: 600;"">Estimasi SLA:</td><td>Selesai dalam {ticket.Category?.DefaultSlaHours ?? 4} Jam ({ticket.DueDate.AddHours(7):HH:mm, dd MMM yyyy})</td></tr>
                </table>
            </div>

            <p><strong>Deskripsi Kendala:</strong></p>
            <p style=""background: #fafafa; border: 1px solid #e1e1e1; padding: 12px; border-radius: 4px; font-style: italic;"">{ticket.Description}</p>

            <div style=""text-align: center; margin: 25px 0;"">
                <a href=""https://baknusitcare.smkbn666.sch.id/track/{ticket.TicketCode}"" style=""background-color: #0078d4; color: #ffffff; padding: 12px 28px; text-decoration: none; border-radius: 25px; font-weight: bold; display: inline-block; box-shadow: 0 3px 8px rgba(0,0,0,0.15);"">
                    🔍 Lacak Status Tiket #{ticket.TicketCode} &rarr;
                </a>
            </div>

            <p style=""margin-top: 15px;"">Tim Teknisi IT akan segera memproses laporan Anda. Anda dapat melacak perkembangan penanganan kapan saja menggunakan nomor tiket di atas.</p>
        </div>
        <div style=""padding: 16px 24px; background-color: #f8f9fa; border-top: 1px solid #eeeeee; font-size: 12px; color: #777777; text-align: center;"">
            &copy; 2026 Tim IT Infrastructure - SMK Bakti Nusantara 666.<br/>
            Email ini dikirimkan secara otomatis dari sistem auto-responder BaknusITCare.
        </div>
    </div>
</div>";

            return await SendEmailAsync(ticket.RequesterEmail, subject, bodyHtml);
        }

        public async Task<bool> SendNewTicketAlertToTimITAsync(Ticket ticket, System.Collections.Generic.List<string> timItEmails)
        {
            if (timItEmails == null || !timItEmails.Any()) return false;

            string subject = $"[NOTIFIKASI TIM IT] Tiket Kendala Baru Masuk #{ticket.TicketCode} - {ticket.Title}";

            string bodyHtml = $@"
<div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; padding: 30px 15px;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.08); border-top: 5px solid #d83b01;"">
        <div style=""padding: 24px; background-color: #d83b01; color: #ffffff;"">
            <h2 style=""margin: 0; font-size: 22px; font-weight: 600;"">🚨 BaknusITCare - Laporan Kendala Baru Masuk</h2>
            <p style=""margin: 5px 0 0 0; font-size: 14px; opacity: 0.9;"">Pemberitahuan Khusus Anggota Tim IT & Administrator</p>
        </div>
        <div style=""padding: 24px; color: #333333;"">
            <p style=""font-size: 16px;"">Halo <strong>Rekan Tim IT SMK Bakti Nusantara 666</strong>,</p>
            <p>Terdapat laporan kendala IT baru yang diajukan oleh pengguna dan memerlukan tindak lanjut penanganan:</p>
            
            <div style=""background-color: #fff4ce; border-left: 4px solid #d83b01; padding: 15px; margin: 20px 0; border-radius: 4px;"">
                <table style=""width: 100%; border-collapse: collapse; font-size: 14px;"">
                    <tr><td style=""padding: 5px 0; font-weight: 600; width: 140px;"">Nomor Tiket:</td><td><span style=""background: #d83b01; color: #fff; padding: 3px 8px; border-radius: 4px; font-weight: bold;"">#{ticket.TicketCode}</span></td></tr>
                    <tr><td style=""padding: 5px 0; font-weight: 600;"">Pelapor:</td><td><strong>{ticket.RequesterName}</strong> ({ticket.RequesterEmail})</td></tr>
                    <tr><td style=""padding: 5px 0; font-weight: 600;"">Judul Masalah:</td><td><strong>{ticket.Title}</strong></td></tr>
                    <tr><td style=""padding: 5px 0; font-weight: 600;"">Kategori:</td><td><span style=""background: #e1dfdd; padding: 2px 8px; border-radius: 4px;"">{ticket.Category?.Name ?? "Umum"}</span></td></tr>
                    <tr><td style=""padding: 5px 0; font-weight: 600;"">Lokasi / Ruangan:</td><td><strong style=""color: #0078d4;"">{ticket.Location}</strong></td></tr>
                    <tr><td style=""padding: 5px 0; font-weight: 600;"">Tag / Aset:</td><td>{ticket.AssetTag ?? "-"}</td></tr>
                    <tr><td style=""padding: 5px 0; font-weight: 600;"">Tingkat Urgensi:</td><td><strong style=""color: #d13438;"">{ticket.Priority}</strong></td></tr>
                    <tr><td style=""padding: 5px 0; font-weight: 600;"">Batas Waktu SLA:</td><td>Selesai dalam {ticket.Category?.DefaultSlaHours ?? 4} Jam ({ticket.DueDate.AddHours(7):HH:mm, dd MMM yyyy})</td></tr>
                </table>
            </div>

            <p><strong>Rincian Kendala dari Pelapor:</strong></p>
            <p style=""background: #fafafa; border: 1px solid #e1e1e1; padding: 12px; border-radius: 4px; font-style: italic; white-space: pre-line;"">{ticket.Description}</p>

            <div style=""text-align: center; margin: 30px 0;"">
                <a href=""https://baknusitcare.smkbn666.sch.id/tickets/detail/{ticket.Id}"" style=""background-color: #0078d4; color: #ffffff; padding: 12px 28px; text-decoration: none; border-radius: 25px; font-weight: bold; display: inline-block; box-shadow: 0 3px 8px rgba(0,0,0,0.15);"">
                    Buka & Tindak Lanjuti Tiket Ini &rarr;
                </a>
            </div>

            <p style=""font-size: 13px; color: #666666;"">Silakan segera lakukan penanganan di lokasi dan perbarui status tiket menjadi <em>'Diproses'</em> di aplikasi BaknusITCare.</p>
        </div>
        <div style=""padding: 16px 24px; background-color: #f8f9fa; border-top: 1px solid #eeeeee; font-size: 12px; color: #777777; text-align: center;"">
            &copy; 2026 Tim IT Infrastructure - SMK Bakti Nusantara 666.<br/>
            Email notifikasi ini dikirimkan otomatis kepada seluruh Anggota Tim IT & Administrator.
        </div>
    </div>
</div>";

            bool anySuccess = false;
            foreach (var recipient in timItEmails)
            {
                if (string.IsNullOrWhiteSpace(recipient)) continue;
                var res = await SendEmailAsync(recipient.Trim(), subject, bodyHtml);
                if (res) anySuccess = true;
            }

            return anySuccess;
        }

        public async Task<bool> SendTicketStatusUpdatedAsync(Ticket ticket, string oldStatus, string newStatus, string? commentMessage = null, System.Collections.Generic.List<string>? additionalRecipients = null)
        {
            string subject = newStatus switch
            {
                "Diproses" => $"[BaknusITCare - SEDANG DITANGANI] Laporan Kendala #{ticket.TicketCode}: {ticket.Title}",
                "Selesai" => $"[BaknusITCare - SELESAI] Laporan Kendala #{ticket.TicketCode} Telah Berhasil Diperbaiki",
                "MenungguSparepart" => $"[BaknusITCare - MENUNGGU SPAREPART] Tiket #{ticket.TicketCode}: Menunggu Pengadaan Onderdil",
                "Ditutup" => $"[BaknusITCare - DITUTUP] Tiket #{ticket.TicketCode} Telah Ditutup",
                _ => $"[BaknusITCare] Pembaharuan Status Tiket #{ticket.TicketCode}: {newStatus}"
            };

            string statusBadgeColor = newStatus switch
            {
                "Diproses" => "#ffaa00",
                "MenungguSparepart" => "#8e44ad",
                "Selesai" => "#107c41",
                "Ditutup" => "#666666",
                _ => "#0078d4"
            };

            string statusHeadline = newStatus switch
            {
                "Diproses" => "Laporan Kendala Anda SEDANG DITANGANI oleh Tim IT",
                "Selesai" => "Laporan Kendala Anda TELAH SELESAI DIPERBAIKI! 🎉",
                "MenungguSparepart" => "Laporan Sedang Menunggu Pengadaan Sparepart / Vendor",
                "Ditutup" => "Tiket Kendala IT Telah Resmi Ditutup",
                _ => $"Status Tiket Diperbarui: {newStatus}"
            };

            string statusDescription = newStatus switch
            {
                "Diproses" => $"Laporan kendala IT saat ini <strong>SEDANG DITANGANI</strong> oleh petugas Tim IT (<strong>{ticket.AssignedTechnicianName ?? "Petugas Tim IT"}</strong>). Petugas sedang memeriksa sistem atau menuju lokasi.",
                "Selesai" => $"Kabar baik! Laporan kendala IT telah <strong>BERHASIL DIPERBAIKI / SELESAI</strong>. Silakan periksa kembali perangkat/layanan Anda.",
                "MenungguSparepart" => "Petugas telah melakukan pemeriksaan awal, dan penanganan memerlukan penggantian sparepart atau koordinasi vendor luar.",
                _ => $"Status laporan telah diperbarui dari <strong>{oldStatus}</strong> menjadi <strong>{newStatus}</strong>."
            };

            string extraNotice = newStatus == "Selesai" 
                ? $@"<div style=""background: #dff6dd; border: 1px solid #107c41; color: #107c41; padding: 16px; border-radius: 6px; margin: 20px 0; text-align: center;"">
                        <strong style=""font-size: 15px;"">⭐ Berikan Penilaian Kepuasan Layanan:</strong>
                        <p style=""margin: 6px 0 12px 0; font-size: 13px;"">Bantu kami meningkatkan kualitas layanan IT sekolah dengan memberikan rating bintang & ulasan.</p>
                        <a href=""https://baknusitcare.smkbn666.sch.id/tickets/detail/{ticket.Id}"" style=""background: #107c41; color: #ffffff; padding: 8px 20px; border-radius: 20px; text-decoration: none; font-weight: bold; display: inline-block;"">
                            Beri Rating & Ulasan Sekarang
                        </a>
                     </div>"
                : "";

            string commentBlock = !string.IsNullOrWhiteSpace(commentMessage)
                ? $"<div style=\"background: #f0f4f8; border-left: 4px solid #0078d4; padding: 12px 16px; margin: 15px 0; border-radius: 4px;\"><strong style=\"color: #0078d4;\">Catatan dari Petugas IT ({ticket.AssignedTechnicianName ?? "Tim IT Support"}):</strong><p style=\"margin: 6px 0 0 0; color: #333;\">{commentMessage}</p></div>"
                : "";

            string bodyHtml = $@"
<div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; padding: 30px 15px;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.08); border-top: 5px solid {statusBadgeColor};"">
        <div style=""padding: 24px; background-color: #0078d4; color: #ffffff;"">
            <h2 style=""margin: 0; font-size: 20px; font-weight: 600;"">BaknusITCare - Update Perkembangan Status</h2>
            <p style=""margin: 5px 0 0 0; font-size: 13px; opacity: 0.9;"">Laporan IT #{ticket.TicketCode} - {ticket.Title}</p>
        </div>
        <div style=""padding: 24px; color: #333333;"">
            <p style=""font-size: 16px;"">Halo <strong>{ticket.RequesterName}</strong>,</p>
            <p style=""font-size: 15px; color: #222;"">{statusDescription}</p>
            
            <div style=""text-align: center; margin: 20px 0;"">
                <span style=""background-color: {statusBadgeColor}; color: #ffffff; padding: 8px 22px; border-radius: 20px; font-size: 15px; font-weight: bold; display: inline-block;"">
                    Status: {statusHeadline}
                </span>
            </div>

            {commentBlock}
            {extraNotice}

            <div style=""background-color: #f9f9f9; border: 1px solid #e5e5e5; padding: 14px; border-radius: 6px; margin-top: 15px;"">
                <table style=""width: 100%; border-collapse: collapse; font-size: 13px;"">
                    <tr><td style=""padding: 4px 0; font-weight: 600; width: 140px; color: #666;"">Nomor Tiket:</td><td><strong style=""color: #0078d4;"">#{ticket.TicketCode}</strong></td></tr>
                    <tr><td style=""padding: 4px 0; font-weight: 600; color: #666;"">Judul Kendala:</td><td>{ticket.Title}</td></tr>
                    <tr><td style=""padding: 4px 0; font-weight: 600; color: #666;"">Petugas Penanggungjawab:</td><td><strong>{ticket.AssignedTechnicianName ?? "Tim IT Support"}</strong></td></tr>
                    <tr><td style=""padding: 4px 0; font-weight: 600; color: #666;"">Lokasi Ruangan:</td><td>{ticket.Location}</td></tr>
                </table>
            </div>

            <div style=""text-align: center; margin-top: 25px;"">
                <a href=""https://baknusitcare.smkbn666.sch.id/track/{ticket.TicketCode}"" style=""background-color: #0078d4; color: #ffffff; padding: 10px 24px; text-decoration: none; border-radius: 20px; font-weight: bold; display: inline-block; margin-bottom: 10px; box-shadow: 0 2px 6px rgba(0,0,0,0.12);"">
                    🔍 Lacak Status Tiket #{ticket.TicketCode} &rarr;
                </a>
                <br/>
                <a href=""https://baknusitcare.smkbn666.sch.id/tickets/detail/{ticket.Id}"" style=""color: #666666; font-size: 13px; text-decoration: underline;"">
                    Lihat Detail Lengkap & Riwayat Tiket
                </a>
            </div>
        </div>
        <div style=""padding: 16px 24px; background-color: #f8f9fa; border-top: 1px solid #eeeeee; font-size: 12px; color: #777777; text-align: center;"">
            &copy; 2026 Tim IT Infrastructure - SMK Bakti Nusantara 666.<br/>
            Email ini dikirimkan otomatis oleh Mailcow Server BaknusITCare.
        </div>
    </div>
</div>";

            // 1. Kirim ke Pembuat Tiket (Pelapor)
            bool reqResult = await SendEmailAsync(ticket.RequesterEmail, subject, bodyHtml);

            // 2. Kirim juga tembusan update ke Tim IT jika disediakan
            if (additionalRecipients != null && additionalRecipients.Any())
            {
                foreach (var recipient in additionalRecipients)
                {
                    if (string.IsNullOrWhiteSpace(recipient) || recipient.Equals(ticket.RequesterEmail, StringComparison.OrdinalIgnoreCase)) continue;
                    await SendEmailAsync(recipient.Trim(), subject, bodyHtml);
                }
            }

            return reqResult;
        }

        public async Task<bool> SendTicketAssignedAsync(Ticket ticket, string technicianName, string technicianEmail)
        {
            string subject = $"[BaknusITCare] Penugasan Tindak Lanjut Tiket IT #{ticket.TicketCode} - {ticket.Title}";
            
            string bodyHtml = $@"
<div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; padding: 30px 15px;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.08); border-top: 5px solid #d13438;"">
        <div style=""padding: 24px; background-color: #0078d4; color: #ffffff;"">
            <h2 style=""margin: 0; font-size: 22px; font-weight: 600;"">BaknusITCare - Penugasan Tindak Lanjut</h2>
            <p style=""margin: 5px 0 0 0; font-size: 14px; opacity: 0.9;"">Instruksi Penanganan Tiket IT (Admin Mailcow Delegate)</p>
        </div>
        <div style=""padding: 24px; color: #333333;"">
            <p style=""font-size: 16px;"">Yth. Bapak/Ibu <strong>{technicianName}</strong>,</p>
            <p>Admin IT telah menunjuk Anda untuk <strong>menindaklanjuti dan menyelesaikan</strong> laporan masalah IT berikut:</p>

            <div style=""background-color: #fff4ce; border-left: 4px solid #ffaa00; padding: 15px; margin: 20px 0; border-radius: 4px;"">
                <table style=""width: 100%; border-collapse: collapse;"">
                    <tr><td style=""padding: 4px 0; font-weight: 600; width: 140px;"">Kode Tiket:</td><td><span style=""background: #0078d4; color: #fff; padding: 2px 8px; border-radius: 4px; font-weight: bold;"">#{ticket.TicketCode}</span></td></tr>
                    <tr><td style=""padding: 4px 0; font-weight: 600;"">Judul Kendala:</td><td><strong>{ticket.Title}</strong></td></tr>
                    <tr><td style=""padding: 4px 0; font-weight: 600;"">Pelapor:</td><td>{ticket.RequesterName} ({ticket.RequesterEmail})</td></tr>
                    <tr><td style=""padding: 4px 0; font-weight: 600;"">Lokasi / Ruangan:</td><td>{ticket.Location}</td></tr>
                    <tr><td style=""padding: 4px 0; font-weight: 600;"">Tag / Kode Aset:</td><td>{ticket.AssetTag ?? "-"}</td></tr>
                    <tr><td style=""padding: 4px 0; font-weight: 600;"">Tingkat Urgensi:</td><td><strong style=""color: #d13438;"">{ticket.Priority}</strong></td></tr>
                    <tr><td style=""padding: 4px 0; font-weight: 600;"">Batas SLA:</td><td><strong style=""color: #d13438;"">{ticket.DueDate.AddHours(7):HH:mm, dd MMM yyyy}</strong></td></tr>
                </table>
            </div>

            <p><strong>Deskripsi Rinci Laporan:</strong></p>
            <p style=""background: #fafafa; border: 1px solid #e1e1e1; padding: 12px; border-radius: 4px; font-style: italic;"">{ticket.Description}</p>

            <p style=""margin-top: 20px;"">Mohon segera lakukan pengecekan di lokasi dan perbarui status tiket atau tambahkan catatan penanganan melalui aplikasi BaknusITCare.</p>
        </div>
        <div style=""padding: 16px 24px; background-color: #f8f9fa; border-top: 1px solid #eeeeee; font-size: 12px; color: #777777; text-align: center;"">
            &copy; 2026 Admin IT Infrastructure - SMK Bakti Nusantara 666.<br/>
            Email ini dikirimkan otomatis oleh Admin via Mailcow Auto-Responder.
        </div>
    </div>
</div>";

            return await SendEmailAsync(technicianEmail, subject, bodyHtml);
        }

        private static string? _overrideUsername;
        private static string? _overridePassword;

        public void UpdateSmtpCredentials(string username, string password)
        {
            if (!string.IsNullOrWhiteSpace(username)) _overrideUsername = username.Trim();
            if (!string.IsNullOrWhiteSpace(password)) _overridePassword = password;
        }

        public (string Username, string Password) GetCurrentSmtpCredentials()
        {
            string user = _overrideUsername ?? _config["Mailcow:SmtpUsername"] ?? "admin@smk.baktinusantara666.sch.id";
            string pass = _overridePassword ?? _config["Mailcow:SmtpPassword"] ?? "buhun666";
            return (user, pass);
        }

        private async Task<bool> SendEmailAsync(string recipientEmail, string subject, string bodyHtml)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail)) return false;

            string host = _config["Mailcow:SmtpHost"] ?? "mail.smk.baktinusantara666.sch.id";
            int port = int.TryParse(_config["Mailcow:SmtpPort"], out var p) ? p : 587;
            var (username, password) = GetCurrentSmtpCredentials();
            string senderName = _config["Mailcow:SenderName"] ?? "BaknusITCare - Layanan IT Sekolah";

            if (!MailboxAddress.TryParse(recipientEmail.Trim(), out var toAddress))
            {
                _logger.LogWarning("Format email penerima '{Recipient}' tidak valid, pengiriman dibatalkan.", recipientEmail);
                return false;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, username));
            message.To.Add(toAddress);
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = bodyHtml };

            var connectionTargets = new (string Host, int Port, SecureSocketOptions Sec)[]
            {
                (host, port, SecureSocketOptions.StartTls),
                (host, 465, SecureSocketOptions.SslOnConnect)
            };

            Exception? lastEx = null;
            foreach (var target in connectionTargets)
            {
                try
                {
                    using var smtp = new SmtpClient();
                    smtp.Timeout = 5000;
                    smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    smtp.CheckCertificateRevocation = false;

                    await smtp.ConnectAsync(target.Host, target.Port, target.Sec);
                    smtp.AuthenticationMechanisms.Remove("XOAUTH2");
                    await smtp.AuthenticateAsync(username, password);
                    await smtp.SendAsync(message);
                    await smtp.DisconnectAsync(true);

                    _logger.LogInformation("SUKSES: Email terkirim ke {Recipient} via {Host}:{Port} [{Subject}]", recipientEmail, target.Host, target.Port, subject);
                    return true;
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    _logger.LogWarning("Info: Percobaan SMTP ke {Host}:{Port} ({Error}). Mencoba jalur berikutnya...", target.Host, target.Port, ex.Message);
                }
            }

            _logger.LogError(lastEx, "GAGAL TOTAL: Tidak dapat mengirim email ke {Recipient} setelah mencoba seluruh jalur koneksi SMTP.", recipientEmail);
            return false;
        }

        public async Task<(bool Success, string Details)> TestEmailConnectionAsync(string recipientEmail, string? customSenderEmail = null, string? customSenderPassword = null)
        {
            var log = new System.Text.StringBuilder();
            log.AppendLine($"=== MEMULAI PENGUJIAN KONEKSI SMTP KE MAILCOW ===");
            log.AppendLine($"Waktu Pengujian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            log.AppendLine($"Target Email Penerima: {recipientEmail}");

            string host = _config["Mailcow:SmtpHost"] ?? "mail.smk.baktinusantara666.sch.id";
            int port = int.TryParse(_config["Mailcow:SmtpPort"], out var p) ? p : 587;
            var (defaultUser, defaultPass) = GetCurrentSmtpCredentials();
            string username = !string.IsNullOrWhiteSpace(customSenderEmail) ? customSenderEmail.Trim() : defaultUser;
            string password = !string.IsNullOrWhiteSpace(customSenderPassword) ? customSenderPassword : defaultPass;
            string senderName = _config["Mailcow:SenderName"] ?? "BaknusITCare - Layanan IT Sekolah";

            log.AppendLine($"Akun Pengirim (Kredensial): {username}");
            log.AppendLine($"Host & Port Terkonfigurasi: {host}:{port}");

            if (!MailboxAddress.TryParse(recipientEmail.Trim(), out var toAddress))
            {
                log.AppendLine($"[ERROR] Format alamat email penerima '{recipientEmail}' tidak valid.");
                return (false, log.ToString());
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, username));
            message.To.Add(toAddress);
            message.Subject = $"[TEST IT-CARE] Verifikasi Pengiriman Email Petugas IT ({DateTime.Now:HH:mm:ss})";
            
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = $@"
<div style=""font-family: Arial, sans-serif; padding: 20px; background-color: #f4f6f9;"">
    <div style=""max-width: 500px; margin: 0 auto; background: #fff; padding: 25px; border-radius: 8px; border-top: 5px solid #107c41; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
        <h3 style=""color: #107c41; margin-top: 0;"">✅ Uji Coba Pengiriman Email Berhasil!</h3>
        <p>Halo <strong>Petugas IT SMK Bakti Nusantara 666</strong>,</p>
        <p>Email ini dikirimkan secara langsung dari sistem <strong>BaknusITCare</strong> untuk memverifikasi bahwa akun pengirim <code>{username}</code> telah berfungsi 100% normal.</p>
        <div style=""background: #f0f7fd; padding: 12px; border-radius: 6px; font-size: 13px; color: #0078d4;"">
            <strong>Kredensial Aktif:</strong><br/>
            - Pengirim: {username}<br/>
            - Penerima: {recipientEmail}<br/>
            - Status: Terkirim Sukses via SMTP Mailcow
        </div>
        <p style=""font-size: 12px; color: #777; margin-top: 20px;"">&copy; 2026 Tim IT Infrastructure - SMK Bakti Nusantara 666</p>
    </div>
</div>"
            };

            var connectionTargets = new (string Host, int Port, SecureSocketOptions Sec)[]
            {
                (host, port, SecureSocketOptions.StartTls),
                (host, 465, SecureSocketOptions.SslOnConnect)
            };

            foreach (var target in connectionTargets)
            {
                log.AppendLine($"\n--> Menguji koneksi ke {target.Host}:{target.Port} (Mode: {target.Sec})...");
                try
                {
                    using var smtp = new SmtpClient();
                    smtp.Timeout = 5000;
                    smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    smtp.CheckCertificateRevocation = false;

                    await smtp.ConnectAsync(target.Host, target.Port, target.Sec);
                    log.AppendLine($"    [OK] Terhubung ke socket {target.Host}:{target.Port}.");

                    smtp.AuthenticationMechanisms.Remove("XOAUTH2");
                    log.AppendLine($"    [OK] Melakukan otentikasi dengan user '{username}'...");
                    
                    try
                    {
                        await smtp.AuthenticateAsync(username, password);
                        log.AppendLine($"    [OK] Otentikasi BERHASIL!");
                    }
                    catch (AuthenticationException authEx)
                    {
                        log.AppendLine($"    [GAGAL] Otentikasi ditolak: {authEx.Message}");
                        if (username.Contains("@"))
                        {
                            string localUser = username.Split('@')[0];
                            log.AppendLine($"    --> Mencoba alternatif otentikasi menggunakan username lokal '{localUser}'...");
                            try
                            {
                                await smtp.AuthenticateAsync(localUser, password);
                                log.AppendLine($"    [OK] Otentikasi BERHASIL menggunakan username '{localUser}'!");
                                username = localUser;
                            }
                            catch (Exception altEx)
                            {
                                log.AppendLine($"    [GAGAL] Alternatif '{localUser}' juga ditolak: {altEx.Message}");
                                log.AppendLine($"\n[DIAGNOSTIK PENTING]:");
                                log.AppendLine($"Error 535 artinya Mailcow Postfix/Dovecot menolak kredensial ini.");
                                log.AppendLine($"Penyebab utama:");
                                log.AppendLine($"1. Akun '{username}' belum dibuat sebagai 'Mailbox' (Kotak Surat) di Mailcow Web UI (Menu Mail Setup -> Mailboxes). Akun administrator web Mailcow tidak otomatis menjadi mailbox email!");
                                log.AppendLine($"2. Atau password mailbox di Mailcow berbeda dengan password yang dimasukkan.");
                                log.AppendLine($"Solusi: Buat Mailbox '{username}' di panel Mailcow atau gunakan email mailbox yang sudah aktif (misal akun staf/guru Anda) melalui formulir di atas.");
                                throw;
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }

                    log.AppendLine($"    [OK] Mengirim pesan email ke '{recipientEmail}'...");
                    await smtp.SendAsync(message);
                    log.AppendLine($"    [OK] Pesan DITERIMA oleh server SMTP Mailcow!");

                    await smtp.DisconnectAsync(true);
                    log.AppendLine($"\n=== KESIMPULAN: PENGIRIMAN EMAIL BERHASIL 100% VIA {target.Host}:{target.Port} ===");
                    return (true, log.ToString());
                }
                catch (Exception ex)
                {
                    log.AppendLine($"    [STATUS] Percobaan ke {target.Host}:{target.Port} selesai: {ex.GetType().Name} - {ex.Message}");
                }
            }

            log.AppendLine($"\n=== KESIMPULAN: GAGAL MENGIRIM KARENA OTENTIKASI/KREDENSIAL DITOLAK MAILCOW ===");
            return (false, log.ToString());
        }
    }
}

