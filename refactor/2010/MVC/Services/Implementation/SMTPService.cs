using System.Net.Mail;
using MVC.Services.Interfaces;

namespace MVC.Services.Implementation
{
    public class SMTPService : ISMTPService
    {
        public bool SendEMail(MailMessage message)
        {
            //Not Implemented
            return true;
        }
    }
}