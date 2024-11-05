using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Enums;

namespace NS.Bot.App.Commands
{
    [Group("хелпер", "Команды для хелперов и старше")]
    public class HelperModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IWarnSettingsService _warnSettingsService;
        private readonly IGuildService _guildService;
        private readonly IBaseService<GuildData> _guildData;
        public HelperModule(IWarnSettingsService warnSettingsService, IGuildService guildService, IBaseService<GuildData> guildData)
        {
            _warnSettingsService = warnSettingsService;
            _guildService = guildService;
            _guildData = guildData;
        }

        [SlashCommand("уведомление", "Отправляет уведомление выбранного типа")]
        public async Task Notification([Summary("Пользователь")] IGuildUser user, [Summary("Тип")] HelperNotificationType notificationType)
        {
            await DeferAsync(ephemeral: true);

            var currentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            var guildData = await _guildData.GetAll().FirstOrDefaultAsync(x => x.RelatedGuildId == currentGuild.Id);
            if (guildData == null)
            {
                await FollowupAsync("Не удалось поулчить данные сервера", ephemeral: true);
                return;
            }

            var settings = await _warnSettingsService.GetWarnSettingsAsync(Context.Guild.Id);
            if (settings == null)
            {
                await FollowupAsync("Не удалось поулчить настройки предупреждений", ephemeral: true);
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
                    msg = $"Во время игры вы должны быть в канале https://discord.com/channels/{Context.Guild.Id}/{guildData.ZoneVoiceId}, либо в канале РП-Рации, при имении самой рации на рюкзаке или в инвентаре со вставленной батарейкой.";
                    msgToUser.AddField("Зайдите в канал Зона", msg);
                    notificationEmbed.AddField("Зайдите в канал Зона", $"Во время игры вы должны быть в канале {MentionUtils.MentionChannel(guildData.ZoneVoiceId)}, либо в канале РП-Рации, при имении самой рации на рюкзаке или в инвентаре со вставленной батарейкой.");
                    break;
                case HelperNotificationType.JDK:
                    msg = $"Зайдите в канал https://discord.com/channels/{Context.Guild.Id}/{guildData.JDKVoiceId}";
                    msgToUser.AddField("Зайдите в канал \"Жду Куратора\"", msg);
                    notificationEmbed.AddField("Зайдите в канал \"Жду Куратора\"", $"Зайдите в канал {MentionUtils.MentionChannel(guildData.JDKVoiceId)}");
                    break;
                case HelperNotificationType.JDH:
                    msg = $"Зайдите в канал https://discord.com/channels/{Context.Guild.Id}/{guildData.JDHVoiceId}";
                    msgToUser.AddField("Зайдите в канал \"Жду Хелпера\"", msg);
                    notificationEmbed.AddField("Зайдите в канал \"Жду Хелпера\"", $"Зайдите в канал {MentionUtils.MentionChannel(guildData.JDHVoiceId)}");
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
            await user.SendMessageAsync(embed:msgToUser.Build());

            await FollowupAsync("Уведомление отправлено", ephemeral: true);
        }
    }
}
