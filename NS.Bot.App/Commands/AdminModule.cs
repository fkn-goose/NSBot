using Discord;
using Discord.Interactions;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Enums;

namespace NS.Bot.App.Commands
{
    [Group("admin", "Команды для админстрации")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class AdminModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IMemberService _memberService;
        private readonly IGuildService _guildService;
        private readonly IGuildMemberService _guildMemberService;
        private readonly IBaseService<GuildRoles> _guildRolesService;
        private readonly IGroupService _groupService;
        public AdminModule(IMemberService memberService, IGuildService guildService, IGuildMemberService guildMemberService, IBaseService<GuildRoles> guildRolesService, IGroupService groupService)
        {
            _memberService = memberService;
            _guildService = guildService;
            _guildMemberService = guildMemberService;
            _guildRolesService = guildRolesService;
            _groupService = groupService;
        }

        [SlashCommand("назначить", "Назначить пользователю роль")]
        public async Task SetAdmin([Summary("пользователь")] IGuildUser user, [Summary("роль")] RoleEnum role)
        {
            await DeferAsync(ephemeral: true);

            var currentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            if (currentGuild == null)
            {
                await FollowupAsync("Сервер не найден", ephemeral: true);
                return;
            }

            var rolesList = _guildRolesService.GetAll().FirstOrDefault(x => x.RelatedGuild.Id == currentGuild.Id);
            if (rolesList == null)
            {
                await FollowupAsync("Не найден список ролей", ephemeral: true);
                return;
            }

            var guildMember = await _guildMemberService.GetByDiscordIdAsync(user.Id, currentGuild.GuildId);
            if (guildMember == null)
            {
                var member = await _memberService.GetByDiscordIdAsync(user.Id);
                member ??= await _memberService.CreateMemberAsync(user.Id, currentGuild);
                guildMember = await _guildMemberService.GetByMemberAsync(member, currentGuild);
            }

            guildMember.Role = role;
            await _guildMemberService.UpdateAsync(guildMember);

            if (rolesList.AdminListMessageId != 0 || rolesList.AdminListMessageChannelId != 0)
            {
                var admins = _guildMemberService.GetAll().Where(x => x.Role != RoleEnum.Player).ToList();
                var channel = Context.Guild.GetTextChannel(rolesList.AdminListMessageChannelId);
                var newmsg = AdminListEmbed(admins, rolesList);

                await channel.ModifyMessageAsync(rolesList.AdminListMessageId, msg => { msg.Embeds = new Embed[] { newmsg }; });
            }

            switch (role)
            {
                case RoleEnum.Helper:
                    await user.AddRoleAsync(rolesList.HelperRoleId);
                    break;
                case RoleEnum.JuniorCurator:
                    await user.AddRoleAsync(rolesList.JuniorCuratorRoleId);
                    break;
                case RoleEnum.Curator:
                    await user.AddRoleAsync(rolesList.CuratorRoleId);
                    break;
                case RoleEnum.SeniorCurator:
                    await user.AddRoleAsync(rolesList.SeniorCuratorRoleId);
                    break;
                case RoleEnum.RPAdmin:
                    await user.AddRoleAsync(rolesList.RPAdminRoleId);
                    break;
                case RoleEnum.ChiefAdminDeputy:
                    await user.AddRoleAsync(rolesList.ChiefAdminDeputyRoleId);
                    break;
                case RoleEnum.ChiefAdmin:
                    await user.AddRoleAsync(rolesList.ChiefAdminRoleId);
                    break;
                case RoleEnum.JuniorEventmaster:
                    await user.AddRoleAsync(rolesList.JuniorEventmasterRoleId);
                    break;
                case RoleEnum.Eventmaster:
                    await user.AddRoleAsync(rolesList.EventmasterRoleId);
                    break;
                case RoleEnum.ChiefEventmaster:
                    await user.AddRoleAsync(rolesList.ChiefEventmasterRoleId);
                    break;
                case RoleEnum.Player:
                    await user.RemoveRolesAsync(new List<ulong>() { rolesList.HelperRoleId, rolesList.JuniorCuratorRoleId,
                                                                    rolesList.CuratorRoleId, rolesList.SeniorCuratorRoleId,
                                                                    rolesList.RPAdminRoleId, rolesList.ChiefAdminDeputyRoleId,
                                                                    rolesList.ChiefAdminRoleId, rolesList.JuniorEventmasterRoleId,
                                                                    rolesList.EventmasterRoleId, rolesList.ChiefEventmasterRoleId });
                    break;
            }

            await FollowupAsync("Игроку выдана роль");
        }

        [SlashCommand("список", "Отправить в канал сообщение со списком администрации")]
        public async Task SendAdminListMessage()
        {
            await DeferAsync(ephemeral: true);

            var currentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            if (currentGuild == null)
            {
                await FollowupAsync("Сервер не найден", ephemeral: true);
                return;
            }

            var rolesList = _guildRolesService.GetAll().FirstOrDefault(x => x.RelatedGuild.Id == currentGuild.Id);
            if (rolesList == null)
            {
                await FollowupAsync("Не найден список ролей", ephemeral: true);
                return;
            }

            var admins = _guildMemberService.GetAll().Where(x => x.Role != RoleEnum.Player).ToList();
            var embed = AdminListEmbed(admins, rolesList);

            var msg = await Context.Channel.SendMessageAsync(embed: embed);

            rolesList.AdminListMessageChannelId = Context.Channel.Id;
            rolesList.AdminListMessageId = msg.Id;
            await _guildRolesService.UpdateAsync(rolesList);

            await FollowupAsync("Список администрации отправлен", ephemeral: true);
        }

        [SlashCommand("обновить", "обновляет список администрации и сообщение")]
        public async Task UpdateAdminList()
        {
            await DeferAsync(ephemeral: true);

            var currentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            if (currentGuild == null)
            {
                await FollowupAsync("Сервер не найден", ephemeral: true);
                return;
            }

            var rolesList = _guildRolesService.GetAll().FirstOrDefault(x => x.RelatedGuild.Id == currentGuild.Id);
            if (rolesList == null)
            {
                await FollowupAsync("Не найден список ролей", ephemeral: true);
                return;
            }

            var chief = Context.Guild.GetRole(rolesList.ChiefAdminRoleId);
            var deputyChief = Context.Guild.GetRole(rolesList.ChiefAdminDeputyRoleId);
            var rpAdmin = Context.Guild.GetRole(rolesList.RPAdminRoleId);
            var seniorCurator = Context.Guild.GetRole(rolesList.SeniorCuratorRoleId);
            var curator = Context.Guild.GetRole(rolesList.CuratorRoleId);
            var juniorCurator = Context.Guild.GetRole(rolesList.JuniorCuratorRoleId);
            var helper = Context.Guild.GetRole(rolesList.HelperRoleId);
            var juniorEvent = Context.Guild.GetRole(rolesList.JuniorEventmasterRoleId);
            var even = Context.Guild.GetRole(rolesList.EventmasterRoleId);
            var chiefEven = Context.Guild.GetRole(rolesList.ChiefEventmasterRoleId);

            var discordAdmins = Context.Guild.Users.Where(x => x.Roles.Contains(chief) || x.Roles.Contains(deputyChief) || x.Roles.Contains(rpAdmin) || x.Roles.Contains(seniorCurator) || x.Roles.Contains(curator) || x.Roles.Contains(juniorCurator) || x.Roles.Contains(helper) || x.Roles.Contains(juniorEvent) || x.Roles.Contains(even) || x.Roles.Contains(chiefEven)).ToList();
            var admins = _guildMemberService.GetAll().Where(x => discordAdmins.Select(x => x.Id).Contains(x.Member.DiscordId)).ToList();
            var oldAdmins = _guildMemberService.GetAll().Where(x => x.Role != RoleEnum.Player).ToList();

            foreach (var oldAdmin in oldAdmins)
                if (!admins.Select(x => x.Id).Contains(oldAdmin.Id))
                {
                    oldAdmin.Role = RoleEnum.Player;
                    await _guildMemberService.UpdateAsync(oldAdmin);
                }

            foreach (var admin in admins)
            {
                if (admin.Role == RoleEnum.Player)
                {
                    //впадлу.
                }
            }

            if (rolesList.AdminListMessageId != 0 || rolesList.AdminListMessageChannelId != 0)
            {
                var channel = Context.Guild.GetTextChannel(rolesList.AdminListMessageChannelId);
                var newmsg = AdminListEmbed(admins, rolesList);

                await channel.ModifyMessageAsync(rolesList.AdminListMessageId, msg => { msg.Embeds = new Embed[] { newmsg }; });
            }
        }

        [SlashCommand("куратор_гп", "Назначить администратора куратором группировки")]
        public async Task SetCurator([Summary("пользователь")] IGuildUser user, GroupEnum inputGroup)
        {
            await DeferAsync(ephemeral: true);

            var currentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            if (currentGuild == null)
            {
                await FollowupAsync("Сервер не найден", ephemeral: true);
                return;
            }

            var group = await _groupService.GetGroupByEnum(inputGroup, currentGuild);
            if (group == null)
            {
                await FollowupAsync("Группировка не найдена", ephemeral: true);
                return;
            }

            var guildMember = await _guildMemberService.GetByDiscordIdAsync(user.Id, currentGuild.GuildId);
            if (guildMember == null)
            {
                var member = await _memberService.GetByDiscordIdAsync(user.Id);
                member ??= await _memberService.CreateMemberAsync(user.Id, currentGuild);
                guildMember = await _guildMemberService.GetByMemberAsync(member, currentGuild);
                await FollowupAsync("Пользователь не является курирующим администратором", ephemeral: true);
                return;
            }

            if (guildMember.Role == RoleEnum.Player || guildMember.Role == RoleEnum.Eventmaster || guildMember.Role == RoleEnum.JuniorEventmaster || guildMember.Role == RoleEnum.ChiefEventmaster)
            {
                await FollowupAsync("Пользователь не является курирующим администратором", ephemeral: true);
                return;
            }

            group.Curator = guildMember;
            await _groupService.UpdateAsync(group);
            await FollowupAsync("Куратор установлен", ephemeral: true);
        }

        private Embed AdminListEmbed(List<GuildMember> admins, GuildRoles roles)
        {
            var chiefList = string.Empty;
            var deputyChiefList = string.Empty;
            var rpAdminList = string.Empty;
            var seniorCuratorList = string.Empty;
            var curatorList = string.Empty;
            var juniorCuratorList = string.Empty;
            var helperList = string.Empty;
            var juniorEventList = string.Empty;
            var eventList = string.Empty;
            var chiefEventList = string.Empty;

            //Ну да, вопросы будут?
            foreach (var admin in admins)
            {
                switch (admin.Role)
                {
                    case RoleEnum.Helper:
                        helperList += string.Format("{0} - {1}\n", MentionUtils.MentionUser(admin.Member.DiscordId), admin.Member.SteamId);
                        break;
                    case RoleEnum.JuniorCurator:
                        juniorCuratorList += string.Format("{0} - {1}", MentionUtils.MentionUser(admin.Member.DiscordId), admin.Member.SteamId);
                        break;
                    case RoleEnum.Curator:
                        curatorList += string.Format("{0} - {1}", MentionUtils.MentionUser(admin.Member.DiscordId), admin.Member.SteamId);
                        break;
                    case RoleEnum.SeniorCurator:
                        seniorCuratorList += string.Format("{0} - {1}", MentionUtils.MentionUser(admin.Member.DiscordId), admin.Member.SteamId);
                        break;
                    case RoleEnum.RPAdmin:
                        rpAdminList += string.Format("{0} - {1}", MentionUtils.MentionUser(admin.Member.DiscordId), admin.Member.SteamId);
                        break;
                    case RoleEnum.ChiefAdminDeputy:
                        deputyChiefList += string.Format("{0} - {1}", MentionUtils.MentionUser(admin.Member.DiscordId), admin.Member.SteamId);
                        break;
                    case RoleEnum.ChiefAdmin:
                        chiefList += string.Format("{0} - {1}", MentionUtils.MentionUser(admin.Member.DiscordId), admin.Member.SteamId);
                        break;
                    case RoleEnum.JuniorEventmaster:
                        juniorEventList += string.Format("{0} - {1}", MentionUtils.MentionUser(admin.Member.DiscordId), admin.Member.SteamId);
                        break;
                    case RoleEnum.Eventmaster:
                        eventList += string.Format("{0} - {1}", MentionUtils.MentionUser(admin.Member.DiscordId), admin.Member.SteamId);
                        break;
                    case RoleEnum.ChiefEventmaster:
                        chiefEventList += string.Format("{0} - {1}", MentionUtils.MentionUser(admin.Member.DiscordId), admin.Member.SteamId);
                        break;
                }
            }

            EmbedBuilder adminlist = new EmbedBuilder()
                .WithTitle("Список администрации")
                .AddField(MentionUtils.MentionRole(roles.ChiefAdminRoleId), chiefList)
                .AddField(MentionUtils.MentionRole(roles.ChiefAdminDeputyRoleId), deputyChiefList)
                .AddField(MentionUtils.MentionRole(roles.RPAdminRoleId), rpAdminList)
                .AddField(MentionUtils.MentionRole(roles.SeniorCuratorRoleId), seniorCuratorList)
                .AddField(MentionUtils.MentionRole(roles.CuratorRoleId), curatorList)
                .AddField(MentionUtils.MentionRole(roles.JuniorCuratorRoleId), juniorCuratorList)
                .AddField(MentionUtils.MentionRole(roles.HelperRoleId), helperList)
                .AddField(MentionUtils.MentionRole(roles.ChiefEventmasterRoleId), chiefEventList)
                .AddField(MentionUtils.MentionRole(roles.EventmasterRoleId), eventList)
                .AddField(MentionUtils.MentionRole(roles.JuniorEventmasterRoleId), juniorEventList)
                .WithFooter(new EmbedFooterBuilder().WithText($"Последнее обновление {DateTime.Now.ToString()}"));

            return adminlist.Build();
        }
    }
}
