using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using NS2Bot.Models;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Timers;

namespace NS2Bot.CommandModules
{
    public class PDAModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("setpdachannel", "Установить общий-кпк канал")]
        [RequireOwner]
        public async Task SetPublicPDAChannel()
        {
            MainData.configData.PublicPDAChannelId = Context.Channel.Id;
            await RespondAsync("Канал установлен как \"Общий-кпк\"", ephemeral: true);
        }

        [SlashCommand("anonymous", "Отправить анонимное сообщение")]
        public async Task AnonymousMessage([Summary(name: "Сообщение", description: "Текст сообщения")] string message, [Summary(name: "Изображение", description: "Прикрепить изображение")][Optional] IAttachment attachment)
        {   
            HttpClient client = new HttpClient();
            WebhookModel webhookModel = new()
            {
                username = "Аноним",
                avatar_url = "https://media.moddb.com/cache/images/downloads/1/221/220278/thumb_620x2000/No_data.png"
            };

            await DeferAsync(ephemeral:true);

            message = message.Replace(@"\n", "\n");
            webhookModel.content = message;

            MultipartFormDataContent msgData = new MultipartFormDataContent
            {
                { new StringContent(JsonSerializer.Serialize(webhookModel)), "payload_json" }
            };

            if (attachment != null && attachment.Width != null)
            {
                var fileByte = client.GetStreamAsync(new Uri(attachment.ProxyUrl)).Result;
                MemoryStream ms = new MemoryStream();
                fileByte.CopyTo(ms);
                msgData.Add(new ByteArrayContent(ms.ToArray()), "Photo", attachment.Filename);
            }

            var response = await client.PostAsync(MainData.publicPdaWebhook, msgData);

            await FollowupAsync("Отправлено", ephemeral: true);

            string responseContent = await response.Content.ReadAsStringAsync();
            ulong webHookMsgId = JObject.Parse(responseContent)["id"].Value<ulong>();

            await MainData.logger.LogAsync(new LogMessage(LogSeverity.Info, "Anon", Context.User.GlobalName.ToString() + ": Отправил анонимное сообщение"));

            var pdaEmbedLog = new EmbedBuilder()
                .WithTitle("Анонимное сообщение в кпк")
                .WithDescription(Format.Code(message))
                .AddField("Автор сообщения", MentionUtils.MentionUser(Context.User.Id))
                .AddField("Ссылка на сообщение", (Context.Guild.GetChannel(MainData.configData.PublicPDAChannelId) as SocketTextChannel).GetMessageAsync(webHookMsgId).Result.GetJumpUrl())
                .WithCurrentTimestamp();

            var logChannel = Context.Guild.GetTextChannel(MainData.configData.PDALogsChannelId);
            await logChannel.SendMessageAsync(embed: pdaEmbedLog.Build());

            client.Dispose();
            msgData.Dispose();
        }
    }
}