using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Enums;
using NS2Bot.Extensions;
using static NS2Bot.Extensions.ModalExtensions;

namespace NS.Bot.App.Commands
{
    public class TicketModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ITicketService _ticketService;
        private readonly IGuildService _guildService;

        public TicketModule(ITicketService ticketService, IGuildService guildService)
        {
            _ticketService = ticketService;
            _guildService = guildService;
        }

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

        #region Settings commands

        [SlashCommand("setsettings", "Задать настройки тикетов")]
        [RequireOwner]
        public async Task SetTicketLogChannel([Summary(name: "Категория_хелперов", description: "Категория хелперов")] ICategoryChannel? newHelperCategory, [Summary(name: "Канал_тикетов_хелперов", description: "Канал тикетов хелперов")] ITextChannel? newHelperChannel,
                                              [Summary(name: "Старая_категория_хелперов", description: "Старая категория хелперов")] ICategoryChannel? oldHelperCategory,
                                              [Summary(name: "Категория_тикетов_кураторов", description: "Категория тикетов кураторов")] ICategoryChannel? newCuratorCategory, [Summary(name: "Канал_тикетов_кураторов", description: "Канал тикетов кураторов")] ITextChannel? newCuraturChannel,
                                              [Summary(name: "Старая_категория_кураторов", description: "Старая категория кураторов")] ICategoryChannel? oldCuratorCategory,
                                              [Summary(name: "Категория_тикетов_админов", description: "Категория тикетов админов")] ICategoryChannel? newAdminCategory, [Summary(name: "Канал_тикетов_админов", description: "Канал тикетов админов")] ITextChannel? newAdminChannel,
                                              [Summary(name: "Старая_категория_админов", description: "Старая категория админов")] ICategoryChannel? oldAdminCategory,
                                              [Summary(name: "Канал_для_логов_тикетов", description: "Канал для логов тикетов")] ITextChannel? logChannel)
        {
            var curSettings = _ticketService.GetSettingsByGuildId(Context.Guild.Id).Result;
            curSettings ??= new TicketSettings();

            curSettings.NewHelperTicketsCategoryId = newHelperCategory != null ? newHelperCategory.Id : curSettings.NewHelperTicketsCategoryId;
            curSettings.NewHelperTicketsChannelId = newHelperChannel != null ? newHelperChannel.Id : curSettings.NewHelperTicketsChannelId;
            curSettings.OldHelperTicketsCategory = oldHelperCategory != null ? oldHelperCategory.Id : curSettings.OldHelperTicketsCategory;
            curSettings.NewCuratorTicketsCategoryId = newCuratorCategory != null ? newCuratorCategory.Id : curSettings.NewCuratorTicketsCategoryId;
            curSettings.NewCuratorTicketsChannelId = newCuraturChannel != null ? newCuraturChannel.Id : curSettings.NewCuratorTicketsChannelId;
            curSettings.OldCuratorTicketsCategory = oldCuratorCategory != null ? oldCuratorCategory.Id : curSettings.OldCuratorTicketsCategory;
            curSettings.NewAdminTicketsCategoryId = newAdminCategory != null ? newAdminCategory.Id : curSettings.NewAdminTicketsCategoryId;
            curSettings.NewAdminTicketsChannelId = newAdminChannel != null ? newAdminChannel.Id : curSettings.NewAdminTicketsChannelId;
            curSettings.OldAdminTicketsCategory = oldAdminCategory != null ? oldAdminCategory.Id : curSettings.OldAdminTicketsCategory;
            curSettings.TicketLogs = logChannel != null ? logChannel.Id : curSettings.TicketLogs;

            _ticketService.Update(curSettings);

            await RespondAsync("Настройки успешно сохранены", ephemeral: true);
        }

        #endregion

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

        #region ModalMethods
        //ЕБУЧЕЕ МОДАЛЬНОЕ ОКНО НЕ УМЕЕТ ДАУНКАСТИТЬ КЛАССЫ, ПОЭТОМУ КОСТЫЛИМ

        [ModalInteraction("createHelperTicket")]
        public async Task CreateHelperTicket(HelperTicketModal modal)
        {
            await DeferAsync(ephemeral: true);
            await CreateTicket(new BaseTicket(modal));
        }

        [ModalInteraction("createCuratorTicket")]
        public async Task CreateCuratorTicket(CuratorTicketModal modal)
        {
            await DeferAsync(ephemeral: true);
            await CreateTicket(new BaseTicket(modal));
        }

        [ModalInteraction("createAdminTicket")]
        public async Task CreateAdminTicket(AdminTicketModal modal)
        {
            await DeferAsync(ephemeral: true);
            await CreateTicket(new BaseTicket(modal));
        }

        #endregion

        #region Ticket Commands

        private async Task CreateTicket(BaseTicket modal)
        {
            //Создаем пустые переменные для заполнения в зависимости от типа тикета
            uint ticketCount = 0;
            ulong ticketCategoryId = 0;
            ulong ticketChannelId = 0;
            string ticketType = modal.TicketType.GetDescription();
            var ticketSettings = _ticketService.GetSettingsByGuildId(Context.Guild.Id).Result;
            if (ticketSettings == null)
            {
                await FollowupAsync("Не найдены настройки тикетов! Сообщите об этой ошибке администрации сервера", ephemeral: true);
                return;
            }

            switch (modal.TicketType)
            {
                case TicketTypeEnum.Helper:
                    ticketCount = ticketSettings.HelperTicketsCount;
                    ticketCategoryId = ticketSettings.NewHelperTicketsCategoryId;
                    ticketChannelId = ticketSettings.NewHelperTicketsChannelId;
                    ticketSettings.HelperTicketsCount++;
                    _ticketService.Update(ticketSettings);
                    break;

                case TicketTypeEnum.Curator:
                    ticketCount = ticketSettings.CuratorTicketsCount;
                    ticketCategoryId = ticketSettings.NewCuratorTicketsCategoryId;
                    ticketChannelId = ticketSettings.NewCuratorTicketsChannelId;
                    ticketSettings.CuratorTicketsCount++;
                    _ticketService.Update(ticketSettings);
                    break;

                case TicketTypeEnum.Admin:
                    ticketCount = ticketSettings.AdminTicketsCount;
                    ticketCategoryId = ticketSettings.NewAdminTicketsCategoryId;
                    ticketChannelId = ticketSettings.NewAdminTicketsChannelId;
                    ticketSettings.AdminTicketsCount++;
                    _ticketService.Update(ticketSettings);
                    break;
            }

            var ticketData = new TicketEntity()
            {
                Guild = _guildService.GetByDiscordId(Context.Guild.Id),
                ChannelId = ticketChannelId,
                DeleteTime = null,
                Type = modal.TicketType
            };

            //Создаем канал-тикет
            var ticketChannel = await Context.Guild.CreateTextChannelAsync($"{ticketType}-тикет-{ticketCount}", prop => prop.CategoryId = ticketCategoryId);
            await ticketChannel.SyncPermissionsAsync();
            await ticketChannel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow));

            //Создаем кнопку по которой пользователь может закрыть тикет
            var embedChannelTicket = new EmbedBuilder().WithTitle($"Тикет #{ticketCount}")
                                                       .WithDescription(modal.Reason)
                                                       .WithColor(Color.Blue);
            var closeTicketComponent = new ComponentBuilder().WithButton(new ButtonBuilder()
            {
                CustomId = "closeTicket",
                Label = "Закрыть обращение",
                Style = ButtonStyle.Primary
            });
            await ticketChannel.SendMessageAsync(embed: embedChannelTicket.Build(), components: closeTicketComponent.Build());

            //Создаем кнопку по которой администратор может взять тикет в работу
            var newTicketEmbed = new EmbedBuilder()
                .WithTitle($"Тикет #{ticketCount} [Открыто]")
                .WithDescription($"Обращение создал - {MentionUtils.MentionUser(Context.User.Id)} ({Context.User.Username})")
                .WithCurrentTimestamp()
                .WithColor(Color.LightGrey);
            var takeTicketComponent = new ComponentBuilder().WithButton(new ButtonBuilder()
            {
                CustomId = "takeTicket",
                Label = "Взять тикет в работу",
                Style = ButtonStyle.Primary
            });
            var ticketMenu = Context.Guild.GetTextChannel(ticketChannelId);
            var takeTicketButtonMessage = await ticketMenu.SendMessageAsync(embed: newTicketEmbed.Build(), components: takeTicketComponent.Build());
            ticketData.MessageId = takeTicketButtonMessage.Id;

            _ticketService.Create(ticketData);
            await Context.Guild.GetTextChannel(ticketSettings.TicketLogs)
                .SendMessageAsync($"{MentionUtils.MentionUser(Context.User.Id)} ({Context.User.Username}) создал тикет {MentionUtils.MentionChannel(ticketChannel.Id)} ({Context.Guild.GetChannel(ticketChannel.Id).Name})");

            //Оповещаем пользователя о создании тикета и заполняем остальные данные
            await FollowupAsync($"Создан тикет - {MentionUtils.MentionChannel(ticketChannel.Id)}. Как только появится свободный {ticketType}, он возьмет его в работу.", ephemeral: true);
        }

        [ComponentInteraction("takeTicket")]
        public async Task TakeTicket()
        {
            await DeferAsync(ephemeral: true);

            var ticket = _ticketService.GetByMessageId(((SocketMessageComponent)Context.Interaction).Message.Id).Result;
            if (ticket == null)
            {
                await FollowupAsync("Ошибка, тикет не существует. Сообщите об этой ошибке администрации сервера", ephemeral: true);
                return;
            }

            var ticketSettings = _ticketService.GetSettingsByGuildId(Context.Guild.Id).Result;
            if (ticketSettings == null)
            {
                await FollowupAsync("Не найдены настройки тикетов! Сообщите об этой ошибке администрации сервера", ephemeral: true);
                return;
            }

            var ticketChannel = Context.Guild.GetTextChannel(ticket.ChannelId);
            var ticketMessage = ticketChannel.GetMessageAsync(ticket.MessageId).Result as SocketMessageComponent;

            if(ticketMessage == null)
            {
                await FollowupAsync("Не найдено сообщение! Сообщите об этой ошибке администрации сервера", ephemeral: true);
                return;
            }

            await ticketChannel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow));
            await ticketChannel.SendMessageAsync($"{MentionUtils.MentionUser(Context.User.Id)} взял тикет в работу");

            var ticketEmbed = ticketMessage.Message.Embeds.First();
            var ticketEmbedUpdated = ticketEmbed.ToEmbedBuilder();
            ticketEmbedUpdated.Title = ticketEmbed.Title.Replace("[Открыто]", "[В работе]");
            ticketEmbedUpdated.AddField("Взял в работу", $"{MentionUtils.MentionUser(Context.User.Id)} ({Context.User.Username})");
            ticketEmbedUpdated.Color = Color.Orange;

            await ticketMessage.UpdateAsync(msg => { msg.Embeds = new Embed[] { ticketEmbedUpdated.Build() }; msg.Components = new ComponentBuilder().Build(); });
            ticket.MessageId = ticketMessage.Id;
            _ticketService.Update(ticket);

            await Context.Guild.GetTextChannel(ticketSettings.TicketLogs)
                .SendMessageAsync($"{MentionUtils.MentionUser(Context.User.Id)} ({Context.User.Username}) взял в работу тикет {MentionUtils.MentionChannel(ticketChannel.Id)} ({Context.Guild.GetChannel(ticketChannel.Id).Name})");

            await FollowupAsync($"Тикет {MentionUtils.MentionChannel(ticketChannel.Id)} взят в работу.", ephemeral: true);
        }

        [ComponentInteraction("closeTicket")]
        public async Task CloseTicket()
        {
            await DeferAsync(ephemeral: true);


            var ticket = _ticketService.GetByChannelId(Context.Channel.Id).Result;
            if (ticket == null)
            {
                await FollowupAsync("Ошибка, тикет не существует. Сообщите об этой ошибке администрации сервера", ephemeral: true);
                return;
            }

            var ticketSettings = _ticketService.GetSettingsByGuildId(Context.Guild.Id).Result;
            if (ticketSettings == null)
            {
                await FollowupAsync("Не найдены настройки тикетов! Сообщите об этой ошибке администрации сервера", ephemeral: true);
                return;
            }

            var ticketChannel = Context.Guild.GetTextChannel(Context.Channel.Id);

            switch (ticket.Type)
            {
                case TicketTypeEnum.Helper:
                    await ticketChannel.ModifyAsync(prop => prop.CategoryId = ticketSettings.OldHelperTicketsCategory);
                    break;

                case TicketTypeEnum.Curator:
                    await ticketChannel.ModifyAsync(prop => prop.CategoryId = ticketSettings.OldCuratorTicketsCategory);
                    break;

                case TicketTypeEnum.Admin:
                    await ticketChannel.ModifyAsync(prop => prop.CategoryId = ticketSettings.OldAdminTicketsCategory);
                    break;

                default:
                    await FollowupAsync("Ошибка, тип тикета не найден");
                    return;
            }

            await ticketChannel.SyncPermissionsAsync();

            ticket.DeleteTime = DateTime.Now.AddDays(3);
            ticket.IsFinished = true;
            _ticketService.Update(ticket);

            var closeTicketMessage = (SocketMessageComponent)Context.Interaction;
            var closeTicketEmbed = closeTicketMessage.Message.Embeds.First().ToEmbedBuilder();
            closeTicketEmbed.Color = Color.Green;
            await closeTicketMessage.UpdateAsync(msg => { msg.Embeds = new Embed[] { closeTicketEmbed.Build() }; msg.Components = new ComponentBuilder().Build(); });

            var ticketStatusMessage = ticketChannel.GetMessageAsync(ticket.MessageId).Result;

            var embed = ticketStatusMessage?.Embeds.First();
            var newEmbed = embed.ToEmbedBuilder();
            newEmbed.Title = embed?.Title.Replace("[В работе]", "[Закрыто]");
            newEmbed.AddField("Описание проблемы", closeTicketEmbed.Description);
            newEmbed.AddField("Закрыл ", $"{MentionUtils.MentionUser(Context.User.Id)} ({Context.User.Username})");
            newEmbed.Color = Color.Green;

            await Context.Guild.GetTextChannel(ticketSettings.TicketLogs).SendMessageAsync(embed: newEmbed.Build());
            await FollowupAsync("Тикет закрыт", ephemeral: true);
        }

        #endregion
    }
}
