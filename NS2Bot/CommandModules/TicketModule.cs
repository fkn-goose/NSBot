using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;

namespace NS2Bot.CommandModules
{
    public class TicketModule : InteractionModuleBase<SocketInteractionContext>
    {

        [SlashCommand("createticketmenu", "Создает меню тикетов в данном канале")]
        [RequireOwner]
        public async Task CreateTicketMenu()
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Здесь вы можете создать тикет с обращением к администрации")
                .WithColor(Color.Green);

            var helperButton = new ButtonBuilder()
            {
                CustomId = "createHelperModal",
                Label = "Создать обращение к хелперу",
                Style = ButtonStyle.Primary
            };

            var curatorButton = new ButtonBuilder()
            {
                CustomId = "createCuratorModal",
                Label = "Создать обращение к куратору",
                Style = ButtonStyle.Primary
            };

            var adminsButton = new ButtonBuilder()
            {
                CustomId = "createAdminModal",
                Label = "Создать обращение к высшей администрации",
                Style = ButtonStyle.Primary
            };

            var component = new ComponentBuilder();
            component.WithButton(helperButton);
            component.WithButton(curatorButton);
            component.WithButton(adminsButton);

            await RespondAsync(embed: embedBuilder.Build(), components: component.Build());
        }

        [ComponentInteraction("createHelperModal")]
        public async Task HelperModal()
        {
            var modular = new ModalBuilder()
                .WithTitle("Создание обращение к хелперу")
                .WithCustomId("createHelperTicket")
                .AddTextInput("Подробно опишите суть обращения", "reason", TextInputStyle.Paragraph, "Cменить позывной, помочь с подключением к серверу и т.д.");

            await RespondWithModalAsync(modular.Build());
        }

        [ComponentInteraction("createCuratorModal")]
        public async Task CuratorModal()
        {
            var modular = new ModalBuilder()
                .WithTitle("Создание обращение к курации")
                .WithCustomId("createCuratorTicket")
                .AddTextInput("Подробно опишите суть обращения", "reason", TextInputStyle.Paragraph, "Выдача доната, разбор ситуации, жалобы на игроков и т.д.");

            await RespondWithModalAsync(modular.Build());
        }

        [ComponentInteraction("createAdminModal")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AdminModal()
        {
            var modular = new ModalBuilder()
                .WithTitle("Создание обращение к администрации")
                .WithCustomId("createAdminTicket")
                .AddTextInput("Подробно опишите суть обращения", "reason", TextInputStyle.Paragraph, "Жалоба на куратора/хелпера, вопросы по поводу занятия ГП, частные случаи и т.д.");

            await RespondWithModalAsync(modular.Build());
        }

        [ComponentInteraction("takeTicket")]
        public async Task HelperTakeTicket()
        {
            var newMessage = (SocketMessageComponent)Context.Interaction;
            var oldMessageId = ((IComponentInteraction)Context.Interaction).Message.Id;
            var channel = Context.Guild.GetTextChannel(MainData.configData.MessageChannelTickerPair[oldMessageId]);

            await channel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow));
            await channel.SendMessageAsync($"{MentionUtils.MentionUser(Context.User.Id)} взял тикет в работу");

            var embed = newMessage.Message.Embeds.First();
            var newEmbed = embed.ToEmbedBuilder();
            newEmbed.Title = embed.Title.Replace("[Открыто]", "[В работе]");
            newEmbed.AddField("Взял в работу", $"{MentionUtils.MentionUser(Context.User.Id)}");
            newEmbed.Color = Color.Orange;

            await newMessage.UpdateAsync(msg => { msg.Embeds = new Embed[] { newEmbed.Build() }; msg.Components = new ComponentBuilder().Build(); })
                .ContinueWith(task =>
                {
                    //Добавляю ключ сообщения "Тикет в работе" к каналу тикета 
                    MainData.configData.MessageChannelTickerPair.Add(newMessage.Id, channel.Id);
                });
        }

        [ComponentInteraction("closeHelperTicket")]
        public async Task CloseHelperTicket()
        {
            var channel = Context.Guild.GetTextChannel(Context.Channel.Id);
            await channel.ModifyAsync(prop => prop.CategoryId = MainData.configData.Category.OldTicketsCategoryId);
            await channel.SyncPermissionsAsync();

            var origMessage = (SocketMessageComponent)Context.Interaction;
            var newEmbed = origMessage.Message.Embeds.First().ToEmbedBuilder();
            newEmbed.Color = Color.Green;
            await origMessage.UpdateAsync(msg => { msg.Embeds = new Embed[] { newEmbed.Build() }; msg.Components = new ComponentBuilder().Build(); });

            var keys = MainData.configData.MessageChannelTickerPair.Where(v => v.Value == channel.Id).Select(k => k.Key).ToList();

            var helperTicketChannel = Context.Guild.GetTextChannel(MainData.configData.Category.HelperTicketsChannelId);
            List<IMessage> oldMessages = new List<IMessage>();
            foreach (var key in keys)
            {
                var msg = helperTicketChannel.GetMessageAsync(key).Result;
                if (msg != null)
                    oldMessages.Add(msg);

                //Удаляю запись открытого тикета
                MainData.configData.MessageChannelTickerPair.Remove(key);
            }

            await helperTicketChannel.DeleteMessagesAsync(oldMessages);
        }
    }
}
