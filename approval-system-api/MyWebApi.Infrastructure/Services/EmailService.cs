using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MyWebApi.Domain.Constants;
using MyWebApi.Domain.Interfaces;

namespace MyWebApi.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var host = _configuration[ConfigKeys.MailSettings.Host];
            var port = int.Parse(_configuration[ConfigKeys.MailSettings.Port]);
            var mailFrom = _configuration[ConfigKeys.MailSettings.Mail];
            var displayName = _configuration[ConfigKeys.MailSettings.DisplayName] ?? "系統通知";
            var userName = _configuration[ConfigKeys.MailSettings.UserName];
            var password = _configuration[ConfigKeys.MailSettings.Password];

            var email = new MimeMessage();

            //寄件人
            email.From.Add(new MailboxAddress(displayName, mailFrom));

            // 收件者
            email.To.Add(new MailboxAddress("", toEmail));

            // 主旨
            email.Subject = subject;

            // 郵件內容
            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    //連線到 SMTP 伺服器
                    await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);

                    //驗證身份
                    await client.AuthenticateAsync(userName, password);

                    //寄送
                    string a = await client.SendAsync(email);

                    _logger.LogInformation("郵件已成功寄出。收件人: {Receiver}, 時間: {Time}", toEmail, DateTime.Now);
                }
                catch (SmtpCommandException ex)
                {
                    // 5.1.3 代表 Bad recipient address syntax
                    if (ex.ErrorCode == SmtpErrorCode.RecipientNotAccepted || ex.Message.Contains("5.1.3"))
                    {
                        _logger.LogError("郵件格式錯誤，取消重試。收件人: {Receiver}, 錯誤: {Error}", toEmail, ex.Message);

                        return;
                    }

                    _logger.LogWarning("SMTP 指令失敗: {ErrorMsg}。收件人: {Receiver}", ex.Message, toEmail);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("寄信失敗 ({ErrorMsg})。收件人: {Receiver}", ex.Message, toEmail);
                    throw;
                }
                finally
                {
                    //斷開連線
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}
