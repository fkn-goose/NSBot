using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using NS2Bot.Models;
using System.Text;
using System.Text.Json;
using System.Timers;

namespace NS2Bot.CommandModules
{
    public class PDAModule : InteractionModuleBase<SocketInteractionContext>
    {
        List<SocketMessage> webhookMessages = new List<SocketMessage>();

        [SlashCommand("setpdachannel", "Установить общий-кпк канал")]
        [RequireOwner]
        public async Task SetPublicPDAChannel()
        {
            MainData.configData.PublicPDAChannelId = Context.Channel.Id;
            await RespondAsync("Канал установлен как \"Общий-кпк\"", ephemeral: true);
        }

        [SlashCommand("anonymous", "Отправить анонимное сообщение")]
        public async Task AnonymousMessage([Summary(name: "Сообщение", description: "Текст сообщения")] string message)
        {
            webhookMessages = new List<SocketMessage>();
            HttpClient client = new HttpClient();
            
            WebhookModel webhookModel = new()
            {
                username = "Аноним",
                avatar_url = "https://media.moddb.com/cache/images/downloads/1/221/220278/thumb_620x2000/No_data.png"
            };

            message = message.Replace(@"\n", "\n");
            webhookModel.content = message;
            List<SocketMessage> messages = new List<SocketMessage>();

            Context.Client.MessageReceived += WebhookPDAMessages;

            string json = JsonSerializer.Serialize(webhookModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(MainData.publicPdaWebhook, content);
            string responseContent = await response.Content.ReadAsStringAsync();

            System.Timers.Timer webhookWaiter = new (5000);
            webhookWaiter.AutoReset = false;
            webhookWaiter.Elapsed += new ElapsedEventHandler((sender, e) => SendLog(sender, e, message, responseContent));
            webhookWaiter.Start();

            await RespondAsync("Отправлено", ephemeral: true);
        }

        private async void SendLog(object? sender, ElapsedEventArgs e, string message, string responseContent)
        {
            Context.Client.MessageReceived -= WebhookPDAMessages;

            var webhookMessageUrl = webhookMessages.FirstOrDefault()?.GetJumpUrl();

            var pdaEmbedLog = new EmbedBuilder()
                .WithTitle("Анонимное сообщение в кпк")
                .WithDescription(Format.Code(message))
                .AddField("Автор сообщения", MentionUtils.MentionUser(Context.User.Id))
                .WithCurrentTimestamp();

            if (!string.IsNullOrEmpty(webhookMessageUrl))
                pdaEmbedLog.AddField("Ссылка на сообщение", webhookMessageUrl);

            var logChannel = Context.Guild.GetTextChannel(MainData.configData.PDALogsChannelId);
            await logChannel.SendMessageAsync(embed: pdaEmbedLog.Build());

            await MainData.logger.LogAsync(new LogMessage(LogSeverity.Info, "Anon", Context.User.GlobalName.ToString() + ':' + message));
            await MainData.logger.LogAsync(new LogMessage(LogSeverity.Info, "Anon", responseContent));
        }

        private async Task WebhookPDAMessages(SocketMessage message)
        {
            if (!message.Author.IsWebhook)
                return;

            if (MainData.configData.PublicPDAChannelId == 0 || message.Channel.Id != MainData.configData.PublicPDAChannelId)
                return;

            webhookMessages.Add(message);
            await Task.CompletedTask;
        }
    }
}