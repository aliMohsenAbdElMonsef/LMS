using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    internal class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(EmailSettings emailSettings)
        {
            _emailSettings = emailSettings;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (_emailSettings.UseMockEmail)
            {
                await Task.Delay(100);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                
                if (!string.IsNullOrEmpty(_emailSettings.Username))
                {
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }

        public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachmentData, string attachmentName)
        {
            if (_emailSettings.UseMockEmail)
            {

                await Task.Delay(100);
                return;
            }


            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };


            builder.Attachments.Add(attachmentName, attachmentData);
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort,
                    _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                if (!string.IsNullOrEmpty(_emailSettings.Username))
                {
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email with attachment: {ex.Message}", ex);
            }
        }
    }
}
