using System.Net.Mail;

namespace MVC.Services.Interfaces
{
    public interface ISMTPService
    {
        bool SendEMail(MailMessage message);
    }
}
