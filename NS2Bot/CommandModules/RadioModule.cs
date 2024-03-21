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
            Model.Data.Channels.Radio.RadioInitChannelId = Context.Channel.Id;
            Model.Data.Channels.Radio.IsRadioEnabled = true;

            await Context.Channel.SendMessageAsync("Для создания или подключения к частоте **напишите** команду /частота");
            await RespondAsync("Канал выбран как создание частот", ephemeral: true);
        }

        [SlashCommand("stopradio", "Отлючение радио канала")]
        [RequireOwner]
        public async Task StopRadioChannel()
        {
            Model.Data.Channels.Radio.RadioInitChannelId = 0;
            Model.Data.Channels.Radio.IsRadioEnabled = false;

            await RespondAsync("Создание частот отключено", ephemeral: true);
        }

        [SlashCommand("частота", "Подключиться к определенной частоте")]
        public async Task VoiceCreator([Summary("Частота")] string freq)
        {
            await DeferAsync(ephemeral: true);

            if (!Model.Data.Channels.Radio.IsRadioEnabled)
            {
                await FollowupAsync("Создание частот отключено");
                return;
            }

            SocketVoiceChannel? redirectionChannel = Context.Channel as SocketVoiceChannel;
            if (redirectionChannel != null && !redirectionChannel.ConnectedUsers.Contains(Context.User))
            {
                await FollowupAsync("Вы не подлючены к каналу \"Создание частоты\"", ephemeral: true);
                await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.Username} не подключен к войсу при создании рации"));
                return;
            }

            if (!radioname.IsMatch(freq))
            {
                await FollowupAsync("Сообщение должно иметь вид 000.000, где вместо нулей могут быть любые цифры", ephemeral: true);
                await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.Username} неправильно написал частоту"));
                return;
            }

            if (string.Equals(freq, "000.000"))
            {
                await FollowupAsync("Недопустимое значение частоты", ephemeral: true);
                await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.Username} ввёл 000.000"));
                return;
            }

            var category = ((SocketTextChannel)Context.Channel).Category;
            var curRadios = Context.Guild.Channels.Where(x => (x is IVoiceChannel) && ((SocketTextChannel)x)?.Category.Id == category.Id);

            var radioExists = curRadios.Where(x => string.Equals(x.Name, freq + " mhz")).FirstOrDefault();
            if (radioExists != null)
            {
                await ((SocketGuildUser)Context.User).ModifyAsync(x => x.ChannelId = radioExists.Id);
                await FollowupAsync($"Вы подключены к частоте {freq}", ephemeral: true);
                await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.Username} подключился к существующей частоте {freq}"));
                return;
            }

            var newVoice = await Context.Guild.CreateVoiceChannelAsync(freq + " mhz", prop => prop.CategoryId = category.Id);
            await newVoice.SyncPermissionsAsync();
            await newVoice.AddPermissionOverwriteAsync(Context.User, new Discord.OverwritePermissions(viewChannel: Discord.PermValue.Allow, moveMembers: Discord.PermValue.Allow));

            if (Model.Data.Channels.Radio.ActiveRadios == null)
                Model.Data.Channels.Radio.ActiveRadios = new List<ulong>();
            Model.Data.Channels.Radio.ActiveRadios.Add(newVoice.Id);

            await ((SocketGuildUser)Context.User).ModifyAsync(x => x.ChannelId = newVoice.Id);
            await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"{Context.User.Username} создал новую частоту {freq}"));

            await FollowupAsync($"Вы подключены к частоте {freq}", ephemeral: true);
        }
    }
}
