using Discord.Interactions;
using Discord.WebSocket;
using System.Linq;
using System.Text.RegularExpressions;

namespace NS2Bot.CommandModules
{
    public class RadioModule : InteractionModuleBase<SocketInteractionContext>
    {
        Regex radioname = new Regex("\\d\\d\\d\\.\\d\\d\\d");

        [SlashCommand("startradiocreation", "Инициализация радио канала")]
        [RequireOwner]
        public async Task InitRadioChannel()
        {
            MainData.configData.Category.RadioInitChannelId = Context.Channel.Id;

            MainData.configData.IsRadioEnabled = true;

            //Context.Client.MessageReceived += VoiceCreator;
            //Context.Client.UserVoiceStateUpdated += VoiceRemover;

            await RespondAsync("Канал выбран как создание частот", ephemeral: true);
        }

        //private async Task VoiceCreator(SocketMessage message)
        //{
        //    var sender = Context.Guild.GetUser(message.Author.Id);
        //    if (message.Channel.Id != MainData.configData.Category.RadioInitChannelId)
        //        return;

        //    if(!radioname.IsMatch(message.Content))
        //    {
        //        await RespondAsync("Сообщение должно иметь вид 000.000, где вместо нулей могут быть любые цифры", ephemeral: true);
        //        await message.DeleteAsync();
        //        return;
        //    }

        //    if(string.Equals(message.Content, "000.000"))
        //    {
        //        await RespondAsync("Недопустимое значение частоты", ephemeral: true);
        //        await message.DeleteAsync();
        //        return;
        //    }

        //    var redirectionChannel = Context.Guild.GetVoiceChannel(message.Channel.Id);
        //    if (!redirectionChannel.ConnectedUsers.Contains(message.Author))
        //    {
        //        await RespondAsync("Вы не подлючены к каналу \"Переадресация\"", ephemeral: true);
        //        await message.DeleteAsync();
        //        return;
        //    }    

        //    var category = Context.Guild.CategoryChannels.FirstOrDefault(x => x.Channels.Contains((SocketGuildChannel)message.Channel));

        //    var userVoice = await Context.Guild.CreateVoiceChannelAsync(message.Content, prop => prop.CategoryId = category.Id);
        //    await userVoice.AddPermissionOverwriteAsync(sender, new Discord.OverwritePermissions(moveMembers: Discord.PermValue.Allow));
        //    if (MainData.configData.Category.ActiveRadios == null)
        //        MainData.configData.Category.ActiveRadios = new List<ulong>();
        //    MainData.configData.Category.ActiveRadios.Add(userVoice.Id);

        //    await sender.ModifyAsync(x=>x.ChannelId = userVoice.Id);
        //}

        //private async Task VoiceRemover(SocketUser user, SocketVoiceState state1, SocketVoiceState state2)
        //{
        //    if (MainData.configData.Category.ActiveRadios.Contains(state1.VoiceChannel.Id) && state1.VoiceChannel.ConnectedUsers.Count == 0)
        //        await state1.VoiceChannel.DeleteAsync();
        //}
    }
}
