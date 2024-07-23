using Discord;
using Discord.Interactions;
using Discord.Webhook;
using Discord.WebSocket;
using NS.Bot.App;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NS2Bot.CommandModules
{
    public class TestModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("testcommand", "Тестовая команда")]
        [RequireOwner]
        public async Task AnonymousMessage([Summary(name: "Сообщение", description: "Текст сообщения")] string message, [Summary(name: "Изображение", description: "Прикрепить изображение")][Optional][DefaultParameterValue(null)] IAttachment? attachment)
        {
            HttpClient client = new HttpClient();
            DiscordWebhookClient webhookClient = new DiscordWebhookClient("https://discord.com/api/webhooks/1218717671705677976/rnbokuykXm3PwNRXkSjU-ZSTbTolxKP3MkRR5s69kJCQsLyHHBYeNisDhysx5nFuR0ti");
            List<byte[]> files = new List<byte[]>();

            await DeferAsync(ephemeral: true);

            message = message.Replace(@"\n", "\n");
            List<SocketMessage> messages = new List<SocketMessage>();

            //var fileByte = File.ReadAllBytes(TempImgFolder + attachment.Filename);  
            //msgData.Add(new ByteArrayContent(fileByte, 0, fileByte.Length));

            //var content = new StringContent(json, Encoding.UTF8, "application/json");
            var asd = webhookClient.SendMessageAsync(message, username: "Аноним", avatarUrl: "https://media.moddb.com/cache/images/downloads/1/221/220278/thumb_620x2000/No_data.png");
            //var asd = webhookClient.SendFileAsync(stream: client.GetStreamAsync(new Uri(attachment.ProxyUrl)).Result, filename: attachment.Filename, text: message, username: "Аноним", avatarUrl: "https://media.moddb.com/cache/images/downloads/1/221/220278/thumb_620x2000/No_data.png");
            ulong id = asd.Result;
            await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "ImgTest", "123"));
            await FollowupAsync("Отправлено", ephemeral: true);
        }
    }
}
