using Discord;
using Discord.Interactions;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Entities.Warn;
using NS.Bot.Shared.Enums;

namespace NS.Bot.App.Commands
{
    public class WarnModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IMemberService _memberService;
        private readonly IGuildService _guildService;
        private readonly IGuildMemberService _guildMemberService;
        private readonly IWarnSettingsService _warnSettingsService;
        private readonly IBaseService<WarnEntity> _warnService;
        private GuildEntity CurrentGuild;
        private static Dictionary<ulong, WarnSettings> WarnSettings = new Dictionary<ulong, WarnSettings>();
        public WarnModule(IMemberService memberService, IGuildService guildService, IGuildMemberService guildMemberService, IWarnSettingsService warnSettingsService, IBaseService<WarnEntity> warnService)
        {
            _memberService = memberService;
            _guildService = guildService;
            _guildMemberService = guildMemberService;
            _warnSettingsService = warnSettingsService;
            _warnService = warnService;
        }

        [SlashCommand("initwarns", "Настройка предов")]
        [RequireOwner]
        public async Task InitWarnSettings([Summary("Канал_предов")] ITextChannel warnChannel, [Summary("Первый")] IRole firstRole, [Summary("Второй")] IRole secondRole, [Summary("Третий")] IRole thirdRole, [Summary("Бан")] IRole banRole, [Summary("Ридонли")] IRole readonlyRole)
        {
            await DeferAsync(ephemeral: true);

            var guild = await _guildService.GetByDiscordId(Context.Guild.Id);
            if (guild == null)
                return;
            WarnSettings settings = new WarnSettings()
            {
                RelatedGuild = guild,
                WarnChannelId = warnChannel.Id,
                FirstWarnRoleId = firstRole.Id,
                SecondWarnRoleId = secondRole.Id,
                ThirdWarnRoleId = thirdRole.Id,
                BanRoleId = banRole.Id,
                ReadOnlyRoleId = readonlyRole.Id
            };

            await _warnSettingsService.CreateOrUpdateAsync(settings);
            if (!WarnSettings.TryAdd(Context.Guild.Id, settings))
            {
                WarnSettings.Remove(Context.Guild.Id);
                WarnSettings.Add(Context.Guild.Id, settings);
            }

            await FollowupAsync("Настройки сохранены", ephemeral: true);
        }

        [SlashCommand("предупреждение", "Выдать предупреждение")]
        public async Task GiveWarn([Summary("Пользователь")] IUser user, [Summary("Причина")] string reason, [Summary("Длительность")] uint durationInput, [Summary("измерение")] TimeUnitEnum timeUnit)
        {
            if (durationInput <= 0)
            {
                await RespondAsync("Длительность должна быть положительным целым числом");
                return;
            }    

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

            DateTime endDate;

            switch (timeUnit)
            {
                case TimeUnitEnum.Seconds:
                    endDate = DateTime.Now.AddSeconds(durationInput);
                    break;
                case TimeUnitEnum.Minutes:
                    endDate = DateTime.Now.AddMinutes(durationInput);
                    break;
                case TimeUnitEnum.Hours:
                    endDate = DateTime.Now.AddHours(durationInput);
                    break;
                case TimeUnitEnum.Days:
                    endDate = DateTime.Now.AddDays(durationInput);
                    break;
                default:
                    await FollowupAsync("Не указана длительность и/или ед. измерения", ephemeral: true);
                    return;
            }

            WarnEntity warn = new WarnEntity()
            {
                Responsible = responsibleGuildMember.Member,
                IssuedTo = issuedToGuildMember.Member,
                Reason = reason,
                FromDate = DateTime.Now,
                ToDate = endDate,
                Duration = durationInput
            };


            var userRoles = (user as IGuildUser).RoleIds;
            var activeWarnsCount = issuedToGuildMember.Member.Warns?.Count(w => w.IsActive) ?? 0;

            if (activeWarnsCount < 3)
            {
                switch (activeWarnsCount)
                {
                    case 0:
                        await (user as IGuildUser).AddRoleAsync(settings.FirstWarnRoleId);
                        await FollowupAsync("Игроку выдано первое предупреждение", ephemeral: true);
                        break;
                    case 1:
                        await (user as IGuildUser).AddRoleAsync(settings.SecondWarnRoleId);
                        await FollowupAsync("Игроку выдано второе предупреждение", ephemeral: true);
                        break;
                    case 2:
                        await (user as IGuildUser).AddRoleAsync(settings.ThirdWarnRoleId);
                        await FollowupAsync("Игроку выдано третье предупреждение", ephemeral: true);
                        break;
                    default:
                        await FollowupAsync("Ошибка: кол-во предов некорректно", ephemeral: true);
                        return;
                }
                issuedToGuildMember.Member.TotalWarnCount++;
                await _memberService.Update(issuedToGuildMember.Member);
                await Context.Guild.GetTextChannel(settings.WarnChannelId).SendMessageAsync(embed: SendEmbedWarnMessage(warn));
                await _warnService.CreateOrUpdateAsync(warn);
            }
            else
            {
                await FollowupAsync("У игрока уже есть три предупреждения", ephemeral: true);
                return;
            }
        }

        private Embed SendEmbedWarnMessage(WarnEntity warn)
        {
            var embedWarn = new EmbedBuilder()
                .WithTitle("Предупреждение")
                .WithColor(Color.Red)
                .AddField("Администратор", MentionUtils.MentionUser(warn.Responsible.DiscordId))
                .AddField("Нарушитель", MentionUtils.MentionUser(warn.IssuedTo.DiscordId))
                .AddField("Причина", warn.Reason)
                .AddField("Истекает", new TimestampTag(warn.ToDate, TimestampTagStyles.Relative));

            return embedWarn.Build();
        }
    }
}
