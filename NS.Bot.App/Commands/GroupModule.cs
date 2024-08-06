using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities;
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
        public async Task AddToGroup([Summary(name: "Группировка", description: "Название группировки")] GroupsEnum groupsEnum, [Summary(name: "Пользователь", description: "Пользователь")] IUser user)
        {
            if (user.IsBot)
            {
                await RespondAsync("Вы выбрали бота", ephemeral: true);
                return;
            }

            if (groupsEnum == GroupsEnum.Loner)
            {
                await RespondAsync("Для выписки из группировки используйте комманду /removegroupmember", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            CurrentGuild = _guildService.GetByDiscordId(Context.Guild.Id).Result;

            //Создаем или получаем запрошенную группировку на текущем дискорд-сервере
            var requestedGroup = _groupService.GetAll().FirstOrDefault(x => x.Name == groupsEnum);
            if (requestedGroup == null)
            {
                requestedGroup = new GroupEntity()
                {
                    Name = groupsEnum,
                    Guild = CurrentGuild,
                };
                await _groupService.CreateOrUpdate(requestedGroup);
            }

            //Создаем или получаем данные игрока
            var member = _memberService.GetAll().FirstOrDefault(x => user.Id == x.DiscordId);
            var guildMember = new GuildMember();
            if (member == null)
            {
                member = await CreateMember(user.Id);
                guildMember = await CreateGuildMember(member);
            }
            else
            {
                //Создаем или получаем члена сервера
                guildMember = _guildMemberService.GetByMember(member, CurrentGuild).Result;
                guildMember ??= await CreateGuildMember(member);
            }

            var currentGroup = guildMember?.Group;

            if ((currentGroup != null && currentGroup.Name == GroupsEnum.Loner) || currentGroup == null)
            {
                //null быть не может, оно пиздит
                guildMember.Group = requestedGroup;
                await _guildMemberService.Update(guildMember);

                await FollowupAsync($"Игрок вписан в группировку {groupsEnum.GetDescription()}", ephemeral: true);
                return;
            }

            await FollowupAsync($"Игрок принадлежит к группировке {currentGroup.Name.GetDescription()}, сначала необходимо его выписать", ephemeral: true);
            return;
        }

        [SlashCommand("выписать", "Выписать игрока из группировки")]
        public async Task RemoveFromGroup([Summary(name: "Группировка", description: "Название группировки")] GroupsEnum group, [Summary(name: "Пользователь", description: "Пользователь")] IUser user)
        {
            if (user.IsBot)
            {
                await RespondAsync("Вы выбрали бота", ephemeral: true);
                return;
            }

            if (group == GroupsEnum.Loner)
            {
                await RespondAsync("Невозможно выписать игрока из одиночек.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            CurrentGuild = _guildService.GetByDiscordId(Context.Guild.Id).Result;

            var member = _memberService.GetAll().FirstOrDefault(x => user.Id == x.DiscordId);
            var guildMember = new GuildMember();
            if (member == null)
            {
                member = await CreateMember(user.Id);
                guildMember = await CreateGuildMember(member);
                await FollowupAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            guildMember = _guildMemberService.GetByMember(member, CurrentGuild).Result;
            if (guildMember == null)
            {
                guildMember = await CreateGuildMember(member);
                await FollowupAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            var currentGroup = guildMember.Group;
            if (currentGroup.Name == GroupsEnum.Loner)
            {
                await FollowupAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            var lonerGroup = await _groupService.GetGroupByEnum(GroupsEnum.Loner, CurrentGuild);

            guildMember.Group = lonerGroup;
            await _groupService.Update(currentGroup);

            await FollowupAsync($"Игрок успешно выписан из групппировки {group.GetDescription()}", ephemeral: true);
        }

        [SlashCommand("лидер", "Установить лидера группировки")]
        public async Task SetLeader([Summary(name: "Группировка", description: "Название группировки")] GroupsEnum groupsEnum, [Summary(name: "Лидер", description: "Пользователь")] IUser user)
        {
            if (user.IsBot)
            {
                await RespondAsync("Вы выбрали бота", ephemeral: true);
                return;
            }

            if (groupsEnum == GroupsEnum.Loner)
            {
                await RespondAsync("Нельзя установить лидера группировки \"Одиночки\"", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            CurrentGuild = _guildService.GetByDiscordId(Context.Guild.Id).Result;

            var requestedGroup = _groupService.GetAll().FirstOrDefault(x => x.Name == groupsEnum);
            if (requestedGroup == null)
            {
                await FollowupAsync("В группировке нет ни одного игрока, впишите игрока в группировку, чтобы сделать его лидером", ephemeral: true);
                return;
            }

            var member = _memberService.GetAll().FirstOrDefault(x => user.Id == x.DiscordId);
            var guildMember = new GuildMember();
            if (member == null)
            {
                member = await CreateMember(user.Id);
                guildMember = await CreateGuildMember(member);
                await FollowupAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            guildMember = await _guildMemberService.GetByMember(member, CurrentGuild);
            if (guildMember == null)
            {
                guildMember = await CreateGuildMember(member);
                await FollowupAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            var currentGroup = guildMember.Group;
            if (currentGroup == null || currentGroup.Name == GroupsEnum.Loner)
            {
                await FollowupAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            if (currentGroup.Name != groupsEnum)
            {
                await FollowupAsync("Игрок не состоит в данной группировке", ephemeral: true);
                return;
            }

            currentGroup.Leader = guildMember.Id;
            await _groupService.Update(currentGroup);
            await FollowupAsync($"Игрок успешно установлен как лидер группировки {groupsEnum.GetDescription()}", ephemeral: true);
        }

        [SlashCommand("расформ", "Расформировать группировку")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DisbandGroup([Summary(name: "Группировка", description: "Название группировки")] GroupsEnum groupsEnum)
        {
            if(groupsEnum == GroupsEnum.Loner)
            {
                await RespondAsync("Нельзя расформировать одиночек!", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            CurrentGuild = _guildService.GetByDiscordId(Context.Guild.Id).Result;
            var group = await _groupService.GetGroupByEnum(groupsEnum, CurrentGuild);

            var groupMembers = _guildMemberService.GetAll().Where(x => x.Guild.GuildId == CurrentGuild.GuildId && x.Group.Name == groupsEnum).ToList();
            if (groupMembers == null || !groupMembers.Any())
            {
                await FollowupAsync($"В группировке \"{groupsEnum.GetDescription()}\" нет игроков", ephemeral: true);
                return;
            }

            var lonerGroup = await _groupService.GetGroupByEnum(GroupsEnum.Loner, CurrentGuild);
            foreach (var groupMember in groupMembers)
            {
                groupMember.Group = lonerGroup;
                await _guildMemberService.Update(groupMember);
            }

            group.Leader = null;

            await _groupService.Update(group);
            await FollowupAsync($"Группировка {groupsEnum.GetDescription()} расформирована", ephemeral: true);
        }

        [SlashCommand("состав", "Состав группировки")]
        public async Task GetGroupMembers([Summary(name: "Группировка", description: "Название группировки")] GroupsEnum groupsEnum)
        {
            await DeferAsync(ephemeral: false);

            CurrentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            var group = await _groupService.GetGroupByEnum(groupsEnum, CurrentGuild);
            if (group == null)
            {
                await FollowupAsync($"В группировке \"{groupsEnum.GetDescription()}\" нет игроков", ephemeral: false);
                return;
            }

            var groupMembers = _guildMemberService.GetAll().Where(x => x.Guild.GuildId == CurrentGuild.GuildId)
                                                           .Where(x => x.Group.Name == groupsEnum)
                                                           .Where(x => x.Member != null)
                                                           .ToList();

            if (groupMembers == null || !groupMembers.Any())
            {
                await FollowupAsync($"В группировке \"{groupsEnum.GetDescription()}\" нет игроков", ephemeral: false);
                return;
            }

            var memberNames = string.Empty;
            var i = 0;

            foreach (var groupMember in groupMembers)
            {
                i++;
                var member = groupMember.Member;
                var discordUser = Context.Guild.GetUser(member.DiscordId);
                memberNames += string.Format("{0}. {1} ({2}) \n", i.ToString(), MentionUtils.MentionUser(discordUser.Id), discordUser.Username);
            }

            var groupEmbed = new EmbedBuilder()
                .WithTitle($"Группировка {GetValueExtension.GetDescription(groupsEnum)}");
            SocketGuildUser groupLeaderDiscrodUser;
            if (group.Leader != null)
            {
                var groupLeaderGuildMember = await _guildMemberService.Get(group.Leader.Value);
                groupLeaderDiscrodUser = Context.Guild.GetUser(groupLeaderGuildMember.Member.DiscordId);
                groupEmbed.AddField("Лидер группировки", string.Format("{0} ({1})", MentionUtils.MentionUser(groupLeaderDiscrodUser.Id), groupLeaderDiscrodUser.Username));
            }

            groupEmbed.AddField("Состав:", memberNames)
                      .WithColor(Color.Blue);
            
            await FollowupAsync(embed: groupEmbed.Build(), ephemeral: false);
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

            CurrentGuild = _guildService.GetByDiscordId(Context.Guild.Id).Result;

            var member = await _memberService.GetAll().FirstOrDefaultAsync(x => x.DiscordId == user.Id);
            if (member == null)
            {
                await FollowupAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            var guildMember = await _guildMemberService.GetByMember(member, CurrentGuild);
            if (guildMember == null || guildMember.Group == null || guildMember.Group.Name == GroupsEnum.Loner)
            {
                await FollowupAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            await FollowupAsync($"Игрок состоит в группировке \"{guildMember.Group.Name.GetDescription()}\"", ephemeral: true);
        }

        private async Task<MemberEntity> CreateMember(ulong userId)
        {
            var member = new MemberEntity()
            {
                DiscordId = userId
            };
            await _memberService.CreateOrUpdate(member);

            return member;
        }

        private async Task<GuildMember> CreateGuildMember(MemberEntity member)
        {
            var guildMember = new GuildMember()
            {
                Guild = CurrentGuild,
                Member = member
            };
            await _guildMemberService.CreateOrUpdate(guildMember);

            return guildMember;
        }
    }
}
