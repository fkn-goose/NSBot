using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using NS2Bot.Models;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace NS2Bot.CommandModules
{
    public class TestModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("testcommand", "Тестовая команда")]
        [RequireOwner]
        public async Task AnonymousMessage([Summary(name: "Сообщение", description: "Текст сообщения")] string message, [Summary(name: "Изображение", description: "Прикрепить изображение")][Optional] IAttachment attachment)
        {
            await DeferAsync(ephemeral: true);

            HttpClient client = new HttpClient();
            List<byte[]> files = new List<byte[]>();

            WebhookModel webhookModel = new()
            {
                username = "Аноним",
                avatar_url = "https://media.moddb.com/cache/images/downloads/1/221/220278/thumb_620x2000/No_data.png"
            };

            message = message.Replace(@"\n", "\n");
            webhookModel.content = message;
            string json = JsonSerializer.Serialize(webhookModel);
            List<SocketMessage> messages = new List<SocketMessage>();

            MultipartFormDataContent msgData = new MultipartFormDataContent();
            msgData.Add(new StringContent(json), "payload_json");
            MemoryStream ms = new MemoryStream();
            var fileByte = client.GetStreamAsync(new Uri(attachment.ProxyUrl)).Result;
            fileByte.CopyTo(ms);
            msgData.Add(new ByteArrayContent(ms.ToArray()), "Photo", attachment.Filename);


            //var fileByte = File.ReadAllBytes(TempImgFolder + attachment.Filename);  
            //msgData.Add(new ByteArrayContent(fileByte, 0, fileByte.Length));

            //var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://discord.com/api/webhooks/1218717671705677976/rnbokuykXm3PwNRXkSjU-ZSTbTolxKP3MkRR5s69kJCQsLyHHBYeNisDhysx5nFuR0ti", msgData);
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject o = JObject.Parse(responseContent);
            ulong id = o["id"].Value<ulong>();
            await MainData.logger.LogAsync(new LogMessage(LogSeverity.Info, "ImgTest", responseContent));
            await FollowupAsync("Отправлено", ephemeral: true);
        }
    }
}
