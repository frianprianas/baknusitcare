namespace BaknusITCare.Models
{
    public class MailcowConfig
    {
        public string ApiUrl { get; set; } = "https://mail.smk.baktinusantara666.sch.id";
        public string ApiKey { get; set; } = "925B68-0FF6BB-36B760-F6C051-AAF343";
        public string SmtpHost { get; set; } = "mail.smk.baktinusantara666.sch.id";
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = "admin@smk.baktinusantara666.sch.id";
        public string SmtpPassword { get; set; } = "buhun666";
        public string SenderName { get; set; } = "BaknusITCare - Layanan IT Sekolah";
    }
}
