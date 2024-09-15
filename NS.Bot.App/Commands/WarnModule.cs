using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Entities.Warn;
using NS.Bot.Shared.Enums;

namespace NS.Bot.App.Commands
{
    [Group("предупреждение", "предупреждение")]
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
                IsPermanent = IsPermanent
            };

            DateTime endDate;
            uint durationInSeconds = 0;

            switch (type)
            {
                case WarnType.Verbal:
                    warn.IsVerbal = true;
                    issuedToGuildMember.Member.TotalWarnCount++;
                    await _memberService.UpdateAsync(issuedToGuildMember.Member);
                    await _warnService.CreateOrUpdateAsync(warn);
                    var msg = await Context.Guild.GetTextChannel(settings.WarnChannelId).SendMessageAsync(embed: GetEmbedWarnMessage(warn));
                    warn.MessageId = msg.Id;
                    await FollowupAsync("Игроку выдано устное предупреждение", ephemeral: true);
                    return;
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

                    var activeWarnsCount = issuedToGuildMember.Member.Warns?.Count(w => w.IsActive && !w.IsVerbal && !w.IsReadOnly && !w.IsRebuke && !w.IsPermanent) ?? 0;

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
                        var ordinaryMsg = await Context.Guild.GetTextChannel(settings.WarnChannelId).SendMessageAsync(embed: GetEmbedWarnMessage(warn));
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
                    var activeROCount = issuedToGuildMember.Member.Warns?.Count(w => w.IsActive && !w.IsVerbal && w.IsReadOnly && !w.IsRebuke && !w.IsPermanent) ?? 0;
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
                    warn.IsReadOnly = true;
                    warn.ToDate = endDate;
                    warn.Duration = durationInSeconds;
                    await user.AddRoleAsync(settings.ReadOnlyRoleId);
                    await _warnService.CreateOrUpdateAsync(warn);
                    var ROMsg = await Context.Guild.GetTextChannel(settings.WarnChannelId).SendMessageAsync(embed: GetEmbedWarnMessage(warn));
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
                await RespondAsync("Недостаточно прав", ephemeral:true);
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

            warn.IsActive = false;

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

            foreach (var guild in dsGuilds)
            {
                if (!WarnSettings.TryGetValue(Context.Guild.Id, out WarnSettings settings))
                {
                    settings = await _warnSettingsService.GetWarnSettingsAsync(guild.Id);
                    if (settings == null)
                        continue;
                    WarnSettings[Context.Guild.Id] = settings;
                }
                if (warn.IsReadOnly)
                    await guild.GetUser(warn.IssuedTo.DiscordId).RemoveRoleAsync(settings.ReadOnlyRoleId);
                else
                {
                    if (warn.IssuedTo.Warns.Count(w => w.IsActive && !w.IsVerbal && !w.IsReadOnly && !w.IsRebuke && !w.IsPermanent) == 3)
                        await guild.GetUser(warn.IssuedTo.DiscordId).RemoveRoleAsync(settings.ThirdWarnRoleId);

                    if (warn.IssuedTo.Warns.Count(w => w.IsActive && !w.IsVerbal && !w.IsReadOnly && !w.IsRebuke && !w.IsPermanent) == 2)
                        await guild.GetUser(warn.IssuedTo.DiscordId).RemoveRoleAsync(settings.SecondWarnRoleId);

                    if (warn.IssuedTo.Warns.Count(w => w.IsActive && !w.IsVerbal && !w.IsReadOnly && !w.IsRebuke && !w.IsPermanent) == 1)
                        await guild.GetUser(warn.IssuedTo.DiscordId).RemoveRoleAsync(settings.FirstWarnRoleId);
                }
            }

            var chnl = Context.Guild.GetTextChannel(WarnSettings[Context.Guild.Id].WarnChannelId);
            var msg = await chnl.GetMessageAsync(warn.MessageId);
            var embed = msg.Embeds.First().ToEmbedBuilder();
            var removeField = embed.Fields.FirstOrDefault(x => x.Name == "Истекает");
            if (removeField != null)
            {
                embed.Fields.Remove(removeField);
                embed.AddField("Истекает", $"Предупреждение снято. Администратор - {MentionUtils.MentionUser(Context.User.Id)}");
                embed.WithColor(Color.Green);
                await chnl.ModifyMessageAsync(warn.MessageId, msg => { msg.Embeds = new Embed[] { embed.Build() }; });
            }
            await _warnService.UpdateAsync(warn);
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
                warnquerry = warnquerry.Where(x => !x.IsVerbal);

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

            await FollowupAsync(response, ephemeral: true);
        }

        private Embed GetEmbedWarnMessage(WarnEntity warn)
        {
            string title = warn.IsVerbal ? "Устное предупреждение № " : warn.IsReadOnly ? "ReadOnly №" : "Предупреждение №";
            title += warn.Id.ToString();

            var embedWarn = new EmbedBuilder()
                .WithTitle(title)
                .WithColor(Color.Red)
                .AddField("Администратор", MentionUtils.MentionUser(warn.Responsible.DiscordId))
                .AddField("Нарушитель", MentionUtils.MentionUser(warn.IssuedTo.DiscordId))
                .AddField("Причина", warn.Reason);
            if (!warn.IsVerbal)
                embedWarn.AddField("Истекает", new TimestampTag(warn.ToDate, TimestampTagStyles.Relative));

            return embedWarn.Build();
        }
    }
}
