using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Data.Service.Auth
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpEmail(string toEmail, string otp)
        {
            var fromEmail = _configuration["EmailSettings:Email"];
            var password = _configuration["EmailSettings:Password"];
            var host = _configuration["EmailSettings:Host"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(fromEmail);
                mail.To.Add(toEmail);

                mail.Subject = "Mã OTP xác thực tài khoản";

                mail.Body = $@"
                    Xin chào,

                    Mã OTP xác thực tài khoản của bạn là:

                    {otp}

                    OTP có hiệu lực trong 5 phút.
                ";

                mail.IsBodyHtml = false;

                using (SmtpClient smtp = new SmtpClient(host, port))
                {
                    smtp.Credentials =
                        new NetworkCredential(fromEmail, password);

                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(mail);
                }
            }
        }

        public async Task SendWelcomeEmail(
    string toEmail,
    string name)
        {
            var fromEmail =
                _configuration["EmailSettings:Email"];

            var password =
                _configuration["EmailSettings:Password"];

            var host =
                _configuration["EmailSettings:Host"];

            var port =
                int.Parse(_configuration["EmailSettings:Port"]);

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(fromEmail);

                mail.To.Add(toEmail);

                mail.Subject =
                    "🎉 Đăng ký tài khoản thành công";

                mail.Body = $@"
            <div style='font-family:Arial;padding:20px;'>

                <h2 style='color:#111;'>
                    Xin chào {name} 👋
                </h2>

                <p>
                    Chào mừng bạn đến với
                    <b>VYBE</b>.
                </p>

                <p>
                    Tài khoản của bạn đã được
                    đăng ký thành công.
                </p>

                <p>
                    Tại VYBE bạn có thể:
                </p>

                <ul>
                    <li>Mua sắm thời trang mới nhất</li>
                    <li>Theo dõi đơn hàng</li>
                    <li>Nhận ưu đãi độc quyền</li>
                    <li>Lưu sản phẩm yêu thích</li>
                </ul>

                <br/>

                <p>
                    Cảm ơn bạn đã đồng hành cùng VYBE ❤️
                </p>

            </div>
        ";

                mail.IsBodyHtml = true;

                using (SmtpClient smtp =
                    new SmtpClient(host, port))
                {
                    smtp.Credentials =
                        new NetworkCredential(
                            fromEmail,
                            password);

                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(mail);
                }
            }
        }
    }
}