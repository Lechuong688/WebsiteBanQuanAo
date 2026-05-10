using Data.Entity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace Data.Service.ChatBot
{
    public class ChatBotService
    {
        private readonly string apiKey;
        private readonly string apiUrl;

        public ChatBotService(IConfiguration configuration)
        {
            apiKey = configuration["GroqAI:ApiKey"];
            apiUrl = configuration["GroqAI:ApiUrl"];
        }

        public async Task<string> AskAI(string systemPrompt, string userMessage, List<ChatMessageEntity> history = null)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var messageList = new List<object>();

                messageList.Add(new { role = "system", content = systemPrompt });

                if (history != null && history.Any())
                {
                    var recentHistory = history.OrderBy(x => x.CreatedDate).TakeLast(8);
                    foreach (var msg in recentHistory)
                    {
                        string role = (msg.SenderType == "AI") ? "assistant" : "user";
                        messageList.Add(new { role = role, content = msg.Message });
                    }
                }

                messageList.Add(new { role = "user", content = userMessage });

                var requestBody = new
                {
                    model = "llama-3.1-8b-instant",
                    messages = messageList,
                    temperature = 0.5,
                    max_tokens = 400
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(apiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return "AI lỗi: " + responseString;
                }

                dynamic result = JsonConvert.DeserializeObject(responseString);
                return result.choices[0].message.content.ToString();
            }
            catch (Exception ex)
            {
                return "AI lỗi: " + ex.Message;
            }
        }
    }
}