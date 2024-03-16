using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using NS2Bot.Models;
using System.Net;
using System.Runtime.InteropServices;

namespace NS2Bot.CommandModules
{
    public class TestModule : InteractionModuleBase<SocketInteractionContext>
    {
        private const string TempImgFolder = "tempImg";

        [SlashCommand("testcommand", "Тестовая команда")]
        [RequireOwner]
        public async Task AnonymousMessage([Summary(name: "Сообщение", description: "Текст сообщения")] string message, [Summary(name: "Файл", description: "Прикрепить файл")] [Optional] IAttachment? attachment)
        {
            HttpClient client = new HttpClient();

            WebhookModel webhookModel = new()
            {
                username = "Аноним",
                avatar_url = "https://media.moddb.com/cache/images/downloads/1/221/220278/thumb_620x2000/No_data.png"
            };

            message = message.Replace(@"\n", "\n");
            webhookModel.content = message;
            List<SocketMessage> messages = new List<SocketMessage>();

            MultipartFormDataContent msgData = new MultipartFormDataContent();
            WebClient downloader = new WebClient();
            await downloader.DownloadFileTaskAsync(new Uri(attachment.ProxyUrl), attachment.Filename);

            //string json = JsonSerializer.Serialize(webhookModel);
            //var content = new StringContent(json, Encoding.UTF8, "application/json");
            //var response = await client.PostAsync("https://discord.com/api/webhooks/1218540209105408090/Ux5o8rPLxJkJi1-cDc4VJyXXvh9jpYVRN2U3kSqT7X-86DCoAWUvtifEHzGm5l23FU36", content);
            //string responseContent = await response.Content.ReadAsStringAsync();

            await RespondAsync("Отправлено", ephemeral: true);
        }
    }
}
