using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CarvedRock.WebApp;

public class EmailService : IEmailSender
{
    private readonly SmtpClient _client;
    public EmailService(IConfiguration config)
    {
        var smtpUri = new Uri(config.GetConnectionString("SmtpUri")!);
        _client = new() { Host = smtpUri.Host, Port = smtpUri.Port  };
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {       
        var mailMessage = new MailMessage
        {
            Body = htmlMessage,
            Subject = subject,
            IsBodyHtml = true,
            From = new MailAddress("e-commerce@carvedrock.com", "Carved Rock Shop"),
            To = { email }
        };        

        await _client.SendMailAsync(mailMessage);
    }
}
