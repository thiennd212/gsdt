using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GSDT.Notifications.Infrastructure.Email;

/// <summary>IEmailSender implementation using MailKit SMTP (Vault-backed config).</summary>
public sealed class MailKitEmailSender(
    IOptions<SmtpOptions> options,
    ILogger<MailKitEmailSender> logger) : IEmailSender
{
    private readonly SmtpOptions _opts = options.Value;

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_opts.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        try
        {
            var secureSocketOptions = _opts.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(_opts.Host, _opts.Port, secureSocketOptions, cancellationToken);

            if (!string.IsNullOrEmpty(_opts.Username))
                await client.AuthenticateAsync(_opts.Username, _opts.Password, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            logger.LogInformation("Email sent to {To} subject={Subject}", to, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
}

/// <summary>SMTP configuration — loaded from appsettings; credentials from Vault in production.</summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Email";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string From { get; set; } = "no-reply@gov.vn";
    public bool UseSsl { get; set; } = false;
    public string? Username { get; set; }
    public string? Password { get; set; }
}
