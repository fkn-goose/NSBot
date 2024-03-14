using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Linq;
using System.Text.RegularExpressions;

namespace NS2Bot.CommandModules
{
    public class RadioModule : InteractionModuleBase<SocketInteractionContext>
    {
        Regex radioname = new Regex("\\d\\d\\d\\.\\d\\d\\d");

        [SlashCommand("startradio", "Инициализация радио канала")]
        [RequireOwner]
        public async Task InitRadioChannel()
        {
            MainData.configData.Category.RadioInitChannelId = Context.Channel.Id;
            MainData.configData.IsRadioEnabled = true;

            await RespondAsync("Канал выбран как создание частот", ephemeral: true);
        }

        [SlashCommand("stopradio", "Отлючение радио канала")]
        [RequireOwner]
        public async Task StopRadioChannel()
        {
            MainData.configData.Category.RadioInitChannelId = 0;
            MainData.configData.IsRadioEnabled = false;

            await RespondAsync("Создание частот отключено", ephemeral: true);
        }

        [SlashCommand("частота", "Подключиться к определенной частоте")]
        public async Task VoiceCreator([Summary("Частота")] string freq)
        {
            if (!MainData.configData.IsRadioEnabled)
            {
                await RespondAsync("Создание частот отключено");
                return;
            }

            SocketVoiceChannel? redirectionChannel = Context.Channel as SocketVoiceChannel;
            if (redirectionChannel != null && !redirectionChannel.ConnectedUsers.Contains(Context.User))
            {
                await RespondAsync("Вы не подлючены к каналу \"Создание частоты\"", ephemeral: true);
                await MainData.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.GlobalName} не подключен к войсу при создании рации"));
                return;
            }

            if (!radioname.IsMatch(freq))
            {
                await RespondAsync("Сообщение должно иметь вид 000.000, где вместо нулей могут быть любые цифры", ephemeral: true);
                await MainData.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.GlobalName} неправильно написал частоту"));
                return;
            }

            if (string.Equals(freq, "000.000"))
            {
                await RespondAsync("Недопустимое значение частоты", ephemeral: true);
                await MainData.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.GlobalName} ввёл 000.000"));
                return;
            }

            await DeferAsync(ephemeral: true);

            var category = ((SocketTextChannel)Context.Channel).Category;
            var curRadios = Context.Guild.Channels.Where(x => (x is IVoiceChannel) && ((SocketTextChannel)x)?.Category.Id == category.Id);

            var radioExists = curRadios.Where(x => string.Equals(x.Name, freq + " mhz")).FirstOrDefault();
            if (radioExists != null)
            {
                await ((SocketGuildUser)Context.User).ModifyAsync(x => x.ChannelId = radioExists.Id);
                await FollowupAsync($"Вы подключены к частоте {freq}", ephemeral: true);
                await MainData.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.GlobalName} подключился к существующей частоте {freq}"));
                return;
            }

            var newVoice = await Context.Guild.CreateVoiceChannelAsync(freq + " mhz", prop => prop.CategoryId = category.Id);
            await newVoice.SyncPermissionsAsync();
            await newVoice.AddPermissionOverwriteAsync(Context.User, new Discord.OverwritePermissions(viewChannel: Discord.PermValue.Allow, moveMembers: Discord.PermValue.Allow));

            if (MainData.configData.Category.ActiveRadios == null)
                MainData.configData.Category.ActiveRadios = new List<ulong>();
            MainData.configData.Category.ActiveRadios.Add(newVoice.Id);

            await ((SocketGuildUser)Context.User).ModifyAsync(x => x.ChannelId = newVoice.Id);
            await MainData.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.GlobalName} создал новую частоту {freq}"));

            await FollowupAsync($"Вы подключены к частоте {freq}", ephemeral: true);
        }
    }
}
