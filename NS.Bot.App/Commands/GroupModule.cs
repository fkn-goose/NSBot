using Discord;
using Discord.Interactions;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Group;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Enums;
using NS.Bot.Shared.Extensions;

namespace NS.Bot.Commands.CommandModules
{
    public class GroupModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IGroupService _groupService;
        private readonly IMemberService _memberService;
        private readonly IGuildService _guildService;
        private readonly IGuildMemberService _guildMemberService;
        private GuildEntity CurrentGuild;
        public GroupModule(IGroupService groupService, IMemberService memberService, IGuildService guildService, IGuildMemberService guildMemberService)
        {
            _groupService = groupService;
            _memberService = memberService;
            _guildService = guildService;
            _guildMemberService = guildMemberService;
        }

        [SlashCommand("вписать", "Вписать игрока в группировку")]
        public async Task AddToGroup([Summary(name: "Группировка", description: "Название группировки")] GroupEnum groupsEnum, [Summary(name: "Пользователь", description: "Пользователь")] IUser user)
        {
            if (user.IsBot)
            {
                await RespondAsync("Вы выбрали бота", ephemeral: true);
                return;
            }

            if (groupsEnum == GroupEnum.Loner)
            {
                await RespondAsync("Для выписки из группировки используйте комманду /removegroupmember", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            CurrentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);

            //Создаем или получаем запрошенную группировку на текущем дискорд-сервере
            var requestedGroup = _groupService.GetAll().FirstOrDefault(x => x.GroupType == groupsEnum);
            if (requestedGroup == null)
            {
                requestedGroup = new GroupEntity()
                {
                    GroupType = groupsEnum,
                    Guild = CurrentGuild,
                };
                await _groupService.CreateOrUpdateAsync(requestedGroup);
            }

            //Создаем или получаем данные игрока
            var guildMember = await _guildMemberService.GetByDiscordIdAsync(user.Id, CurrentGuild.GuildId);
            if (guildMember == null)
            {
                var member = await _memberService.GetByDiscordIdAsync(user.Id);
                member ??= await _memberService.CreateMemberAsync(user.Id, CurrentGuild);
                guildMember = await _guildMemberService.GetByMemberAsync(member, CurrentGuild);
            }

            var currentGroup = guildMember.Group;

            if ((currentGroup != null && currentGroup.GroupType == GroupEnum.Loner) || currentGroup == null)
            {
                guildMember.Group = requestedGroup;
                await _guildMemberService.UpdateAsync(guildMember);

                await FollowupAsync($"Игрок вписан в группировку {groupsEnum.GetDescription()}", ephemeral: true);
                return;
            }

            await FollowupAsync($"Игрок принадлежит к группировке {currentGroup.GroupType.GetDescription()}, сначала необходимо его выписать", ephemeral: true);
            return;
        }

        [SlashCommand("выписать", "Выписать игрока из группировки")]
        public async Task RemoveFromGroup([Summary(name: "Группировка", description: "Название группировки")] GroupEnum group, [Summary(name: "Пользователь", description: "Пользователь")] IUser user)
        {
            if (user.IsBot)
            {
                await RespondAsync("Вы выбрали бота", ephemeral: true);
                return;
            }

            if (group == GroupEnum.Loner)
            {
                await RespondAsync("Невозможно выписать игрока из одиночек.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            CurrentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);

            var guildMember = await _guildMemberService.GetByDiscordIdAsync(user.Id, CurrentGuild.GuildId);
            if (guildMember == null)
            {
                await FollowupAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                var member = await _memberService.GetByDiscordIdAsync(user.Id);
                if (member == null)
                    await _memberService.CreateMemberAsync(user.Id, CurrentGuild);
                return;
            }

            var currentGroup = guildMember.Group;
            if (currentGroup.GroupType == GroupEnum.Loner)
            {
                await FollowupAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            var lonerGroup = await _groupService.GetGroupByEnum(GroupEnum.Loner, CurrentGuild);

            guildMember.Group = lonerGroup;
            await _groupService.UpdateAsync(currentGroup);

            await FollowupAsync($"Игрок успешно выписан из групппировки {group.GetDescription()}", ephemeral: true);
        }

        [SlashCommand("лидер", "Установить лидера группировки")]
        public async Task SetLeader([Summary(name: "Группировка", description: "Название группировки")] GroupEnum groupsEnum, [Summary(name: "Лидер", description: "Пользователь")] IUser user)
        {
            if (user.IsBot)
            {
                await RespondAsync("Вы выбрали бота", ephemeral: true);
                return;
            }

            if (groupsEnum == GroupEnum.Loner)
            {
                await RespondAsync("Нельзя установить лидера группировки \"Одиночки\"", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            CurrentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);

            var requestedGroup = _groupService.GetAll().FirstOrDefault(x => x.GroupType == groupsEnum);
            if (requestedGroup == null)
            {
                await FollowupAsync("В группировке нет ни одного игрока, впишите игрока в группировку, чтобы сделать его лидером", ephemeral: true);
                return;
            }

            var guildMember = await _guildMemberService.GetByDiscordIdAsync(user.Id, CurrentGuild.GuildId);
            if (guildMember == null)
            {
                await FollowupAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                var member = await _memberService.GetByDiscordIdAsync(user.Id);
                if (member == null)
                    await _memberService.CreateMemberAsync(user.Id, CurrentGuild);
                return;
            }

            var currentGroup = guildMember.Group;
            if (currentGroup == null || currentGroup.GroupType == GroupEnum.Loner)
            {
                await FollowupAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            if (currentGroup.GroupType != groupsEnum)
            {
                await FollowupAsync("Игрок не состоит в данной группировке", ephemeral: true);
                return;
            }

            currentGroup.Leader = guildMember.Id;
            await _groupService.UpdateAsync(currentGroup);
            await FollowupAsync($"Игрок успешно установлен как лидер группировки {groupsEnum.GetDescription()}", ephemeral: true);
        }

        [SlashCommand("расформ", "Расформировать группировку")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DisbandGroup([Summary(name: "Группировка", description: "Название группировки")] GroupEnum groupsEnum)
        {
            if (groupsEnum == GroupEnum.Loner)
            {
                await RespondAsync("Нельзя расформировать одиночек!", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            CurrentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            var group = await _groupService.GetGroupByEnum(groupsEnum, CurrentGuild);

            var groupMembers = _guildMemberService.GetAll().Where(x => x.Guild.GuildId == CurrentGuild.GuildId && x.Group.GroupType == groupsEnum).ToList();
            if (groupMembers == null || !groupMembers.Any())
            {
                await FollowupAsync($"В группировке \"{groupsEnum.GetDescription()}\" нет игроков", ephemeral: true);
                return;
            }

            var lonerGroup = await _groupService.GetGroupByEnum(GroupEnum.Loner, CurrentGuild);
            foreach (var groupMember in groupMembers)
            {
                groupMember.Group = lonerGroup;
                await _guildMemberService.UpdateAsync(groupMember);
            }

            group.Leader = null;

            await _groupService.UpdateAsync(group);
            await FollowupAsync($"Группировка {groupsEnum.GetDescription()} расформирована", ephemeral: true);
        }

        [SlashCommand("состав", "Состав группировки")]
        public async Task GetGroupMembers([Summary(name: "Группировка", description: "Название группировки")] GroupEnum groupsEnum)
        {
            if (groupsEnum == GroupEnum.Loner)
                await RespondAsync("Одиночка - это не группировка");

            await DeferAsync(ephemeral: false);

            CurrentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            var group = await _groupService.GetGroupByEnum(groupsEnum, CurrentGuild);
            if (group == null)
            {
                await FollowupAsync($"В группировке \"{groupsEnum.GetDescription()}\" нет игроков", ephemeral: false);
                return;
            }

            var groupMembers = _guildMemberService.GetAll().Where(x => x.Guild.GuildId == CurrentGuild.GuildId)
                                                           .Where(x => x.Group.GroupType == groupsEnum)
                                                           .Where(x => x.Member != null)
                                                           .ToList();

            if (groupMembers == null || !groupMembers.Any())
            {
                await FollowupAsync($"В группировке \"{groupsEnum.GetDescription()}\" нет игроков", ephemeral: false);
                return;
            }

            var memberNames = string.Empty;
            var i = 0;
            var nullmembers = string.Empty;

            foreach (var groupMember in groupMembers)
            {
                i++;
                var member = groupMember.Member;
                var discordUser = Context.Guild.GetUser(member.DiscordId);
                if(discordUser == null)
                {
                    var lonerGroup = await _groupService.GetGroupByEnum(GroupEnum.Loner, CurrentGuild);
                    groupMember.Group = lonerGroup;
                    await _guildMemberService.UpdateAsync(groupMember);
                    nullmembers += groupMember.Member.DiscordId + " ";
                    continue;
                }
                memberNames += string.Format("{0}. {1} ({2}) \n", i.ToString(), MentionUtils.MentionUser(discordUser.Id), discordUser.Username);
            }

            var groupEmbed = new EmbedBuilder()
                .WithTitle($"Группировка {GetValueExtension.GetDescription(groupsEnum)}");

            if (group.CuratorId != null && group.CuratorId != 0)
            {
                var curatorGuildMember = await _guildMemberService.Get(group.CuratorId.Value);
                if (curatorGuildMember != null)
                {
                    var groupCuratorDiscrodUser = Context.Guild.GetUser(curatorGuildMember.Member.DiscordId);
                    groupEmbed.AddField("Куратор группировки", string.Format("{0} ({1})", MentionUtils.MentionUser(groupCuratorDiscrodUser.Id), groupCuratorDiscrodUser.Username));
                }
            }

            if (group.Leader != null)
            {
                var groupLeaderGuildMember = await _guildMemberService.Get(group.Leader.Value);
                var groupLeaderDiscrodUser = Context.Guild.GetUser(groupLeaderGuildMember.Member.DiscordId);
                groupEmbed.AddField("Лидер группировки", string.Format("{0} ({1})", MentionUtils.MentionUser(groupLeaderDiscrodUser.Id), groupLeaderDiscrodUser.Username));
            }

            groupEmbed.AddField("Состав:", memberNames)
                      .WithColor(Color.Blue);

            if(string.IsNullOrEmpty(nullmembers))
                await FollowupAsync(embed: groupEmbed.Build(), ephemeral: false);
            else
                await FollowupAsync(text: string.Format("Спискок ненайденных пользователей: {0}. Они были удалены из группировки.", nullmembers),embed: groupEmbed.Build(), ephemeral: false);
        }

        [UserCommand("группировка игрока")]
        public async Task GetGroup(IUser user)
        {
            if (user.IsBot)
            {
                await RespondAsync("Вы выбрали бота", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            CurrentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);

            var guildMember = await _guildMemberService.GetByDiscordIdAsync(user.Id, CurrentGuild.GuildId);
            if (guildMember == null)
            {
                await FollowupAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                var member = await _memberService.GetByDiscordIdAsync(user.Id);
                if (member == null)
                    await _memberService.CreateMemberAsync(user.Id, CurrentGuild);
                return;
            }

            await FollowupAsync($"Игрок состоит в группировке \"{guildMember.Group.GroupType.GetDescription()}\"", ephemeral: true);
        }
    }
}
