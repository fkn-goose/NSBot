using Discord;
using Discord.Interactions;
using Discord.WebSocket;

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

            await Context.Channel.SendMessageAsync(embed: embedBuilder.Build(), components: component.Build());
            await RespondAsync("Меню успешно создано", ephemeral: true);
        }

        [ComponentInteraction("createHelperModal")]
        public async Task HelperModal()
        {
            await RespondWithModalAsync<HelperTicketModal>("createHelperTicket");
        }

        [ComponentInteraction("createCuratorModal")]
        public async Task CuratorModal()
        {
            await RespondWithModalAsync<CuratorTicketModal>("createCuratorTicket");
        }

        [ComponentInteraction("createAdminModal")]
        public async Task AdminModal()
        {
            await RespondWithModalAsync<AdminTicketModal>("createAdminTicket");
        }

        #region HelperMethods

        [ModalInteraction("createHelperTicket")]
        public async Task CreateHelperTicketModal(HelperTicketModal modal)
        {
            await DeferAsync(ephemeral: true);

            var ticketChannel = await Context.Guild.CreateTextChannelAsync($"Хелпер-тикет-{Model.Data.Helper.TicketsCount}", prop => prop.CategoryId = Model.Data.Helper.NewTicketsCategoryId);

            await ticketChannel.SyncPermissionsAsync();
            await ticketChannel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow));

            var embedChannelTicket = new EmbedBuilder()
               .WithTitle($"Тикет #{Model.Data.Helper.TicketsCount}")
               .WithDescription(modal.Reason)
               .WithColor(Color.Blue);

            var closeTicketButton = new ComponentBuilder();
            closeTicketButton.WithButton(new ButtonBuilder()
            {
                CustomId = "closeHelperTicket",
                Label = "Закрыть обращение",
                Style = ButtonStyle.Primary
            });
            await ticketChannel.SendMessageAsync(embed: embedChannelTicket.Build(), components: closeTicketButton.Build());

            var newTicketEmbed = new EmbedBuilder()
                .WithTitle($"Тикет #{Model.Data.Helper.TicketsCount} [Открыто]")
                .WithDescription($"Обращение создал - {MentionUtils.MentionUser(Context.User.Id)}")
                .WithCurrentTimestamp()
                .WithColor(Color.LightGrey);

            var takeTicketButton = new ButtonBuilder()
            {
                CustomId = "takeHelperTicket",
                Label = "Взять тикет в работу",
                Style = ButtonStyle.Primary
            };

            closeTicketButton = new ComponentBuilder();
            closeTicketButton.WithButton(takeTicketButton);

            var ticketMenu = Context.Guild.GetTextChannel(Model.Data.Helper.NewTicketsChannelId);
            var helperButton = await ticketMenu.SendMessageAsync(embed: newTicketEmbed.Build(), components: closeTicketButton.Build());

            await FollowupAsync($"Создан тикет - {MentionUtils.MentionChannel(ticketChannel.Id)}. Как только появится свободный хелпер, он возьмет его в работу.", ephemeral: true)
                .ContinueWith(task =>
                {
                    if (Model.Data.Helper.MessageTitcketPair == null)
                        Model.Data.Helper.MessageTitcketPair = new Dictionary<ulong, ulong>();

                    //Добавляю ключ "Открытый тикет" к каналу тикета
                    Model.Data.Helper.MessageTitcketPair.Add(helperButton.Id, ticketChannel.Id);
                });

            Model.Data.Helper.TicketsCount++;
        }

        [ComponentInteraction("takeHelperTicket")]
        public async Task HelperTakeTicket()
        {
            var newMessage = (SocketMessageComponent)Context.Interaction;
            var messageOldId = newMessage.Message.Id;
            var ticketChannel = Context.Guild.GetTextChannel(Model.Data.Helper.MessageTitcketPair[messageOldId]);

            await ticketChannel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow));
            await ticketChannel.SendMessageAsync($"{MentionUtils.MentionUser(Context.User.Id)} взял тикет в работу");

            var embed = newMessage.Message.Embeds.First();
            var newEmbed = embed.ToEmbedBuilder();
            newEmbed.Title = embed.Title.Replace("[Открыто]", "[В работе]");
            newEmbed.AddField("Взял в работу", $"{MentionUtils.MentionUser(Context.User.Id)}");
            newEmbed.Color = Color.Orange;

            await newMessage.UpdateAsync(msg => { msg.Embeds = new Embed[] { newEmbed.Build() }; msg.Components = new ComponentBuilder().Build(); });
        }

        [ComponentInteraction("closeHelperTicket")]
        public async Task CloseHelperTicket()
        {
            var currentTicketChannel = Context.Guild.GetTextChannel(Context.Channel.Id);
            await currentTicketChannel.ModifyAsync(prop => prop.CategoryId = Model.Data.Helper.OldTicketsCategory);
            await currentTicketChannel.SyncPermissionsAsync();
            Model.Data.Helper.OldTickets.Add(DateTime.Now.AddMinutes(2), currentTicketChannel.Id);

            var closeTicketMessage = (SocketMessageComponent)Context.Interaction;
            var closeTicketEmbed = closeTicketMessage.Message.Embeds.First().ToEmbedBuilder();
            closeTicketEmbed.Color = Color.Green;
            await closeTicketMessage.UpdateAsync(msg => { msg.Embeds = new Embed[] { closeTicketEmbed.Build() }; msg.Components = new ComponentBuilder().Build(); });

            var ticketStatusMessageId = Model.Data.Helper.MessageTitcketPair.Where(x=>x.Value == Context.Channel.Id).FirstOrDefault().Key;
            if (ticketStatusMessageId == 0)
                return;

            var helperTicketChannel = Context.Guild.GetTextChannel(Model.Data.Helper.NewTicketsChannelId);
            var ticketStatusMessage = helperTicketChannel.GetMessageAsync(ticketStatusMessageId).Result;

            var embed = ticketStatusMessage?.Embeds.First();
            var newEmbed = embed.ToEmbedBuilder();
            newEmbed.Title = embed?.Title.Replace("[В работе]", "[Закрыто]");
            newEmbed.AddField("Описание проблемы", closeTicketEmbed.Description);
            newEmbed.AddField("Закрыл ", $"{MentionUtils.MentionUser(Context.User.Id)}");
            newEmbed.Color = Color.Green;

            await Context.Guild.GetTextChannel(Model.Data.Helper.OldTicketsChannelId).SendMessageAsync(embed: newEmbed.Build())
                .ContinueWith(task =>
                {
                    helperTicketChannel.DeleteMessageAsync(ticketStatusMessageId).Wait();
                    Model.Data.Helper.MessageTitcketPair.Remove(ticketStatusMessageId);
                });
        }

        #endregion

        #region TicketModels

        public class HelperTicketModal : IModal
        {
            public string Title => "Создание обращения к хелперу";

            [InputLabel("Подробно опишите суть обращения")]
            [ModalTextInput("reason", TextInputStyle.Paragraph, placeholder: "Помочь с подключением к серверу, проведение собеседования, квенте т.д.", maxLength: 200)]
            public string Reason { get; set; }
        }

        public class CuratorTicketModal : IModal
        {
            public string Title => "Создание обращения к курации";

            [InputLabel("Подробно опишите суть обращения")]
            [ModalTextInput("reason", TextInputStyle.Paragraph, placeholder: "Выдача/восстановление доната и вещей, вопрос по правилам, разбор ситуации и т.д.", maxLength: 200)]
            public string Reason { get; set; }
        }

        public class AdminTicketModal : IModal
        {
            public string Title => "Создание обращения к администрации";

            [InputLabel("Подробно опишите суть обращения")]
            [ModalTextInput("reason", TextInputStyle.Paragraph, placeholder: "Вопросы по поводу занятия ГП, жалобы на курацию, частные случаи и т.д.", maxLength: 200)]
            public string Reason { get; set; }
        }

        #endregion
    }
}
