using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace api_infor_cell.src.Handlers
{
    public class MailHandler
    {
        private readonly string EmailFrom = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? "";
        private readonly string ResendApiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? "";

        public async Task SendMailAsync(string recipient, string subject, string body)
        {
            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ResendApiKey);

            var payload = new
            {
                from = EmailFrom,
                to = new[] { recipient },
                subject = subject,
                html = body
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync("https://api.resend.com/emails", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Resend error: {responseBody}");
        } 
    }
}