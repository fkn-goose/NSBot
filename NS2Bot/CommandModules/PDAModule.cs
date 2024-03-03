using Discord;
using Discord.Interactions;
using NS2Bot.Models;
using System.Text;
using System.Text.Json;

namespace NS2Bot.CommandModules
{
    public class PDAModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("anonymous", "Отправить анонимное сообщение")]
        public async Task AnonymousMessage([Summary(name: "Имя", description: "Имя")] string name, [Summary(name: "Сообщение", description: "Текст сообщения")] string message)
        {
            HttpClient client = new HttpClient();

            WebhookModel webhookModel = new WebhookModel();
            webhookModel.avatar_url = "https://media.moddb.com/cache/images/downloads/1/221/220278/thumb_620x2000/No_data.png";
            webhookModel.content = message;
            webhookModel.username = name;
            string json = JsonSerializer.Serialize(webhookModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(MainData.publicPdaWebhook, content);
            string responseContent = await response.Content.ReadAsStringAsync();
            MainData.logger.Log(new LogMessage(LogSeverity.Info, "Anon", Context.User.GlobalName.ToString() + ':'+ message));

            await RespondAsync("Отправлено", ephemeral: true);
        }
    }
}