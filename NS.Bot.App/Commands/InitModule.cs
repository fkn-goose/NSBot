using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Group;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Entities.Radio;
using NS.Bot.Shared.Entities.Warn;

namespace NS.Bot.App.Commands
{
    [RequireOwner]
    public class InitModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IGuildService _guildService;
        private readonly IBaseService<GroupEntity> _groupService;
        private readonly IWarnSettingsService _warnSettingsService;
        private readonly IRadioSettingsService _radioSettingsService;
        private readonly IBaseService<GuildRoles> _guildRolesService;
        public InitModule(IGuildService guildService, IBaseService<GroupEntity> groupService, IWarnSettingsService warnSettingsService, IRadioSettingsService radioSettingsService, IBaseService<GuildRoles> guildRolesService)
        {
            _guildService = guildService;
            _groupService = groupService;
            _warnSettingsService = warnSettingsService;
            _radioSettingsService = radioSettingsService;
            _guildRolesService = guildRolesService;
        }

        [SlashCommand("serverinit", "Инициализация сервера")]
        public async Task InitServer()
        {
            GuildEntity? curGuild = _guildService.GetAll().FirstOrDefault(x => x.GuildId == Context.Guild.Id);
            if (curGuild != null)
            {
                await RespondAsync("Сервер уже инициализирован", ephemeral: true);
                return;
            }
            await DeferAsync(ephemeral: true);

            await _guildService.CreateOrUpdateAsync(curGuild = new GuildEntity
            {
                GuildId = Context.Guild.Id,
                Name = Context.Guild.Name,
            });

            await _groupService.CreateOrUpdateAsync(new GroupEntity
            {
                GroupType = Shared.Enums.GroupEnum.Loner,
                Guild = curGuild
            });

            await FollowupAsync("Сервер инициализирован", ephemeral: true);
        }
        //Добавить создание группировки одиночек

        [SlashCommand("warninit", "Инициализация предов")]
        [RequireOwner]
        public async Task InitWarnSettings([Summary("Канал_предов")] ITextChannel warnChannel, [Summary("Первый")] IRole firstRole, [Summary("Второй")] IRole secondRole, [Summary("Третий")] IRole thirdRole, 
                                           [Summary("Бан")] IRole banRole, [Summary("Ридонли")] IRole readonlyRole, 
                                           [Summary("Выговор1")] IRole firstRebukeRole, [Summary("Выговор2")] IRole secondRebukeRole, [Summary("Выговор3")] IRole thirdRebukeRole)
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
                FirstRebukeRoleId = firstRebukeRole.Id,
                SecondRebukeRoleId = secondRebukeRole.Id,
                ThirdRebukeRoleId = thirdRebukeRole.Id,
                BanRoleId = banRole.Id,
                ReadOnlyRoleId = readonlyRole.Id
            };

            await _warnSettingsService.CreateOrUpdateAsync(settings);
            await FollowupAsync("Настройки сохранены", ephemeral: true);
        }

        [SlashCommand("radioinit", "Инициализация радио канала")]
        [RequireOwner]
        public async Task InitRadioChannel()
        {
            await DeferAsync(ephemeral: true);
            var settings = new RadioSettings();

            settings = await _radioSettingsService.GetRadioSettingsAsync(Context.Guild.Id);
            var category = ((SocketTextChannel)Context.Channel).Category;
            settings.CommandChannelId = Context.Channel.Id;
            settings.RadiosCategoryId = category.Id;
            settings.IsEnabled = true;

            await _radioSettingsService.UpdateAsync(settings);

            await Context.Channel.SendMessageAsync("Для создания или подключения к частоте **напишите** команду /частота");
            await FollowupAsync("Канал выбран как создание частот", ephemeral: true);
        }

        [SlashCommand("rolesinit", "Инициализация ролей")]
        [RequireOwner]
        public async Task InitRoles([Summary("игрок")] IRole player, [Summary("хелпер")] IRole helper, [Summary("младший_куратор")] IRole juniorCurator,
                                    [Summary("куратор")] IRole curator, [Summary("старший_куратор")] IRole seniorCurator, [Summary("рп_админ")] IRole rpAdmin,
                                    [Summary("зга")] IRole deputy, [Summary("га")] IRole chief, [Summary("младший_ивентолог")] IRole juniorEvent,
                                    [Summary("ивентолог")] IRole eventmaster, [Summary("гланвый_ивентолог")] IRole chiefEvent)
        {
            await DeferAsync(ephemeral: true);

            var guild = await _guildService.GetByDiscordId(Context.Guild.Id);
            if (guild == null)
                return;

            GuildRoles roles = new GuildRoles()
            {
                RelatedGuild = guild,
                PlayerRoleId = player.Id,
                HelperRoleId = helper.Id,
                JuniorCuratorRoleId = juniorCurator.Id,
                CuratorRoleId = curator.Id,
                SeniorCuratorRoleId = seniorCurator.Id,
                RPAdminRoleId = rpAdmin.Id,
                ChiefAdminDeputyRoleId = deputy.Id,
                ChiefAdminRoleId = chief.Id,
                JuniorEventmasterRoleId = juniorEvent.Id,
                EventmasterRoleId = eventmaster.Id,
                ChiefEventmasterRoleId = chiefEvent.Id
            };

            await _guildRolesService.CreateOrUpdateAsync(roles);

            await FollowupAsync("Роли сохранены", ephemeral: true);
        }
    }
}
