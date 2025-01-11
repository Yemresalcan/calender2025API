using System.Net;
using System.Net.Mail;

namespace CalendarAPI.Services
{
    public interface IEmailService
    {
        Task SendResetPasswordEmailAsync(string email, string resetToken);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendResetPasswordEmailAsync(string email, string resetToken)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                _logger.LogInformation($"SMTP Ayarları yükleniyor...");

                var resetUrl = $"{_configuration["AppSettings:FrontendUrl"]}/reset-password/{resetToken}";

                using var smtpClient = new SmtpClient()
                {
                    Host = smtpSettings["Host"],
                    Port = int.Parse(smtpSettings["Port"]),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Timeout = 30000 // 30 saniye
                };

                // Kimlik bilgilerini ayrı ayarla
                var credentials = new NetworkCredential(
                    smtpSettings["Username"],
                    smtpSettings["Password"]
                );
                smtpClient.Credentials = credentials;

                _logger.LogInformation($"SMTP Client yapılandırıldı: {smtpClient.Host}:{smtpClient.Port}");

                using var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(smtpSettings["FromEmail"], smtpSettings["FromName"]);
                mailMessage.To.Add(new MailAddress(email));
                mailMessage.Subject = "Şifre Sıfırlama Talebi";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #4f46e5;'>Şifre Sıfırlama</h2>
                        <p>Merhaba,</p>
                        <p>Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:</p>
                        <p style='margin: 20px 0;'>
                            <a href='{resetUrl}' 
                               style='background-color: #4f46e5; 
                                      color: white; 
                                      padding: 10px 20px; 
                                      text-decoration: none; 
                                      border-radius: 5px;
                                      display: inline-block;'>
                                Şifremi Sıfırla
                            </a>
                        </p>
                        <p><strong>Not:</strong> Bu bağlantı 1 saat süreyle geçerlidir.</p>
                        <p style='color: #666; font-size: 0.9em;'>
                            Eğer bu talebi siz yapmadıysanız, bu emaili görmezden gelebilirsiniz.
                        </p>
                    </div>";

                _logger.LogInformation($"Mail göndermeye çalışılıyor: {email}");

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                    _logger.LogInformation($"Mail başarıyla gönderildi: {email}");
                }
                catch (SmtpException smtpEx)
                {
                    _logger.LogError($"SMTP Hatası: {smtpEx.StatusCode}");
                    _logger.LogError($"SMTP Hata Mesajı: {smtpEx.Message}");
                    _logger.LogError($"SMTP Detay: {smtpEx.StackTrace}");
                    
                    if (smtpEx.InnerException != null)
                    {
                        _logger.LogError($"İç Hata: {smtpEx.InnerException.Message}");
                    }
                    
                    throw new Exception($"Mail gönderimi başarısız: {smtpEx.Message}", smtpEx);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Genel Hata: {ex.Message}");
                _logger.LogError($"Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError($"İç Hata: {ex.InnerException.Message}");
                }
                
                throw;
            }
        }
    }
} 