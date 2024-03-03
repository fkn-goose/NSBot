using Discord;
using Discord.Commands;
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

        [SlashCommand("anonymous", "Отправить анонимное сообщение")]
        [Alias("anon", "an")]
        public async Task AnonymousMessage([Discord.Interactions.Summary(name: "Сообщение", description: "Текст сообщения")] string message)
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

            Context.Client.MessageReceived += WebhookMessages;

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
            Context.Client.MessageReceived -= WebhookMessages;

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

        private async Task WebhookMessages(SocketMessage message)
        {
            if (!message.Author.IsWebhook)
                await Task.CompletedTask;

            webhookMessages.Add(message);
            await Task.CompletedTask;
        }
    }
}