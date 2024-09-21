using Discord.Interactions;
using Discord.WebSocket;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Radio;
using System.Text.RegularExpressions;

namespace NS.Bot.App.Commands
{
    public class RadioModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly Regex radioname = new("\\d\\d\\d\\.\\d\\d\\d");

        private readonly IRadioSettingsService _radioSetttingsService;
        private readonly IBaseService<RadioEntity> _radioEntityService;
        private static Dictionary<ulong, RadioSettings> RadioSettings = new Dictionary<ulong, RadioSettings>();
        public RadioModule(IRadioSettingsService radioSettingsService, IBaseService<RadioEntity> radioEntityService)
        {
            _radioSetttingsService = radioSettingsService;
            _radioEntityService = radioEntityService;
        }

        [SlashCommand("stopradio", "Отлючение радио канала")]
        [RequireOwner]
        public async Task StopRadioChannel()
        {
            await DeferAsync(ephemeral: true);

            var settings = new RadioSettings();

            if (!RadioSettings.TryGetValue(Context.Guild.Id, out settings))
            {   
                settings = await _radioSetttingsService.GetRadioSettingsAsync(Context.Guild.Id);
                if(settings == null)
                {
                    await FollowupAsync("Не удалось поулчить настройки раций", ephemeral: true);
                    return;
                }
                RadioSettings[Context.Guild.Id] = settings;
            }

            if(!settings.IsEnabled)
            {
                await FollowupAsync("Рации уже выключены", ephemeral: true);
                return;
            }
            settings.CommandChannelId = 0;
            settings.IsEnabled = false;

            await _radioSetttingsService.UpdateAsync(settings);
            await FollowupAsync("Создание частот отключено", ephemeral: true);
        }

        [SlashCommand("частота", "Подключиться к частоте")]
        public async Task VoiceCreator([Summary("Частота")] string freq)
        {
            await DeferAsync(ephemeral: true);
            RadioSettings settings = null;
            RadioSettings.TryGetValue(Context.Guild.Id, out settings);

            if (!RadioSettings.TryGetValue(Context.Guild.Id, out settings))
            {
                settings = await _radioSetttingsService.GetRadioSettingsAsync(Context.Guild.Id);
                RadioSettings[Context.Guild.Id] = settings;
            }

            if (!settings.IsEnabled)
            {
                await FollowupAsync("Создание частот невозможно");
                return;
            }

            SocketVoiceChannel? redirectionChannel = Context.Channel as SocketVoiceChannel;
            if (redirectionChannel != null && !redirectionChannel.ConnectedUsers.Contains(Context.User))
            {
                await FollowupAsync("Вы не подлючены к каналу \"Создание частоты\"", ephemeral: true);
                //await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.Username} не подключен к войсу при создании рации"));
                return;
            }

            if (!radioname.IsMatch(freq))
            {
                await FollowupAsync("Частота должна иметь вид 000.000, где вместо нулей могут быть любые цифры", ephemeral: true);
                //await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.Username} неправильно написал частоту"));
                return;
            }

            if (string.Equals(freq, "000.000"))
            {
                await FollowupAsync("Недопустимое значение частоты", ephemeral: true);
                //await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.Username} ввёл 000.000"));
                return;
            }

            var category = ((SocketTextChannel)Context.Channel).Category;
            var curRadios = _radioEntityService.GetAll().Where(x => x.Guild.Id == settings.RelatedGuild.Id).ToList();

            var radioExists = curRadios.FirstOrDefault(x => string.Equals(x.VoiceName, freq + " mhz"));
            if (radioExists != null)
            {
                await ((SocketGuildUser)Context.User).ModifyAsync(x => x.ChannelId = radioExists.VoiceChannelId);
            }
            else
            {
                var newVoice = await Context.Guild.CreateVoiceChannelAsync(freq + " mhz", prop => prop.CategoryId = category.Id);
                await newVoice.SyncPermissionsAsync();
                await newVoice.AddPermissionOverwriteAsync(Context.User, new Discord.OverwritePermissions(viewChannel: Discord.PermValue.Allow, moveMembers: Discord.PermValue.Allow));

                var radio = new RadioEntity()
                {
                    VoiceChannelId = newVoice.Id,
                    VoiceName = newVoice.Name,
                    GuildId = settings.RelatedGuild.Id,
                };
                await _radioEntityService.CreateOrUpdateAsync(radio);

                await ((SocketGuildUser)Context.User).ModifyAsync(x => x.ChannelId = newVoice.Id);
                //await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.Username} создал новую частоту {freq}"));
            }

            await FollowupAsync($"Вы подключены к частоте {freq}", ephemeral: true);
        }
    }
}
