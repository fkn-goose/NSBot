using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Entities.Warn;
using NS.Bot.Shared.Enums;

namespace NS.Bot.App.Commands
{
    [Group("предупреждение", "Команды для предупреждений")]
    public class WarnModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IMemberService _memberService;
        private readonly IGuildService _guildService;
        private readonly IGuildMemberService _guildMemberService;
        private readonly IWarnSettingsService _warnSettingsService;
        private readonly IWarnService _warnService;
        private GuildEntity CurrentGuild;
        private static Dictionary<ulong, WarnSettings> WarnSettings = new Dictionary<ulong, WarnSettings>();
        public WarnModule(IMemberService memberService, IGuildService guildService, IGuildMemberService guildMemberService, IWarnSettingsService warnSettingsService, IWarnService warnService)
        {
            _memberService = memberService;
            _guildService = guildService;
            _guildMemberService = guildMemberService;
            _warnSettingsService = warnSettingsService;
            _warnService = warnService;
        }

        [SlashCommand("выдать", "Выдать предупреждение")]
        public async Task GiveWarn([Summary("Тип")] WarnType type, [Summary("Пользователь")] IGuildUser user, [Summary("Причина")] string reason, [Summary("Длительность")] uint? durationInput, [Summary("измерение")] TimeUnitEnum? timeUnit, [Summary("Бессрочный")] bool IsPermanent = false)
        {
            //if (durationInput <= 0)
            //{
            //    await RespondAsync("Длительность должна быть положительным целым числом");
            //    return;
            //}

            await DeferAsync(ephemeral: true);

            CurrentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);

            if (!WarnSettings.TryGetValue(Context.Guild.Id, out WarnSettings settings))
            {
                settings = await _warnSettingsService.GetWarnSettingsAsync(Context.Guild.Id);
                if (settings == null)
                {
                    await FollowupAsync("Не удалось поулчить настройки предупреждений", ephemeral: true);
                    return;
                }
                WarnSettings[Context.Guild.Id] = settings;
            }

            var issuedToGuildMember = await _guildMemberService.GetByDiscordIdAsync(user.Id, CurrentGuild.GuildId);
            if (issuedToGuildMember == null)
            {
                var member = await _memberService.GetByDiscordIdAsync(user.Id);
                member ??= await _memberService.CreateMemberAsync(user.Id, CurrentGuild);
                issuedToGuildMember = await _guildMemberService.GetByMemberAsync(member, CurrentGuild);
            }

            if (issuedToGuildMember == null)
            {
                await FollowupAsync("Не найден пользователь сервера", ephemeral: true);
                return;
            }

            var responsibleGuildMember = await _guildMemberService.GetByDiscordIdAsync(Context.User.Id, CurrentGuild.GuildId);
            if (responsibleGuildMember == null)
            {
                var member = await _memberService.GetByDiscordIdAsync(Context.User.Id);
                member ??= await _memberService.CreateMemberAsync(Context.User.Id, CurrentGuild);
                responsibleGuildMember = await _guildMemberService.GetByMemberAsync(member, CurrentGuild);
            }

            WarnEntity warn = new WarnEntity()
            {
                Responsible = responsibleGuildMember.Member,
                IssuedTo = issuedToGuildMember.Member,
                Reason = reason,
                FromDate = DateTime.Now,
                IsPermanent = IsPermanent,
                WarnType = type,
            };

            DateTime endDate;
            uint durationInSeconds = 0;

            switch (type)
            {
                case WarnType.Verbal:
                    issuedToGuildMember.Member.TotalWarnCount++;
                    await _memberService.UpdateAsync(issuedToGuildMember.Member);
                    await _warnService.CreateOrUpdateAsync(warn);
                    var msg = await Context.Guild.GetTextChannel(settings.WarnChannelId).SendMessageAsync(text: MentionUtils.MentionUser(user.Id), embed: await GetEmbedWarnMessage(warn));
                    warn.MessageId = msg.Id;
                    await _warnService.UpdateAsync(warn);
                    await FollowupAsync("Игроку выдано устное предупреждение", ephemeral: true);
                    return;
                case WarnType.Rebuke:
                case WarnType.Ordinary:

                    if (durationInput == null)
                    {
                        await FollowupAsync("Не указана длительность и. измерения", ephemeral: true);
                        return;
                    }

                    if (timeUnit == null)
                    {
                        await FollowupAsync("Не указана длительность ед. измерения", ephemeral: true);
                        return;
                    }

                    switch (timeUnit)
                    {
                        case TimeUnitEnum.Seconds:
                            endDate = DateTime.Now.AddSeconds(durationInput.Value);
                            durationInSeconds = durationInput.Value;
                            break;
                        case TimeUnitEnum.Minutes:
                            endDate = DateTime.Now.AddMinutes(durationInput.Value);
                            durationInSeconds = durationInput.Value * 60;
                            break;
                        case TimeUnitEnum.Hours:
                            endDate = DateTime.Now.AddHours(durationInput.Value);
                            durationInSeconds = durationInput.Value * 60 * 60;
                            break;
                        case TimeUnitEnum.Days:
                            endDate = DateTime.Now.AddDays(durationInput.Value);
                            durationInSeconds = durationInput.Value * 60 * 60 * 24;
                            break;
                        default:
                            await FollowupAsync("Не указана длительность и/или ед. измерения", ephemeral: true);
                            return;
                    }

                    warn.ToDate = endDate;
                    warn.Duration = durationInSeconds;

                    var activeWarnsCount = _warnService.GetAll().Where(x => x.IssuedTo.Id == issuedToGuildMember.Member.Id).ToList()?.Count(w => w.IsActive && (w.WarnType == WarnType.Ordinary || w.WarnType == WarnType.Rebuke)) ?? 0;

                    if (type == WarnType.Rebuke)
                    {
                        if (activeWarnsCount < 3)
                        {
                            switch (activeWarnsCount)
                            {
                                case 0:
                                    await user.AddRoleAsync(settings.FirstRebukeRoleId);
                                    await FollowupAsync("Игроку выдан первый выговор", ephemeral: true);
                                    break;
                                case 1:
                                    await user.AddRoleAsync(settings.SecondRebukeRoleId);
                                    await FollowupAsync("Игроку выдан второй выговор", ephemeral: true);
                                    break;
                                case 2:
                                    await user.AddRoleAsync(settings.ThirdRebukeRoleId);
                                    await FollowupAsync("Игроку выдан третий выговор", ephemeral: true);
                                    break;
                                default:
                                    await FollowupAsync("Ошибка: кол-во выговоров некорректно", ephemeral: true);
                                    return;
                            }
                            issuedToGuildMember.Member.TotalWarnCount++;
                            await _memberService.UpdateAsync(issuedToGuildMember.Member);
                            await _warnService.CreateOrUpdateAsync(warn);
                            var ordinaryMsg = await Context.Guild.GetTextChannel(settings.WarnChannelId).SendMessageAsync(text: MentionUtils.MentionUser(user.Id), embed: await GetEmbedWarnMessage(warn));
                            warn.MessageId = ordinaryMsg.Id;
                            await _warnService.UpdateAsync(warn);
                        }
                        else
                        {
                            await FollowupAsync("У игрока уже есть три выговора", ephemeral: true);
                            return;
                        }
                    }
                    if (activeWarnsCount < 3)
                    {
                        switch (activeWarnsCount)
                        {
                            case 0:
                                await user.AddRoleAsync(settings.FirstWarnRoleId);
                                await FollowupAsync("Игроку выдано первое предупреждение", ephemeral: true);
                                break;
                            case 1:
                                await user.AddRoleAsync(settings.SecondWarnRoleId);
                                await FollowupAsync("Игроку выдано второе предупреждение", ephemeral: true);
                                break;
                            case 2:
                                await user.AddRoleAsync(settings.ThirdWarnRoleId);
                                await FollowupAsync("Игроку выдано третье предупреждение", ephemeral: true);
                                break;
                            default:
                                await FollowupAsync("Ошибка: кол-во предов некорректно", ephemeral: true);
                                return;
                        }
                        issuedToGuildMember.Member.TotalWarnCount++;
                        await _memberService.UpdateAsync(issuedToGuildMember.Member);
                        await _warnService.CreateOrUpdateAsync(warn);
                        var ordinaryMsg = await Context.Guild.GetTextChannel(settings.WarnChannelId).SendMessageAsync(text: MentionUtils.MentionUser(user.Id), embed: await GetEmbedWarnMessage(warn));
                        warn.MessageId = ordinaryMsg.Id;
                        await _warnService.UpdateAsync(warn);
                    }
                    else
                    {
                        await FollowupAsync("У игрока уже есть три предупреждения", ephemeral: true);
                        return;
                    }
                    break;
                case WarnType.ReadOnly:
                    var activeROCount = _warnService.GetAll().Where(x => x.IssuedTo.Id == issuedToGuildMember.Member.Id).ToList()?.Count(w => w.IsActive && w.WarnType == WarnType.ReadOnly) ?? 0;
                    if (activeROCount > 0)
                    {
                        await FollowupAsync("У игрока уже есть ReadOnly", ephemeral: true);
                        return;
                    }

                    switch (timeUnit)
                    {
                        case TimeUnitEnum.Seconds:
                            endDate = DateTime.Now.AddSeconds(durationInput.Value);
                            durationInSeconds = durationInput.Value;
                            break;
                        case TimeUnitEnum.Minutes:
                            endDate = DateTime.Now.AddMinutes(durationInput.Value);
                            durationInSeconds = durationInput.Value * 60;
                            break;
                        case TimeUnitEnum.Hours:
                            endDate = DateTime.Now.AddHours(durationInput.Value);
                            durationInSeconds = durationInput.Value * 60 * 60;
                            break;
                        case TimeUnitEnum.Days:
                            endDate = DateTime.Now.AddDays(durationInput.Value);
                            durationInSeconds = durationInput.Value * 60 * 60 * 24;
                            break;
                        default:
                            await FollowupAsync("Не указана длительность и/или ед. измерения", ephemeral: true);
                            return;
                    }
                    warn.ToDate = endDate;
                    warn.Duration = durationInSeconds;
                    await user.AddRoleAsync(settings.ReadOnlyRoleId);
                    await _warnService.CreateOrUpdateAsync(warn);
                    var ROMsg = await Context.Guild.GetTextChannel(settings.WarnChannelId).SendMessageAsync(text: MentionUtils.MentionUser(user.Id), embed: await GetEmbedWarnMessage(warn));
                    warn.MessageId = ROMsg.Id;
                    await FollowupAsync("Игроку выдан ReadOnly", ephemeral: true);
                    await _warnService.UpdateAsync(warn);
                    break;
            }
        }

        [SlashCommand("отозвать", "отозвать")]
        public async Task RevokeWarn([Summary("номер")] long id)
        {
            if (!(Context.User as SocketGuildUser).GuildPermissions.Administrator)
            {
                await RespondAsync("Недостаточно прав", ephemeral: true);
                return;
            }
            await DeferAsync(ephemeral: true);

            CurrentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);

            var warn = _warnService.GetAll().FirstOrDefault(x => x.Id == id);
            if (warn == null)
            {
                await FollowupAsync("Предупреждение не найдено", ephemeral: true);
                return;
            }

            var guilds = _guildService.GetAll().ToList();
            var dsGuilds = new List<SocketGuild>();
            foreach (var guild in guilds)
            {
                var discrodGuild = Context.Client.GetGuild(guild.GuildId);
                dsGuilds.Add(discrodGuild);
            }

            if (!dsGuilds.Any())
            {
                await FollowupAsync("Не найдены сервера", ephemeral: true);
                return;
            }

            var activeMemberWarns = _warnService.GetAll().Where(x => x.IssuedTo.Id == warn.IssuedTo.Id && x.IsActive).ToList();
            if (activeMemberWarns != null && !activeMemberWarns.Any())
                return;

            if (warn.IsActive)
            {
                foreach (var guild in dsGuilds)
                {
                    if (!WarnSettings.TryGetValue(Context.Guild.Id, out WarnSettings settings))
                    {
                        settings = await _warnSettingsService.GetWarnSettingsAsync(guild.Id);
                        if (settings == null)
                            continue;
                        WarnSettings[Context.Guild.Id] = settings;
                    }

                    switch (warn.WarnType)
                    {
                        case WarnType.ReadOnly:
                            activeMemberWarns = activeMemberWarns?.Where(x => x.WarnType == WarnType.Rebuke).ToList() ?? new List<WarnEntity>();

                            await guild.GetUser(warn.IssuedTo.DiscordId).RemoveRoleAsync(settings.ReadOnlyRoleId);

                            break;

                        case WarnType.Rebuke:
                            activeMemberWarns = activeMemberWarns?.Where(x => x.WarnType == WarnType.Rebuke).ToList() ?? new List<WarnEntity>();

                            if (activeMemberWarns.Count == 3)
                                await guild.GetUser(warn.IssuedTo.DiscordId).RemoveRoleAsync(settings.ThirdRebukeRoleId);
                            else if (activeMemberWarns.Count == 2)
                                await guild.GetUser(warn.IssuedTo.DiscordId).RemoveRoleAsync(settings.SecondRebukeRoleId);
                            else if (activeMemberWarns.Count == 1)
                                await guild.GetUser(warn.IssuedTo.DiscordId).RemoveRoleAsync(settings.FirstRebukeRoleId);

                            break;

                        case WarnType.Ordinary:
                            activeMemberWarns = activeMemberWarns?.Where(x => x.WarnType == WarnType.Ordinary).ToList() ?? new List<WarnEntity>();

                            if (activeMemberWarns.Count == 3)
                                await guild.GetUser(warn.IssuedTo.DiscordId).RemoveRoleAsync(settings.ThirdWarnRoleId);
                            else if (activeMemberWarns.Count == 2)
                                await guild.GetUser(warn.IssuedTo.DiscordId).RemoveRoleAsync(settings.SecondWarnRoleId);
                            else if (activeMemberWarns.Count == 1)
                                await guild.GetUser(warn.IssuedTo.DiscordId).RemoveRoleAsync(settings.FirstWarnRoleId);
                            break;

                        case WarnType.Verbal:
                            break;
                    }
                }
            }

            var chnl = Context.Guild.GetTextChannel(WarnSettings[Context.Guild.Id].WarnChannelId);
            var msg = await chnl.GetMessageAsync(warn.MessageId);
            var embed = msg.Embeds.First().ToEmbedBuilder();
            var removeField = embed.Fields.FirstOrDefault(x => x.Name == "Истекает");
            if (removeField != null)
                embed.Fields.Remove(removeField);

            embed.AddField("Предупреждение снято", $"Администратор - {MentionUtils.MentionUser(Context.User.Id)} ({Context.User.Username})");
            embed.WithColor(Color.Green);
            await chnl.ModifyMessageAsync(warn.MessageId, msg => { msg.Embeds = new Embed[] { embed.Build() }; });

            warn.IsActive = false;
            warn.IssuedTo.TotalWarnCount--;
            await _warnService.UpdateAsync(warn);
            await _memberService.UpdateAsync(warn.IssuedTo);

            await FollowupAsync("Предупреждение отозвано", ephemeral: true);
        }

        [SlashCommand("история", "история")]
        public async Task GetUserWarns([Summary("Пользователь")] IGuildUser user, [Summary("Активные", "Отображать только активные преды. По умолчанию - да")] YesNoEnum IsActive = YesNoEnum.Yes, [Summary("Устные", "Отображать ли устные преды. По умолчанию - нет")] YesNoEnum IsWithVerbal = YesNoEnum.No)
        {
            await DeferAsync(ephemeral: true);

            CurrentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);

            var warnquerry = _warnService.GetAll().Where(x => x.IssuedTo.DiscordId == user.Id);
            var temp = warnquerry.ToList();
            bool Active = IsActive == YesNoEnum.Yes;
            bool WithVerbal = IsWithVerbal == YesNoEnum.Yes;
            if (Active)
                warnquerry = warnquerry.Where(x => x.IsActive);
            if (!WithVerbal)
                warnquerry = warnquerry.Where(x => x.WarnType != WarnType.Verbal);

            var warns = warnquerry.ToList();
            if (warns == null || !warns.Any())
            {
                await FollowupAsync("Предупреждения не найдены", ephemeral: true);
                return;
            }

            if (!WarnSettings.TryGetValue(Context.Guild.Id, out WarnSettings settings))
            {
                settings = await _warnSettingsService.GetWarnSettingsAsync(Context.Guild.Id);
                if (settings == null)
                {
                    await FollowupAsync("Не удалось поулчить настройки предупреждений", ephemeral: true);
                    return;
                }
                WarnSettings[Context.Guild.Id] = settings;
            }

            var response = string.Empty;
            var unknownWarns = 0;
            foreach (var warn in warns)
            {
                var msg = await Context.Guild.GetTextChannel(settings.WarnChannelId).GetMessageAsync(warn.MessageId);
                if (msg == null)
                {
                    unknownWarns++;
                    continue;
                }
                //Собираем ссылку на сообщение вручную, лол
                var link = string.Format("{0}/{1}/{2}/{3}", "https://discord.com/channels", Context.Guild.Id, settings.WarnChannelId, warn.MessageId);
                response += (link + " \n");
            }

            response += string.Format("{0} предупреждений было выдано не на этом сервере", unknownWarns);

            await FollowupAsync(response, ephemeral: true);
        }

        private async Task<Embed> GetEmbedWarnMessage(WarnEntity warn)
        {
            string title = warn.WarnType switch
            {
                WarnType.Verbal => "Устное предупреждение",
                WarnType.ReadOnly => "ReadOnly",
                WarnType.Rebuke => "Выговор",
                WarnType.Ordinary => "Предупреждение",
                _ => ""
            };

            var responsibleDiscrod = Context.Guild.GetUser((await _memberService.Get(warn.ResponsibleId)).DiscordId);
            var issuedToDiscrod = Context.Guild.GetUser((await _memberService.Get(warn.IssuedToId)).DiscordId);

            var embedWarn = new EmbedBuilder()
                .WithTitle(title)
                .WithColor(Color.Red)
                .AddField("Причина", warn.Reason)
                .AddField("Администратор", string.Format("{0} ({1})", MentionUtils.MentionUser(responsibleDiscrod.Id), responsibleDiscrod.Username));

            if (warn.WarnType != WarnType.Verbal && !warn.IsPermanent)
                embedWarn.AddField("Истекает", new TimestampTag(warn.ToDate, TimestampTagStyles.Relative));

            embedWarn.WithFooter($"Уникальный идентификатор - {warn.Id.ToString()}");

            return embedWarn.Build();
        }
    }
}
