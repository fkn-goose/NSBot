using Discord;
using Discord.Interactions;
using Discord.Webhook;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using NS2Bot.Models;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Timers;

namespace NS2Bot.CommandModules
{
    public class PDAModule : InteractionModuleBase<SocketInteractionContext>
    {
        private const string BotUserName = "Аноним";
        private const string BotAvatar = "https://media.moddb.com/cache/images/downloads/1/221/220278/thumb_620x2000/No_data.png";

        [SlashCommand("setpdachannel", "Установить общий-кпк канал")]
        [RequireOwner]
        public async Task SetPublicPDAChannel()
        {
            MainData.configData.PublicPDAChannelId = Context.Channel.Id;
            await RespondAsync("Канал установлен как \"Общий-кпк\"", ephemeral: true);
        }

        [SlashCommand("anonymous", "Отправить анонимное сообщение")]
        public async Task AnonymousMessage([Summary(name: "Сообщение", description: "Текст сообщения")] string message, [Summary(name: "Изображение", description: "Прикрепить изображение")][Optional][DefaultParameterValue(null)] IAttachment? attachment)
        {
            HttpClient client = new HttpClient();

            DiscordWebhookClient pdaWebHook = new DiscordWebhookClient(MainData.publicPdaWebhook);
            pdaWebHook.Log += _ => MainData.logger.LogAsync(_);

            await DeferAsync(ephemeral:true);

            message = message.Replace(@"\n", "\n");
            Task<ulong> messageId;

            if (attachment != null && attachment.Width != null)
                messageId = pdaWebHook.SendFileAsync(stream: client.GetStreamAsync(new Uri(attachment.ProxyUrl)).Result, filename: attachment.Filename, text: message, username: BotUserName, avatarUrl: BotAvatar);
            else
                messageId = pdaWebHook.SendMessageAsync(message, username: BotUserName, avatarUrl: BotAvatar);

            await FollowupAsync("Отправлено", ephemeral: true);

            var pdaEmbedLog = new EmbedBuilder()
                .WithTitle("Анонимное сообщение в кпк")
                .WithDescription(Format.Code(message))
                .AddField("Автор сообщения", MentionUtils.MentionUser(Context.User.Id))
                .AddField("Ссылка на сообщение", (Context.Guild.GetChannel(MainData.configData.PublicPDAChannelId) as SocketTextChannel).GetMessageAsync(messageId.Result).Result.GetJumpUrl())
                .WithCurrentTimestamp();

            var logChannel = Context.Guild.GetTextChannel(MainData.configData.PDALogsChannelId);
            await logChannel.SendMessageAsync(embed: pdaEmbedLog.Build());

            client.Dispose();
            pdaWebHook.Dispose();
        }
    }
}