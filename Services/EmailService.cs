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

            <p style=""margin-top: 25px;"">Tim Teknisi IT akan segera memproses laporan Anda. Anda akan menerima pemberitahuan email otomatis setiap kali ada pembaruan status.</p>
        </div>
        <div style=""padding: 16px 24px; background-color: #f8f9fa; border-top: 1px solid #eeeeee; font-size: 12px; color: #777777; text-align: center;"">
            &copy; 2026 Tim IT Infrastructure - SMK Bakti Nusantara 666.<br/>
            Email ini dikirimkan secara otomatis dari sistem auto-responder BaknusITCare.
        </div>
    </div>
</div>";

            return await SendEmailAsync(ticket.RequesterEmail, subject, bodyHtml);
        }

        public async Task<bool> SendTicketStatusUpdatedAsync(Ticket ticket, string oldStatus, string newStatus, string? commentMessage = null)
        {
            string subject = $"[BaknusITCare] Pembaharuan Status Tiket #{ticket.TicketCode}: {newStatus}";
            
            string statusBadgeColor = newStatus switch
            {
                "Diproses" => "#ffaa00",
                "MenungguSparepart" => "#8e44ad",
                "Selesai" => "#107c41",
                "Ditutup" => "#666666",
                _ => "#0078d4"
            };

            string extraNotice = newStatus == "Selesai" 
                ? "<p style=\"background: #dff6dd; border: 1px solid #107c41; color: #107c41; padding: 12px; border-radius: 4px; font-weight: 600;\">Tiket Anda telah dinyatakan SELESAI. Mohon luangkan waktu 30 detik untuk memberikan Rating & Ulasan Kepuasan di aplikasi BaknusITCare.</p>"
                : "";

            string commentBlock = !string.IsNullOrWhiteSpace(commentMessage)
                ? $"<p><strong>Pesan dari Petugas IT ({ticket.AssignedTechnicianName ?? "IT Support"}):</strong></p><p style=\"background: #f0f4f8; border-left: 4px solid #0078d4; padding: 12px; border-radius: 4px;\">{commentMessage}</p>"
                : "";

            string bodyHtml = $@"
<div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; padding: 30px 15px;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.08); border-top: 5px solid {statusBadgeColor};"">
        <div style=""padding: 24px; background-color: #0078d4; color: #ffffff;"">
            <h2 style=""margin: 0; font-size: 22px; font-weight: 600;"">BaknusITCare - Update Status Tiket</h2>
            <p style=""margin: 5px 0 0 0; font-size: 14px; opacity: 0.9;"">Laporan IT #{ticket.TicketCode}</p>
        </div>
        <div style=""padding: 24px; color: #333333;"">
            <p style=""font-size: 16px;"">Halo <strong>{ticket.RequesterName}</strong>,</p>
            <p>Ada pembaharuan status pada laporan kendala IT Anda:</p>
            
            <div style=""text-align: center; margin: 20px 0;"">
                <span style=""background-color: {statusBadgeColor}; color: #ffffff; padding: 8px 18px; border-radius: 20px; font-size: 16px; font-weight: bold; display: inline-block;"">
                    Status Baru: {newStatus}
                </span>
            </div>

            {extraNotice}
            {commentBlock}

            <table style=""width: 100%; border-collapse: collapse; margin-top: 15px; font-size: 14px;"">
                <tr><td style=""padding: 4px 0; font-weight: 600; width: 140px;"">Judul Kendala:</td><td>{ticket.Title}</td></tr>
                <tr><td style=""padding: 4px 0; font-weight: 600;"">Petugas Penanggungjawab:</td><td>{ticket.AssignedTechnicianName ?? "Staf IT Support"}</td></tr>
                <tr><td style=""padding: 4px 0; font-weight: 600;"">Lokasi:</td><td>{ticket.Location}</td></tr>
            </table>

            <p style=""margin-top: 25px;"">Terima kasih atas kesabaran Anda selama penanganan kendala IT ini.</p>
        </div>
        <div style=""padding: 16px 24px; background-color: #f8f9fa; border-top: 1px solid #eeeeee; font-size: 12px; color: #777777; text-align: center;"">
            &copy; 2026 Tim IT Infrastructure - SMK Bakti Nusantara 666.
        </div>
    </div>
</div>";

            return await SendEmailAsync(ticket.RequesterEmail, subject, bodyHtml);
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

        private async Task<bool> SendEmailAsync(string recipientEmail, string subject, string bodyHtml)
        {
            try
            {
                string host = _config["Mailcow:SmtpHost"] ?? "mail.smk.baktinusantara666.sch.id";
                int port = int.TryParse(_config["Mailcow:SmtpPort"], out var p) ? p : 587;
                string username = _config["Mailcow:SmtpUsername"] ?? "admin@smk.baktinusantara666.sch.id";
                string password = _config["Mailcow:SmtpPassword"] ?? "buhun666";
                string senderName = _config["Mailcow:SenderName"] ?? "BaknusITCare - Layanan IT Sekolah";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, username));
                message.To.Add(MailboxAddress.Parse(recipientEmail));
                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html) { Text = bodyHtml };

                using var smtp = new SmtpClient();
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTlsWhenAvailable);
                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Berhasil mengirim email penugasan ke {Recipient} dengan subjek '{Subject}'", recipientEmail, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengirim email penugasan ke {Recipient}", recipientEmail);
                return false;
            }
        }
    }
}
