using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Service.Auth
{
    public interface IEmailService
    {
        Task SendOtpEmail(string toEmail, string otp);

        Task SendWelcomeEmail(string toEmail, string name);

        Task SendContactEmail(
            string fullName,
            string email,
            string phone,
            string subject,
            string message);

        Task SendOrderSuccessEmail(
            string toEmail,
            string customerName,
            string transactionCode,
            decimal total,
            string address
        );

        Task SendOrderConfirmedEmail(
            string toEmail,
            string customerName,
            string transactionCode
        );

        Task SendShippingEmail(
            string toEmail,
            string customerName,
            string transactionCode
        );

        Task SendCancelOrderEmail(
            string toEmail,
            string customerName,
            string transactionCode
        );

        Task SendEmailAsync(
            string toEmail,
            string subject,
            string body);
    }
}
