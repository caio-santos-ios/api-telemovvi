using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MailKit.Net.Smtp;
using MimeKit;

namespace api_infor_cell.src.Handlers
{
    public class MailHandler
    {
        private readonly string EmailFrom = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? "";
        private readonly string Password = Environment.GetEnvironmentVariable("PASSWORD_EMAIL") ?? "";
        private readonly string ResendApiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? "";
        public async Task SendMailAsync(string recipient, string subject, string body)
        {
            try
            {
                MimeMessage mensagem = new();
                mensagem.From.Add(MailboxAddress.Parse(EmailFrom));
                mensagem.To.Add(MailboxAddress.Parse(recipient));
                mensagem.Subject = subject;

                mensagem.Body = new TextPart("html")
                {
                    Text = body
                };

                using SmtpClient smtp = new();
                await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(EmailFrom, Password);
                await smtp.SendAsync(mensagem);
                await smtp.DisconnectAsync(true);
                
            }
            catch(Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        } 
        public async Task SendMailResendAsync(string recipient, string subject, string body)
        {
            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ResendApiKey);

            var payload = new
            {
                from = EmailFrom,
                to = new[] { recipient },
                subject,
                html = body
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync("https://api.resend.com/emails", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) throw new Exception($"Resend error: {responseBody}");
        } 
    }
}