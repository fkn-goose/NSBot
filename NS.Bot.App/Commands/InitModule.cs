using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.BuisnessLogic.Services;
using NS.Bot.Shared.Entities.Group;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Entities.Warn;

namespace NS.Bot.App.Commands
{
    [RequireOwner]
    public class InitModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IGuildService _guildService;
        private readonly IBaseService<GroupEntity> _groupService;
        private readonly IWarnSettingsService _warnSettingsService;
        public InitModule(IGuildService guildService, IBaseService<GroupEntity> groupService, IWarnSettingsService warnSettingsService)
        {
            _guildService = guildService;
            _groupService = groupService;
            _warnSettingsService = warnSettingsService;
        }

        [SlashCommand("init", "Инициализация сервера")]
        public async Task InitServer()
        {
            GuildEntity? curGuild = _guildService.GetAll().FirstOrDefault(x => x.GuildId == Context.Guild.Id);
            if(curGuild != null)
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
                GroupType = Shared.Enums.GroupsEnum.Loner,
                Guild = curGuild
            });

            await FollowupAsync("Сервер инициализирован", ephemeral: true);
        }
        //Добавить создание группировки одиночек

        [SlashCommand("инициализация", "Настройка предов")]
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
            await FollowupAsync("Настройки сохранены", ephemeral: true);
        }
    }
}
