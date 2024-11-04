using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Ticket;
using NS.Bot.Shared.Enums;
using NS.Bot.Shared.Extensions;
using NS.Bot.Shared.Models;
using System.Text.Json;
using static NS2Bot.Extensions.ModalExtensions;

namespace NS.Bot.App.Commands
{
    [RequireOwner]
    public class TicketModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IBaseTicketService<TicketBase> _ticketBaseService;
        private readonly IBaseTicketService<TicketNick> _ticketNickService;
        private readonly IGuildService _guildService;
        private readonly IGuildMemberService _guildMemberService;
        private readonly IMemberService _memberService;

        private readonly List<TicketSettingsModel> TicketSettings;
        private readonly IOptionsMonitor<string> SteamKey;

        public TicketModule(IBaseTicketService<TicketBase> ticketBaseService, IBaseTicketService<TicketNick> ticketNickService,
            IGuildService guildService, IGuildMemberService guildMemberService, IMemberService memberService, IOptionsMonitor<List<TicketSettingsModel>> ticketSettings, IOptionsMonitor<string> steamkey)
        {
            _ticketBaseService = ticketBaseService;
            _ticketNickService = ticketNickService;
            _guildService = guildService;
            _guildMemberService = guildMemberService;
            _memberService = memberService;

            TicketSettings = ticketSettings.CurrentValue;
            SteamKey = steamkey;
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

            var changeNickButton = new ButtonBuilder()
            {
                CustomId = "createChangeNickModal",
                Label = "Сменить позывной",
                Style = ButtonStyle.Primary
            };

            var curatorButton = new ButtonBuilder()
            {
                CustomId = "createCuratorModal",
                Label = "Создать обращение к куратору",
                Style = ButtonStyle.Success
            };

            var itemsButton = new ButtonBuilder()
            {
                CustomId = "createItemsModal",
                Label = "Получение/восстановление вещей",
                Style = ButtonStyle.Success
            };

            var bonusesButton = new ButtonBuilder()
            {
                CustomId = "createBonusModal",
                Label = "Получение/восстановление бонусов",
                Style = ButtonStyle.Success
            };

            var playerComplaintButton = new ButtonBuilder()
            {
                CustomId = "createPlayerComplaintModal",
                Label = "Подать жалобу на игрока",
                Style = ButtonStyle.Danger
            };

            var adminComplaintButton = new ButtonBuilder()
            {
                CustomId = "createAdminComplaintModal",
                Label = "Подать жалобу на администратора",
                Style = ButtonStyle.Danger
            };

            var adminsButton = new ButtonBuilder()
            {
                CustomId = "createAdminModal",
                Label = "Создать обращение к высшей администрации",
                Style = ButtonStyle.Danger
            };

            ComponentBuilder buttons = new ComponentBuilder();
            buttons.AddRow(new ActionRowBuilder().AddComponent(helperButton.Build()).AddComponent(changeNickButton.Build()));
            buttons.AddRow(new ActionRowBuilder().AddComponent(curatorButton.Build()).AddComponent(itemsButton.Build()).AddComponent(bonusesButton.Build()));
            buttons.AddRow(new ActionRowBuilder().AddComponent(playerComplaintButton.Build()).AddComponent(adminComplaintButton.Build()));
            buttons.AddRow(new ActionRowBuilder().AddComponent(adminsButton.Build()));

            await Context.Channel.SendMessageAsync(embed: embedBuilder.Build(), components: buttons.Build());
            await RespondAsync("Меню успешно создано", ephemeral: true);
        }

        [ComponentInteraction("createHelperModal")]
        public async Task HelperModal()
        {
            await RespondWithModalAsync<HelperTicketModal>("createHelperTicket");
        }

        [ComponentInteraction("createChangeNickModal")]
        public async Task NickModal()
        {
            await RespondWithModalAsync<NickTicketModal>("createNickTicket");
        }

        [ComponentInteraction("createCuratorModal")]
        public async Task CuratorModal()
        {
            await RespondWithModalAsync<CuratorTicketModal>("createCuratorTicket");
        }

        [ComponentInteraction("createItemsModal")]
        public async Task ItemsModal()
        {
            await RespondWithModalAsync<ItemsTicketModal>("createItemsTicket");
        }

        [ComponentInteraction("createBonusModal")]
        public async Task BonusModal()
        {
            await RespondWithModalAsync<BonusTicketModal>("createBonusTicket");
        }

        [ComponentInteraction("createPlayerComplaintModal")]
        public async Task PlayerComplaintModal()
        {
            await RespondWithModalAsync<ComplaintModal>("createPlayerComplaintTicket");
        }

        [ComponentInteraction("createAdminComplaintModal")]
        public async Task AdminComplaintModal()
        {
            await RespondWithModalAsync<ComplaintModal>("createAdminComplaintTicket");
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
            await CreateTicket(modal.TicketTypeEnum, modal.Reason);
        }

        [ModalInteraction("createNickTicket")]
        public async Task CreateNickTicket(NickTicketModal modal)
        {
            await DeferAsync(ephemeral: true);
            await CreateTicket(modal.TicketTypeEnum, modal.Reason, modal.Nick);
        }

        [ModalInteraction("createCuratorTicket")]
        public async Task CreateCuratorTicket(CuratorTicketModal modal)
        {
            await DeferAsync(ephemeral: true);
            await CreateTicket(modal.TicketTypeEnum, modal.Reason);
        }

        [ModalInteraction("createItemsTicket")]
        public async Task CreateItemsTicket(ItemsTicketModal modal)
        {
            await DeferAsync(ephemeral: true);
            await CreateTicket(modal.TicketTypeEnum, modal.Reason, modal.SteamId, modal.WhatLost, modal.When);
        }

        [ModalInteraction("createBonusTicket")]
        public async Task CreateBonusTicket(BonusTicketModal modal)
        {
            await DeferAsync(ephemeral: true);
            await CreateTicket(modal.TicketTypeEnum, modal.Reason, modal.SteamId, modal.WhatLost, modal.When);
        }

        [ModalInteraction("createPlayerComplaintTicket")]
        public async Task CreateComplaintTicket(ComplaintModal modal)
        {
            await DeferAsync(ephemeral: true);
            await CreateTicket(modal.TicketTypeEnum, modal.Reason, modal.When, modal.DiscordTag);
        }

        [ModalInteraction("createAdminComplaintTicket")]
        public async Task CreateAdminComplaintTicket(ComplaintModal modal)
        {
            await DeferAsync(ephemeral: true);
            await CreateTicket(modal.TicketTypeEnum, modal.Reason, modal.When, modal.DiscordTag);
        }

        [ModalInteraction("createAdminTicket")]
        public async Task CreateAdminTicket(AdminTicketModal modal)
        {
            await DeferAsync(ephemeral: true);
            await CreateTicket(modal.TicketTypeEnum, modal.Reason);
        }

        #endregion

        #region Ticket Commands
        //Хелпер, куратор, админтикеты
        private async Task CreateTicket(TicketTypeEnum ticketType, string reason)
        {
            if (TicketSettings == null || !TicketSettings.Any())
            {
                await FollowupAsync("Не найдены настройки тикетов! Сообщите об этой ошибке администрации сервера", ephemeral: true);
                return;
            }

            var currentSettings = TicketSettings.FirstOrDefault(x => x.RelatedGuildId == Context.Guild.Id);
            if (currentSettings == null)
            {
                await FollowupAsync("Не найдены настройки тикетов текущего сервера! Сообщите об этой ошибке администрации сервера", ephemeral: true);
                return;
            }

            var currentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            var createdBy = await _guildMemberService.GetByDiscordIdAsync(Context.User.Id, Context.Guild.Id);
            if (createdBy == null)
            {
                var member = await _memberService.GetByDiscordIdAsync(Context.User.Id);
                member ??= await _memberService.CreateMemberAsync(Context.User.Id, currentGuild);
                createdBy = await _guildMemberService.GetByMemberAsync(member, currentGuild);
            }

            if (createdBy == null)
            {
                await FollowupAsync("Не найден пользователь сервера", ephemeral: true);
                return;
            }

            TicketBase ticketBase = new TicketBase()
            {
                GuildId = currentGuild.Id,
                CreatedById = createdBy.Id,
                Type = ticketType,
                Reason = reason,
            };

            await _ticketBaseService.CreateOrUpdateAsync(ticketBase);
            var ids = new ulong[2];

            switch (ticketBase.Type)
            {
                case TicketTypeEnum.Helper:
                    ids = await CreateUserEmbed(currentSettings.HelperNewTicketsCategoryId, currentSettings.HelperNewTicketsChannelId, ticketBase.Type.GetDescription(), ticketBase.Id, ticketBase.Reason);
                    break;

                case TicketTypeEnum.Curator:
                    ids = await CreateUserEmbed(currentSettings.CuratorNewTicketsCategoryId, currentSettings.CuratorNewTicketsChannelId, ticketBase.Type.GetDescription(), ticketBase.Id, ticketBase.Reason);
                    break;

                case TicketTypeEnum.Admin:
                    ids = await CreateUserEmbed(currentSettings.AdminNewTicketsCategoryId, currentSettings.AdminNewTicketsChannelId, ticketBase.Type.GetDescription(), ticketBase.Id, ticketBase.Reason);
                    break;
            }

            ticketBase.MessageId = ids[0];
            ticketBase.ChannelId = ids[1];
            await _ticketBaseService.UpdateAsync(ticketBase);

            //Оповещаем пользователя о создании тикета и заполняем остальные данные
            await FollowupAsync($"Создан тикет - {MentionUtils.MentionChannel(ticketBase.ChannelId)}. Как только появится свободный администратор, он возьмет его в работу.", ephemeral: true);
        }
        //Тикет на позывной
        private async Task CreateTicket(TicketTypeEnum ticketType, string reason, string nick)
        {
            if (TicketSettings == null || !TicketSettings.Any())
            {
                await FollowupAsync("Не найдены настройки тикетов! Сообщите об этой ошибке администрации сервера", ephemeral: true);
                return;
            }

            var currentSettings = TicketSettings.FirstOrDefault(x => x.RelatedGuildId == Context.Guild.Id);
            if (currentSettings == null)
            {
                await FollowupAsync("Не найдены настройки тикетов текущего сервера! Сообщите об этой ошибке администрации сервера", ephemeral: true);
                return;
            }

            var currentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            var createdBy = await _guildMemberService.GetByDiscordIdAsync(Context.User.Id, Context.Guild.Id);
            if (createdBy == null)
            {
                var member = await _memberService.GetByDiscordIdAsync(Context.User.Id);
                member ??= await _memberService.CreateMemberAsync(Context.User.Id, currentGuild);
                createdBy = await _guildMemberService.GetByMemberAsync(member, currentGuild);
            }

            if (createdBy == null)
            {
                await FollowupAsync("Не найден пользователь сервера", ephemeral: true);
                return;
            }

            TicketNick ticketNick = new TicketNick()
            {
                GuildId = currentGuild.Id,
                CreatedById = createdBy.Id,
                Type = ticketType,
                Reason = reason,
                NewNick = nick
            };

            await _ticketBaseService.CreateOrUpdateAsync(ticketNick);
            var fullReason = $"Новый позывной - {nick}\nПричина:" + reason;

            var ids = await CreateUserEmbed(currentSettings.HelperNewTicketsCategoryId, currentSettings.NameChangeNewTicketChannel, ticketNick.Type.GetDescription(), ticketNick.Id, fullReason);

            ticketNick.MessageId = ids[0];
            ticketNick.ChannelId = ids[1];
            await _ticketBaseService.UpdateAsync(ticketNick);

            //Оповещаем пользователя о создании тикета и заполняем остальные данные
            await FollowupAsync($"Создан тикет - {MentionUtils.MentionChannel(ticketNick.ChannelId)}. Как только появится свободный администратор, он возьмет его в работу.", ephemeral: true);
        }
        //Восстановление вещей и бонусов
        private async Task CreateTicket(TicketTypeEnum ticketType, string reason, string steamId, string what, string when)
        {
            if (TicketSettings == null || !TicketSettings.Any())
            {
                await FollowupAsync("Не найдены настройки тикетов! Сообщите об этой ошибке администрации сервера", ephemeral: true);
                return;
            }

            var currentSettings = TicketSettings.FirstOrDefault(x => x.RelatedGuildId == Context.Guild.Id);
            if (currentSettings == null)
            {
                await FollowupAsync("Не найдены настройки тикетов текущего сервера! Сообщите об этой ошибке администрации сервера", ephemeral: true);
                return;
            }

            if(!(await ValidateSteamID(steamId)))
            {
                await FollowupAsync("SteamID не существует", ephemeral: true);
                return;
            }

            var currentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            var createdBy = await _guildMemberService.GetByDiscordIdAsync(Context.User.Id, Context.Guild.Id);
            if (createdBy == null)
            {
                var member = await _memberService.GetByDiscordIdAsync(Context.User.Id);
                member ??= await _memberService.CreateMemberAsync(Context.User.Id, currentGuild);
                createdBy = await _guildMemberService.GetByMemberAsync(member, currentGuild);
                if(string.IsNullOrEmpty(member.SteamId) || !string.Equals(member.SteamId, steamId))
                    await _memberService.UpdateAsync(member);
            }

            if (createdBy == null)
            {
                await FollowupAsync("Не найден пользователь сервера", ephemeral: true);
                return;
            }

            TicketItemBonusRestor ticketBonusItem = new TicketItemBonusRestor()
            {
                GuildId = currentGuild.Id,
                CreatedById = createdBy.Id,
                Type = ticketType,
                Reason = reason,
                SteamId = steamId,
                What = what,
                When = when,
            };

            await _ticketBaseService.CreateOrUpdateAsync(ticketBonusItem);
            var ids = await CreateUserEmbed(currentSettings.CuratorNewTicketsCategoryId, 
                ticketBonusItem.Type == TicketTypeEnum.BonusesRestor ? currentSettings.BonusesNewTicketChannel : currentSettings.ItemsNewTicketChannel, 
                ticketBonusItem.Type.GetDescription(), ticketBonusItem.Id, reason, what, when, steamId);

            if(ids == null || !ids.Any() || ids[0] == 0 || ids[1] == 0)
            {
                await FollowupAsync("Не найдены настройки", ephemeral: true);
                return;
            }

            ticketBonusItem.MessageId = ids[0];
            ticketBonusItem.ChannelId = ids[1];
            await _ticketBaseService.UpdateAsync(ticketBonusItem);

            //Оповещаем пользователя о создании тикета и заполняем остальные данные
            await FollowupAsync($"Создан тикет - {MentionUtils.MentionChannel(ticketBonusItem.ChannelId)}. Как только появится свободный администратор, он возьмет его в работу.", ephemeral: true);
        }
        //Жалобы
        private async Task CreateTicket(TicketTypeEnum ticketType, string reason, string when, string discordTag)
        {
            if (TicketSettings == null || !TicketSettings.Any())
            {
                await FollowupAsync("Не найдены настройки тикетов! Сообщите об этой ошибке администрации сервера", ephemeral: true);
                return;
            }

            var currentSettings = TicketSettings.FirstOrDefault(x => x.RelatedGuildId == Context.Guild.Id);
            if (currentSettings == null)
            {
                await FollowupAsync("Не найдены настройки тикетов текущего сервера! Сообщите об этой ошибке администрации сервера", ephemeral: true);
                return;
            }

            var currentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            var createdBy = await _guildMemberService.GetByDiscordIdAsync(Context.User.Id, Context.Guild.Id);
            if (createdBy == null)
            {
                var member = await _memberService.GetByDiscordIdAsync(Context.User.Id);
                member ??= await _memberService.CreateMemberAsync(Context.User.Id, currentGuild);
                createdBy = await _guildMemberService.GetByMemberAsync(member, currentGuild);
            }

            if (createdBy == null)
            {
                await FollowupAsync("Не найден пользователь сервера", ephemeral: true);
                return;
            }

            TicketComplaint ticketComplaint = new TicketComplaint()
            {
                GuildId = currentGuild.Id,
                CreatedById = createdBy.Id,
                Type = ticketType,
                Reason = reason,
                DiscrodTag = discordTag,
                When = when
            };

            await _ticketBaseService.CreateOrUpdateAsync(ticketComplaint);
            var ids = await CreateUserEmbed(ticketComplaint.Type == TicketTypeEnum.Complaint ? currentSettings.CuratorNewTicketsCategoryId : currentSettings.AdminNewTicketsCategoryId,
                                            ticketComplaint.Type == TicketTypeEnum.Complaint ? currentSettings.CuratorNewTicketsChannelId : currentSettings.AdminNewTicketsChannelId,
                                            ticketComplaint.Type.GetDescription(), ticketComplaint.Id, reason, when, discordTag);

            if (ids == null || !ids.Any() || ids[0] == 0 || ids[1] == 0)
            {
                await FollowupAsync("Не найдены настройки", ephemeral: true);
                return;
            }

            ticketComplaint.MessageId = ids[0];
            ticketComplaint.ChannelId = ids[1];
            await _ticketBaseService.UpdateAsync(ticketComplaint);

            //Оповещаем пользователя о создании тикета и заполняем остальные данные
            await FollowupAsync($"Создан тикет - {MentionUtils.MentionChannel(ticketComplaint.ChannelId)}. Как только появится свободный администратор, он возьмет его в работу.", ephemeral: true);
        }

        /// <summary>
        /// Создает канал-тикет
        /// </summary>
        /// <returns>ID созданного канала</returns>
        
        //TODO Смена ника если тип тикета такой
        private async Task<ulong[]> CreateUserEmbed(ulong categoryId, ulong channelId, string ticketType, long count, string reason)
        {
            try
            {
                var ticketChannel = await Context.Guild.CreateTextChannelAsync($"{ticketType}-тикет-{count}", prop => prop.CategoryId = categoryId);
                await ticketChannel.SyncPermissionsAsync();
                await ticketChannel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow));
                var newTicket = new EmbedBuilder()
                    .WithTitle($"Статус обращения #{count} [Открыто]")
                    .WithDescription("Обращение находится в статусе \"Открыто\". Как только появится свободный администратор, он сразу возьмет ваш вопрос в работу и появится в данном чате.")
                    .AddField("Суть обращения", reason)
                    .WithColor(Color.DarkGrey);
                var closeTicketComponent = new ComponentBuilder().WithButton(new ButtonBuilder()
                {
                    CustomId = "closeTicket",
                    Label = "Закрыть обращение",
                    Style = ButtonStyle.Primary
                });

                var message = await ticketChannel.SendMessageAsync(embed: newTicket.Build(), components: closeTicketComponent.Build());
                var adminMessage = await CreateAdminEmbedMessage(channelId, ticketType, count);
                return new ulong[2] { adminMessage, ticketChannel.Id };
            }
            catch
            {
                await FollowupAsync("Не найден канал");
                return new ulong[2];
            }
        }
        private async Task<ulong[]> CreateUserEmbed(ulong categoryId, ulong channelId, string ticketType, long count, string reason, string when, string discrodTag)
        {
            var ticketChannel = await Context.Guild.CreateTextChannelAsync($"{ticketType}-тикет-{count}", prop => prop.CategoryId = categoryId);
            await ticketChannel.SyncPermissionsAsync();
            await ticketChannel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow));

            var newTicket = new EmbedBuilder()
                .WithTitle($"Статус обращения #{count} [Открыто]")
                .WithDescription("Обращение находится в статусе \"Открыто\". Как только появится свободный администратор, он сразу возьмет ваш вопрос в работу и появится в данном чате.")
                .AddField("Суть жалобы", reason)
                .AddField("Предполагаемый нарушитель", discrodTag)
                .AddField("Дата нарушения", when)
                .WithColor(Color.DarkGrey);

            var closeTicketComponent = new ComponentBuilder().WithButton(new ButtonBuilder()
            {
                CustomId = "closeTicket",
                Label = "Закрыть обращение",
                Style = ButtonStyle.Primary
            });

            var message = await ticketChannel.SendMessageAsync(embed: newTicket.Build(), components: closeTicketComponent.Build());
            var adminMessage = await CreateAdminEmbedMessage(channelId, ticketType, count);
            return new ulong[2] { adminMessage, ticketChannel.Id };
        }

        private async Task<ulong[]> CreateUserEmbed(ulong categoryId, ulong channelId, string ticketType, long count, string reason, string what, string when, string steamId)
        {
            try
            {
                var ticketChannel = await Context.Guild.CreateTextChannelAsync($"{ticketType}-тикет-{count}", prop => prop.CategoryId = categoryId);
                await ticketChannel.SyncPermissionsAsync();
                await ticketChannel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow));
                string whatlosttext = string.Empty;

                if (string.IsNullOrEmpty(reason))
                    reason = "Первое получение";
                if (string.IsNullOrEmpty(when))
                    when = "Первое получение";
                if (string.Equals(ticketType, "восстановление вещей"))
                    whatlosttext = "Список вещей для восставновления";
                else
                    whatlosttext = "Список бонусов для восстановления/выдачи";


                    var newTicket = new EmbedBuilder()
                    .WithTitle($"Статус обращения #{count} [Открыто]")
                    .WithDescription("Обращение находится в статусе \"Открыто\". Как только появится свободный администратор, он сразу возьмет ваш вопрос в работу и появится в данном чате.")
                    .AddField("SteamID", steamId)
                    .AddField("Как утеряно", reason)
                    .AddField(whatlosttext, what)
                    .AddField("Дата утери", when)
                    .WithColor(Color.DarkGrey);

                var closeTicketComponent = new ComponentBuilder().WithButton(new ButtonBuilder()
                {
                    CustomId = "closeTicket",
                    Label = "Закрыть обращение",
                    Style = ButtonStyle.Primary
                });

                var message = await ticketChannel.SendMessageAsync(embed: newTicket.Build(), components: closeTicketComponent.Build());
                var adminMessage = await CreateAdminEmbedMessage(channelId, ticketType, count);
                return new ulong[2] { adminMessage, ticketChannel.Id };
            }
            catch
            {
                await FollowupAsync("Не найден канал");
                return new ulong[2];
            }
        }

        /// <summary>
        /// Создает кнопку для взятия тикета в работу для админитсрации
        /// </summary>
        /// <returns>ID созданной кнопки</returns>
        private async Task<ulong> CreateAdminEmbedMessage(ulong channelId, string ticketType, long count)
        {
            var channel = Context.Guild.GetTextChannel(channelId);
            if (channel == null)
            {
                await FollowupAsync("Не найден канал");
                return 0;
            }

            var adminMessageEmbed = new EmbedBuilder()
                .WithTitle($"Тикет #{count} [Открыто]")
                .WithDescription($"Обращение создал - {MentionUtils.MentionUser(Context.User.Id)} ({Context.User.Username})")
                .WithCurrentTimestamp()
                .WithColor(Color.LightGrey);
            var adminMessageButton = new ComponentBuilder().WithButton(new ButtonBuilder()
            {
                CustomId = "takeTicket",
                Label = "Взять тикет в работу",
                Style = ButtonStyle.Primary
            });

            var message = await channel.SendMessageAsync(embed: adminMessageEmbed.Build(), components: adminMessageButton.Build());
            return message.Id;
        }

        //[ComponentInteraction("takeTicket")]
        //public async Task TakeTicket()
        //{
        //    await DeferAsync(ephemeral: true);

        //    var ticket = _ticketService.GetByMessageId(((SocketMessageComponent)Context.Interaction).Message.Id).Result;
        //    if (ticket == null)
        //    {
        //        await FollowupAsync("Ошибка, тикет не существует. Сообщите об этой ошибке администрации сервера", ephemeral: true);
        //        return;
        //    }

        //    var ticketSettings = _ticketService.GetSettingsByGuildId(Context.Guild.Id).Result;
        //    if (ticketSettings == null)
        //    {
        //        await FollowupAsync("Не найдены настройки тикетов! Сообщите об этой ошибке администрации сервера", ephemeral: true);
        //        return;
        //    }

        //    var ticketChannel = Context.Guild.GetTextChannel(ticket.ChannelId);
        //    var ticketMessage = (await ticketChannel.GetMessageAsync(ticket.MessageId)) as SocketMessageComponent;

        //    if (ticketMessage == null)
        //    {
        //        await FollowupAsync("Не найдено сообщение! Сообщите об этой ошибке администрации сервера", ephemeral: true);
        //        return;
        //    }

        //    await ticketChannel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow));
        //    await ticketChannel.SendMessageAsync($"{MentionUtils.MentionUser(Context.User.Id)} взял тикет в работу");

        //    var ticketEmbed = ticketMessage.Message.Embeds.First();
        //    var ticketEmbedUpdated = ticketEmbed.ToEmbedBuilder();
        //    ticketEmbedUpdated.Title = ticketEmbed.Title.Replace("[Открыто]", "[В работе]");
        //    ticketEmbedUpdated.AddField("Взял в работу", $"{MentionUtils.MentionUser(Context.User.Id)} ({Context.User.Username})");
        //    ticketEmbedUpdated.Color = Color.Orange;

        //    await ticketMessage.UpdateAsync(msg => { msg.Embeds = new Embed[] { ticketEmbedUpdated.Build() }; msg.Components = new ComponentBuilder().Build(); });
        //    ticket.MessageId = ticketMessage.Id;
        //    _ticketService.Update(ticket);

        //    await Context.Guild.GetTextChannel(ticketSettings.TicketLogs)
        //        .SendMessageAsync($"{MentionUtils.MentionUser(Context.User.Id)} ({Context.User.Username}) взял в работу тикет {MentionUtils.MentionChannel(ticketChannel.Id)} ({Context.Guild.GetChannel(ticketChannel.Id).Name})");

        //    await FollowupAsync($"Тикет {MentionUtils.MentionChannel(ticketChannel.Id)} взят в работу.", ephemeral: true);
        //}

        //[ComponentInteraction("closeTicket")]
        //public async Task CloseTicket()
        //{
        //    await DeferAsync(ephemeral: true);


        //    var ticket = _ticketService.GetByChannelId(Context.Channel.Id).Result;
        //    if (ticket == null)
        //    {
        //        await FollowupAsync("Ошибка, тикет не существует. Сообщите об этой ошибке администрации сервера", ephemeral: true);
        //        return;
        //    }

        //    var ticketSettings = _ticketService.GetSettingsByGuildId(Context.Guild.Id).Result;
        //    if (ticketSettings == null)
        //    {
        //        await FollowupAsync("Не найдены настройки тикетов! Сообщите об этой ошибке администрации сервера", ephemeral: true);
        //        return;
        //    }

        //    var ticketChannel = Context.Guild.GetTextChannel(Context.Channel.Id);

        //    switch (ticket.Type)
        //    {
        //        case TicketTypeEnum.Helper:
        //            await ticketChannel.ModifyAsync(prop => prop.CategoryId = ticketSettings.OldHelperTicketsCategory);
        //            break;

        //        case TicketTypeEnum.Curator:
        //            await ticketChannel.ModifyAsync(prop => prop.CategoryId = ticketSettings.OldCuratorTicketsCategory);
        //            break;

        //        case TicketTypeEnum.Admin:
        //            await ticketChannel.ModifyAsync(prop => prop.CategoryId = ticketSettings.OldAdminTicketsCategory);
        //            break;

        //        default:
        //            await FollowupAsync("Ошибка, тип тикета не найден");
        //            return;
        //    }

        //    await ticketChannel.SyncPermissionsAsync();

        //    ticket.DeleteTime = DateTime.Now.AddDays(3);
        //    ticket.IsFinished = true;
        //    _ticketService.Update(ticket);

        //    var closeTicketMessage = (SocketMessageComponent)Context.Interaction;
        //    var closeTicketEmbed = closeTicketMessage.Message.Embeds.First().ToEmbedBuilder();
        //    closeTicketEmbed.Color = Color.Green;
        //    await closeTicketMessage.UpdateAsync(msg => { msg.Embeds = new Embed[] { closeTicketEmbed.Build() }; msg.Components = new ComponentBuilder().Build(); });

        //    var ticketStatusMessage = ticketChannel.GetMessageAsync(ticket.MessageId).Result;

        //    var embed = ticketStatusMessage?.Embeds.First();
        //    var newEmbed = embed.ToEmbedBuilder();
        //    newEmbed.Title = embed?.Title.Replace("[В работе]", "[Закрыто]");
        //    newEmbed.AddField("Описание проблемы", closeTicketEmbed.Description);
        //    newEmbed.AddField("Закрыл ", $"{MentionUtils.MentionUser(Context.User.Id)} ({Context.User.Username})");
        //    newEmbed.Color = Color.Green;

        //    await Context.Guild.GetTextChannel(ticketSettings.TicketLogs).SendMessageAsync(embed: newEmbed.Build());
        //    await FollowupAsync("Тикет закрыт", ephemeral: true);
        //}

        #endregion

        private async Task<bool> ValidateSteamID(string steamID)
        {
            var steamApiKey = SteamKey.CurrentValue;
            if (steamApiKey == null)
                return false;

            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(string.Format("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids=/{1}", steamApiKey, steamID.ToString()));
            if (response == null)
                return false;

            SteamResponse steamResponse = JsonSerializer.Deserialize<SteamResponse>(response);
            if (steamResponse == null)
                return false;

            if (steamResponse.response == null)
                return false;

            if (steamResponse.response.players == null || !steamResponse.response.players.Any())
                return false;

            if (steamResponse.response.players.First().steamid != steamID.ToString())
                return false;

            return true;
        }
    }
}
