using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Data.Service.Auth
{
    public class EmailService : IEmailService
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
                Xin chào bạn, chào mừng bạn đến với website bán hàng thời trang trực tuyến VYBE

                Mã OTP xác thực tài khoản của bạn là: {otp}

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

        public async Task SendContactEmail(
    string fullName,
    string email,
    string phone,
    string subject,
    string message)
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

                mail.To.Add(fromEmail);

                mail.Subject = $"📩 Liên hệ mới - {subject}";

                mail.Body = $@"
            <div style='font-family:Arial;padding:20px;'>

                <h2 style='color:#111;'>
                    Liên hệ mới từ VYBE
                </h2>

                <p>
                    <b>Họ tên:</b> {fullName}
                </p>

                <p>
                    <b>Email:</b> {email}
                </p>

                <p>
                    <b>Số điện thoại:</b> {phone}
                </p>

                <p>
                    <b>Chủ đề:</b> {subject}
                </p>

                <p>
                    <b>Nội dung:</b>
                </p>

                <div style='background:#f5f5f5;padding:15px;border-radius:10px;'>
                    {message}
                </div>

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

        public async Task SendOrderSuccessEmail(
    string toEmail,
    string customerName,
    string transactionCode,
    decimal total,
    string address)
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
                    "🛍️ Đặt hàng thành công tại VYBE";

                mail.Body = $@"
        <div style='font-family:Arial;padding:20px;background:#f9f9f9;'>

            <div style='max-width:600px;margin:auto;background:white;
                        border-radius:12px;padding:30px;'>

                <h2 style='color:#111;text-align:center;'>
                    Cảm ơn bạn đã đặt hàng ❤️
                </h2>

                <p>
                    Xin chào <b>{customerName}</b>,
                </p>

                <p>
                    Đơn hàng của bạn đã được tạo thành công tại
                    <b>VYBE</b>.
                </p>

                <hr/>

                <h3>📦 Thông tin đơn hàng</h3>

                <p>
                    <b>Mã đơn hàng:</b>
                    {transactionCode}
                </p>

                <p>
                    <b>Tổng tiền:</b>
                    {total:N0} VNĐ
                </p>

                <p>
                    <b>Địa chỉ nhận:</b>
                    {address}
                </p>

                <hr/>

                <p>
                    Shop sẽ sớm xác nhận và giao hàng cho bạn.
                </p>

                <p>
                    Cảm ơn bạn đã đồng hành cùng
                    <b>VYBE</b> 💖
                </p>

            </div>

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

        public async Task SendOrderConfirmedEmail(
    string toEmail,
    string customerName,
    string transactionCode)
        {
            var fromEmail = _configuration["EmailSettings:Email"];
            var password = _configuration["EmailSettings:Password"];
            var host = _configuration["EmailSettings:Host"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(fromEmail);

                mail.To.Add(toEmail);

                mail.Subject = "✅ Đơn hàng đã được xác nhận";

                mail.Body = $@"
        <div style='font-family:Arial;padding:20px;'>

            <h2>Đơn hàng đã được xác nhận 🎉</h2>

            <p>
                Xin chào <b>{customerName}</b>
            </p>

            <p>
                Đơn hàng <b>{transactionCode}</b>
                đã được shop xác nhận.
            </p>

            <p>
                Shop đang chuẩn bị hàng cho bạn ❤️
            </p>

        </div>
        ";

                mail.IsBodyHtml = true;

                using (SmtpClient smtp =
                    new SmtpClient(host, port))
                {
                    smtp.Credentials =
                        new NetworkCredential(fromEmail, password);

                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(mail);
                }
            }
        }

        public async Task SendShippingEmail(
    string toEmail,
    string customerName,
    string transactionCode)
        {
            var fromEmail = _configuration["EmailSettings:Email"];
            var password = _configuration["EmailSettings:Password"];
            var host = _configuration["EmailSettings:Host"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(fromEmail);

                mail.To.Add(toEmail);

                mail.Subject = "🚚 Đơn hàng đang được giao";

                mail.Body = $@"
        <div style='font-family:Arial;padding:20px;'>

            <h2>Đơn hàng đang được giao 🚚</h2>

            <p>
                Xin chào <b>{customerName}</b>
            </p>

            <p>
                Đơn hàng <b>{transactionCode}</b>
                đang được giao đến bạn.
            </p>

            <p>
                Vui lòng chú ý điện thoại để nhận hàng nhé ❤️
            </p>

        </div>
        ";

                mail.IsBodyHtml = true;

                using (SmtpClient smtp =
                    new SmtpClient(host, port))
                {
                    smtp.Credentials =
                        new NetworkCredential(fromEmail, password);

                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(mail);
                }
            }
        }

        public async Task SendCancelOrderEmail(
    string toEmail,
    string customerName,
    string transactionCode)
        {
            var fromEmail = _configuration["EmailSettings:Email"];
            var password = _configuration["EmailSettings:Password"];
            var host = _configuration["EmailSettings:Host"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(fromEmail);

                mail.To.Add(toEmail);

                mail.Subject = "❌ Đơn hàng đã bị huỷ";

                mail.Body = $@"
        <div style='font-family:Arial;padding:20px;'>

            <h2>Đơn hàng đã bị huỷ ❌</h2>

            <p>
                Xin chào <b>{customerName}</b>
            </p>

            <p>
                Đơn hàng <b>{transactionCode}</b>
                đã bị huỷ do quá 24h chưa thanh toán.
            </p>

            <p>
                Bạn có thể quay lại website để đặt hàng lại bất cứ lúc nào ❤️
            </p>

        </div>
        ";

                mail.IsBodyHtml = true;

                using (SmtpClient smtp =
                    new SmtpClient(host, port))
                {
                    smtp.Credentials =
                        new NetworkCredential(fromEmail, password);

                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(mail);
                }
            }
        }
    }
}