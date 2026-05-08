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
    }
}
