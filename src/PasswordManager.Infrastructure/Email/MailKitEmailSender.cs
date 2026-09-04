using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using PasswordManager.Application.Interfaces.Services;
using PasswordManager.Application.Options;

namespace PasswordManager.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> smtpOptions, ILogger<SmtpEmailSender> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Alıcı adresi boş olamaz.", nameof(to));

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromAddress));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpOptions.Username, _smtpOptions.Password);
            await client.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mail gönderilirken hata oluştu. Alıcı: {To}", to);
            throw;
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true);
        }
    }

    
}