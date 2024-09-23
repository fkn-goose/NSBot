using Discord;
using Discord.Interactions;
using NS.Bot.BuisnessLogic.Interfaces;

namespace NS.Bot.App.Commands
{
    [Group("хелпер", "Команды для хелперов и старше")]
    public class HelperModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IWarnSettingsService _warnSettingsService;
        private readonly IGuildService _guildService;
        public HelperModule(IWarnSettingsService warnSettingsService, IGuildService guildService)
        {
            _warnSettingsService = warnSettingsService;
            _guildService = guildService;
        }

        [SlashCommand("смените_ник", "Отправляет уведомление о необходимости смены ника")]
        public async Task NickMessage([Summary("Пользователь")] IGuildUser user, [Summary("Пользователь")] string customMessage = "")
        {
            await DeferAsync(ephemeral: true);

            var currentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            var settings = await _warnSettingsService.GetWarnSettingsAsync(Context.Guild.Id);
            if (settings == null)
            {
                await FollowupAsync("Не удалось поулчить настройки предупреждений", ephemeral: true);
                return;
            }

            var chnl = Context.Guild.GetTextChannel(settings.WarnChannelId);
        }
    }
}
