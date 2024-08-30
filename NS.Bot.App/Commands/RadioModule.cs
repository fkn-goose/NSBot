using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Radio;
using System.Text.RegularExpressions;

namespace NS2Bot.CommandModules
{
    public class RadioModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly Regex radioname = new("\\d\\d\\d\\.\\d\\d\\d");

        private readonly IRadioSettingsService _radioSetttingsService;
        private readonly IBaseService<RadioEntity> _radioEntityService;

        public RadioModule(IRadioSettingsService radioSetttingsService, IBaseService<RadioEntity> radioEntityService)
        {
            _radioSetttingsService = radioSetttingsService;
            _radioEntityService = radioEntityService;
        }

        [SlashCommand("startradio", "Инициализация радио канала")]
        [RequireOwner]
        public async Task InitRadioChannel()
        {
            await DeferAsync(ephemeral: true);

            var settings = await _radioSetttingsService.GetRadioSettingsAsync(Context.Guild.Id);
            var category = ((SocketTextChannel)Context.Channel).Category;
            settings.CommandChannelId = Context.Channel.Id;
            settings.RadiosCategoryId = category.Id;
            settings.IsEnabled = true;

            await _radioSetttingsService.Update(settings);

            await Context.Channel.SendMessageAsync("Для создания или подключения к частоте **напишите** команду /частота");
            await FollowupAsync("Канал выбран как создание частот", ephemeral: true);
        }

        [SlashCommand("stopradio", "Отлючение радио канала")]
        [RequireOwner]
        public async Task StopRadioChannel()
        {
            await DeferAsync(ephemeral: true);

            var settings = await _radioSetttingsService.GetRadioSettingsAsync(Context.Guild.Id);
            settings.CommandChannelId = 0;
            settings.IsEnabled = false;

            await _radioSetttingsService.Update(settings);

            await FollowupAsync("Создание частот отключено", ephemeral: true);
        }

        [SlashCommand("частота", "Подключиться к частоте")]
        public async Task VoiceCreator([Summary("Частота")] string freq)
        {
            await DeferAsync(ephemeral: true);
            var settings = await _radioSetttingsService.GetRadioSettingsAsync(Context.Guild.Id);

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
                await FollowupAsync("Сообщение должно иметь вид 000.000, где вместо нулей могут быть любые цифры", ephemeral: true);
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
            var curRadios = _radioEntityService.GetAll().Where(x=>x.Guild.Id == settings.Guild.Id).ToList();

            var radioExists = curRadios.FirstOrDefault(x => string.Equals(x.VoiceName, freq + " mhz"));
            if (radioExists != null)
            {
                await ((SocketGuildUser)Context.User).ModifyAsync(x => x.ChannelId = radioExists.VoiceChannelId);
                await FollowupAsync($"Вы подключены к частоте {freq}", ephemeral: true);
                //await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.Username} подключился к существующей частоте {freq}"));
                return;
            }

            var newVoice = await Context.Guild.CreateVoiceChannelAsync(freq + " mhz", prop => prop.CategoryId = category.Id);
            await newVoice.SyncPermissionsAsync();
            await newVoice.AddPermissionOverwriteAsync(Context.User, new Discord.OverwritePermissions(viewChannel: Discord.PermValue.Allow, moveMembers: Discord.PermValue.Allow));

            var radio = new RadioEntity()
            {
                VoiceChannelId = newVoice.Id,
                VoiceName = newVoice.Name,
                Guild = settings.Guild,
            };
            await _radioEntityService.CreateOrUpdate(radio);

            await ((SocketGuildUser)Context.User).ModifyAsync(x => x.ChannelId = newVoice.Id);
            //await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.Username} создал новую частоту {freq}"));

            await FollowupAsync($"Вы подключены к частоте {freq}", ephemeral: true);
        }
    }
}
