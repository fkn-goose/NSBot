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

        private readonly IBaseService<GuildMember> _guildMemberService;

        private readonly GuildEntity CurrentGuild;
        public GroupModule(IGroupService groupService, IMemberService memberService, IGuildService guildService, IBaseService<GuildMember> guildMemberService)
        {
            _groupService = groupService;
            _memberService = memberService;
            _guildService = guildService;

            _guildMemberService = guildMemberService;

            CurrentGuild = _guildService.GetByDiscordId(Context.Guild.Id).Result;
        }

        [SlashCommand("addgroupmember", "Вписать игрока в группировку")]
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

            await DeferAsync();

            //Создаем или получаем запрошенную группировку на текущем дискорд-сервере
            var requestedGroup = _groupService.GetAll().FirstOrDefault(x => x.Group == groupsEnum);
            if (requestedGroup == null)
                await _groupService.Create(requestedGroup = new GroupEntity()
                {
                    Group = groupsEnum,
                    Guild = CurrentGuild,
                });

            //Создаем или получаем игрока сервера
            var member = _memberService.GetAll().FirstOrDefault(x => user.Id == x.DiscordId);
            var guildMember = new GuildMember();
            if (member == null)
                CreateMember(ref guildMember, ref member, user.Id);
            else
            {
                //Создаем или получаем члена сервера
                guildMember = _memberService.GetCurrentGuildMember(member, CurrentGuild);
                if (guildMember == null)
                    CreateGuildMember(ref guildMember);
            }

            var currentGroup = _groupService.GetGuildMembersGroup(guildMember).Result;
            if (currentGroup.Group != GroupsEnum.Loner)
            {
                await RespondAsync($"Игрок принадлежит к группировке {currentGroup.Group.GetDescription()}, сначала необходимо его выписать", ephemeral: true);
                return;
            }

            currentGroup?.Groupmembers.Remove(guildMember);
            await _groupService.Update(currentGroup);

            requestedGroup.Groupmembers.Add(guildMember);
            await _groupService.Update(requestedGroup);

            await RespondAsync($"Игрок вписан в группировку {groupsEnum.GetDescription()}", ephemeral: true);
            return;
        }

        [SlashCommand("removegroupmember", "Выписать игрока из группировки")]
        public async Task RemoveFromGroup([Summary(name: "Группировка", description: "Название группировки")] GroupsEnum groupsEnum, [Summary(name: "Пользователь", description: "Пользователь")] IUser user)
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

            await DeferAsync();

            var requestedGroup = _groupService.GetAll().FirstOrDefault(x => x.Group == groupsEnum);
            if (requestedGroup == null)
            {
                await RespondAsync("В группировке нет ни одного игрока", ephemeral: true);
                return;
            }

            var member = _memberService.GetAll().FirstOrDefault(x => user.Id == x.DiscordId);
            var guildMember = new GuildMember();
            if (member == null)
            {
                CreateMember(ref guildMember, ref member, user.Id);
                await RespondAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            guildMember = _memberService.GetCurrentGuildMember(member, CurrentGuild);
            if (guildMember == null)
            {
                CreateGuildMember(ref guildMember);
                await RespondAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            var currentGroup = _groupService.GetGuildMembersGroup(guildMember);
            if (currentGroup.Result.Group == GroupsEnum.Loner)
            {
                await RespondAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            var lonerGroup = _groupService.GetAll().FirstOrDefault(x => x.Group == GroupsEnum.Loner);
            if (lonerGroup != null)
            {
                lonerGroup.Groupmembers.Add(guildMember);
                await _groupService.Update(lonerGroup);
            }
            else
                await _groupService.Create(lonerGroup = new GroupEntity()
                {
                    Group = GroupsEnum.Loner,
                    Guild = CurrentGuild,
                    Groupmembers = new List<GuildMember>() { guildMember }
                });

            currentGroup.Result.Groupmembers.Remove(guildMember);
            await _groupService.Update(currentGroup.Result);

            await RespondAsync($"Игрок успешно выписан из групппировки {groupsEnum.GetDescription()}", ephemeral: true);
        }

        [SlashCommand("setleader", "Установить лидера группировки")]
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

            await DeferAsync();

            var requestedGroup = _groupService.GetAll().FirstOrDefault(x => x.Group == groupsEnum);
            if (requestedGroup == null)
            {
                await RespondAsync("В группировке нет ни одного игрока, впишите игрока в группировку, чтобы сделать его лидером", ephemeral: true);
                return;
            }

            var member = _memberService.GetAll().FirstOrDefault(x => user.Id == x.DiscordId);
            var guildMember = new GuildMember();
            if (member == null)
            {
                CreateMember(ref guildMember, ref member, user.Id);
                await RespondAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            guildMember = _memberService.GetCurrentGuildMember(member, CurrentGuild);
            if (guildMember == null)
            {
                CreateGuildMember(ref guildMember);
                await RespondAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            var currentGroup = _groupService.GetGuildMembersGroup(guildMember);
            if (currentGroup.Result.Group == GroupsEnum.Loner)
            {
                await RespondAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            if(currentGroup.Result.Group != groupsEnum)
            {
                await RespondAsync("Игрок не состоит в данной группировке", ephemeral: true);
                return;
            }

            currentGroup.Result.Leader = guildMember;
            await _groupService.Update(currentGroup.Result);
            await RespondAsync($"Игрок успешно установлен как лидер группировки {groupsEnum.GetDescription()}", ephemeral: true);
        }

        [SlashCommand("disbandgroup", "Расформировать группировку")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DisbandGroup([Summary(name: "Группировка", description: "Название группировки")] GroupsEnum groupsEnum)
        {
            var group = _groupService.GetGroupByEnum(groupsEnum, CurrentGuild);
            if(group.Result == null)
            {
                await RespondAsync("Группировка пуста", ephemeral: true);
                return;
            }

            if (group.Result.Groupmembers == null || !group.Result.Groupmembers.Any())
            {
                await RespondAsync("Группировка пуста", ephemeral: true);
                return;
            }

            await DeferAsync();

            group.Result.Groupmembers = null;
            group.Result.Leader = null;
            await _groupService.Update(group.Result);

            await RespondAsync($"Группировка {groupsEnum.GetDescription()} расформирована", ephemeral: true);
        }

        [SlashCommand("groupmembers", "Список игроков группировки")]
        public async Task GetGroupMembers([Summary(name: "Группировка", description: "Название группировки")] GroupsEnum groupsEnum)
        {
            var group = _groupService.GetGroupByEnum(groupsEnum, CurrentGuild);
            if (group.Result == null)
            {
                await RespondAsync("Группировка пуста", ephemeral: true);
                return;
            }

            if(group.Result.Groupmembers == null || !group.Result.Groupmembers.Any())
            {
                await RespondAsync("Группировка пуста", ephemeral: true);
                return;
            }

            await DeferAsync();

            var memberNames = string.Empty;
            var i = 0;

            foreach (var groupMember in group.Result.Groupmembers)
            {
                i++;
                var member = _memberService.GetMemberByGuildMember(groupMember);
                var discordUser = Context.Guild.GetUser(member.Result.DiscordId);
                memberNames += string.Format("{0}. {1} ({2}) \n", i.ToString(), MentionUtils.MentionUser(discordUser.Id), discordUser.Username);
            }

            var groupEmbed = new EmbedBuilder()
                .WithTitle($"Состав группировки - {GetValueExtension.GetDescription(groupsEnum)}");
            SocketGuildUser groupLeaderDiscrodUser;
            if (group.Result.Leader != null)
            {
                groupLeaderDiscrodUser = Context.Guild.GetUser(_memberService.GetMemberByGuildMember(group.Result.Leader).Result.DiscordId);
                groupEmbed.AddField("Лидер группировки", string.Format("{0} {1}", MentionUtils.MentionUser(groupLeaderDiscrodUser.Id), groupLeaderDiscrodUser.Username));
            }

            groupEmbed.AddField("Состав группировки", memberNames)
                      .WithColor(Color.Blue);

            await RespondAsync(embed: groupEmbed.Build());
        }

        [UserCommand("getgroup")]
        public async Task GetGroup(IUser user)
        {
            if (user.IsBot)
            {
                await RespondAsync("Вы выбрали бота", ephemeral: true);
                return;
            }

            var member = await _memberService.GetAll().FirstOrDefaultAsync(x => x.DiscordId == user.Id);
            if(member == null)
            {
                await RespondAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            var guildMember = _memberService.GetCurrentGuildMember(member, CurrentGuild);
            if(guildMember == null)
            {
                await RespondAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            var group = _groupService.GetGuildMembersGroup(guildMember);
            if(group.Result == null) 
            {
                await RespondAsync("Игрок не состоит ни в одной группировке", ephemeral: true);
                return;
            }

            await RespondAsync($"Игрок состоит в группировке \"{group.Result.Group.GetDescription()}\"", ephemeral: true);
        }

        private void CreateMember(ref GuildMember? guildMember, ref MemberEntity? member, ulong userId)
        {
            CreateGuildMember(ref guildMember);

            member = new MemberEntity()
            {
                DiscordId = userId,
                GuildMembers = new List<GuildMember> { guildMember }
            };

            _memberService.Create(member);
        }

        private void CreateGuildMember(ref GuildMember? guildMember)
        {
            guildMember = new GuildMember()
            {
                Guild = CurrentGuild,
            };
            _guildMemberService.Create(guildMember);
        }
    }
}
