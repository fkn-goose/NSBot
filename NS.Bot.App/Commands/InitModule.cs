using Discord.Interactions;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Group;
using NS.Bot.Shared.Entities.Guild;

namespace NS.Bot.App.Commands
{
    public class InitModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IBaseService<GuildEntity> _guildService;
        private readonly IBaseService<GroupEntity> _groupService;
        public InitModule(IBaseService<GuildEntity> guildService, IBaseService<GroupEntity> groupService)
        {
            _guildService = guildService;
            _groupService = groupService;
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

            await _guildService.CreateOrUpdate(curGuild = new GuildEntity
            {
                GuildId = Context.Guild.Id,
                Name = Context.Guild.Name,
            });

            await _groupService.CreateOrUpdate(new GroupEntity
            {
                Name = Shared.Enums.GroupsEnum.Loner,
                Guild = curGuild
            });

            await FollowupAsync("Сервер инициализирован", ephemeral: true);
        }
        //Добавить создание группировки одиночек
    }
}
