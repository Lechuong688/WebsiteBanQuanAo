using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace Data.Service.ChatBot
{
    public class ChatBotService
    {
        private readonly string apiKey;

        private readonly string apiUrl;

        public ChatBotService(
            IConfiguration configuration)
        {
            apiKey =
                configuration["GroqAI:ApiKey"];

            apiUrl =
                configuration["GroqAI:ApiUrl"];
        }

        public async Task<string> AskAI(
            string prompt)
        {
            try
            {
                using var httpClient =
                    new HttpClient();

                httpClient.Timeout =
                    TimeSpan.FromSeconds(15);

                httpClient.DefaultRequestHeaders.Add(
                    "Authorization",
                    $"Bearer {apiKey}");

                var requestBody = new
                {
                    model =
                    "llama-3.1-8b-instant",

                    messages = new object[]
                    {
                        new
                        {
                            role = "system",

                            content =
                            "Bạn là Vybe AI của website thời trang VYBE."
                        },

                        new
                        {
                            role = "user",

                            content = prompt
                        }
                    },

                    temperature = 0.7,

                    max_tokens = 300
                };

                var json =
                    JsonConvert.SerializeObject(
                        requestBody);

                var content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json");

                var response =
                    await httpClient.PostAsync(
                        apiUrl,
                        content);

                var responseString =
                    await response.Content
                        .ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return responseString;
                }

                dynamic result =
                    JsonConvert.DeserializeObject(
                        responseString);

                return result.choices[0]
                    .message.content
                    .ToString();
            }
            catch (Exception ex)
            {
                return "AI lỗi: " + ex.Message;
            }
        }
    }
}