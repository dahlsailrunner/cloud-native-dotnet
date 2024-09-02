using System.Net.Mail;
using MailKit.Client;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace CarvedRock.WebApp;

public class EmailService(MailKitClientFactory factory) : IEmailSender
{
    //private readonly SmtpClient _client;
    //public EmailService(IConfiguration config)
    //{
    //    var smtpUri = new Uri(config.GetConnectionString("SmtpUri")!);
    //    _client = new() { Host = smtpUri.Host, Port = smtpUri.Port  };
    //}

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {   
        var client = await factory.GetSmtpClientAsync(); // added with MailKit

        using var message = new MailMessage
        {
            Body = htmlMessage,
            Subject = subject,
            IsBodyHtml = true,
            From = new MailAddress("e-commerce@carvedrock.com", "Carved Rock Shop"),
            To = { email }
        };        

        //await _client.SendMailAsync(mailMessage);
        await client.SendAsync(MimeMessage.CreateFromMailMessage(message));
    }
}
