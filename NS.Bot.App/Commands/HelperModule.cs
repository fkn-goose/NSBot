using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Options;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Enums;
using NS.Bot.Shared.Models;

namespace NS.Bot.App.Commands
{
    [Group("хелпер", "Команды для хелперов и старше")]
    public class HelperModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IGuildService _guildService;
        private readonly AppsettingsModel _appsettings;
        public HelperModule(IGuildService guildService, AppsettingsModel appsettings)
        {
            _guildService = guildService;
            _appsettings = appsettings;
        }

        [SlashCommand("уведомление", "Отправляет уведомление выбранного типа")]
        public async Task Notification([Summary("Пользователь")] IGuildUser user, [Summary("Тип")] HelperNotificationType notificationType)
        {
            await DeferAsync(ephemeral: true);
            
            var currentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);

            if (_appsettings.GuildDatas == null)
            {
                await FollowupAsync("Не удалось поулчить настройки предупреждений", ephemeral: true);
                return;
            }

            var settings = _appsettings.GuildDatas.FirstOrDefault(x=>x.RelatedGuildId == Context.Guild.Id);
            if (settings == null)
            {
                await FollowupAsync("Не удалось поулчить настройки предупреждений для сервера", ephemeral: true);
                return;
            }

            var chnl = Context.Guild.GetTextChannel(settings.WarnChannelId);
            if (chnl == null)
            {
                await FollowupAsync("Не удалось найти канал с предупреждениями", ephemeral: true);
                return;
            }

            var msg = string.Empty;
            EmbedBuilder notificationEmbed = new EmbedBuilder()
                .WithTitle("Уведомление")
                .WithColor(Color.DarkBlue);

            EmbedBuilder msgToUser = new EmbedBuilder()
                .WithTitle("Уведомление")
                .WithColor(Color.DarkBlue);

            switch (notificationType)
            {
                case HelperNotificationType.GameNick:
                    msg = "Отредактируйте никнейм в DayZ лаунчере в соответствии с позывным в дискорде английскими буквами";
                    msgToUser.AddField("Смените ник", msg);
                    notificationEmbed.AddField("Смените ник", msg);
                    break;
                case HelperNotificationType.DiscrodNick:
                    msg = "Установите РП-позывной через меню сервера";
                    msgToUser.AddField("Смените ник", msg);
                    notificationEmbed.AddField("Смените ник", msg);
                    break;
                case HelperNotificationType.SetTag:
                    msg = "Установите в лаунчере приписку ГП к которой относитесь в соответствии с правилами сервера";
                    msgToUser.AddField("Установите приписку", msg);
                    notificationEmbed.AddField("Установите приписку", msg);
                    break;
                case HelperNotificationType.RemoveTag:
                    msg = "Уберите в лаунчере приписку ГП членом которой вы не являетесь";
                    msgToUser.AddField("Уберите приписку", msg);
                    notificationEmbed.AddField("Уберите приписку", msg);
                    break;
                case HelperNotificationType.InChannel:
                    msg = $"Во время игры вы должны быть в канале https://discord.com/channels/{Context.Guild.Id}/{settings.ZoneVoiceId}, либо в канале РП-Рации, при имении самой рации на рюкзаке или в инвентаре со вставленной батарейкой.";
                    msgToUser.AddField("Зайдите в канал Зона", msg);
                    notificationEmbed.AddField("Зайдите в канал Зона", $"Во время игры вы должны быть в канале {MentionUtils.MentionChannel(settings.ZoneVoiceId)}, либо в канале РП-Рации, при имении самой рации на рюкзаке или в инвентаре со вставленной батарейкой.");
                    break;
                case HelperNotificationType.JDK:
                    msg = $"Зайдите в канал https://discord.com/channels/{Context.Guild.Id}/{settings.JDKVoiceId}";
                    msgToUser.AddField("Зайдите в канал \"Жду Куратора\"", msg);
                    notificationEmbed.AddField("Зайдите в канал \"Жду Куратора\"", $"Зайдите в канал {MentionUtils.MentionChannel(settings.JDKVoiceId)}");
                    break;
                case HelperNotificationType.JDH:
                    msg = $"Зайдите в канал https://discord.com/channels/{Context.Guild.Id}/{settings.JDHVoiceId}";
                    msgToUser.AddField("Зайдите в канал \"Жду Хелпера\"", msg);
                    notificationEmbed.AddField("Зайдите в канал \"Жду Хелпера\"", $"Зайдите в канал {MentionUtils.MentionChannel(settings.JDHVoiceId)}");
                    break;
                default:
                    await FollowupAsync("Неизвестная интеракция", ephemeral: true);
                    return;
            }

            notificationEmbed.AddField("Администратор", string.Format("{0} ({1})", MentionUtils.MentionUser(Context.User.Id), Context.User.Username));

            var warnChannel = Context.Guild.GetTextChannel(settings.WarnChannelId);
            var msgId = await warnChannel.SendMessageAsync(text: MentionUtils.MentionUser(user.Id), embed: notificationEmbed.Build());

            msgToUser.AddField("Администратор", string.Format("{0} ({1})", MentionUtils.MentionUser(Context.User.Id), Context.User.Username));
            msgToUser.AddField("Сообщение", $"https://discord.com/channels/{Context.Guild.Id}/{settings.WarnChannelId}/{msgId.Id}");
            await user.SendMessageAsync(embed: msgToUser.Build());

            await FollowupAsync("Уведомление отправлено", ephemeral: true);
        }
    }
}
