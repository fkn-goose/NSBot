using Discord;
using Discord.Interactions;
using Discord.Webhook;
using Discord.WebSocket;
using NS2Bot.Enums;
using NS2Bot.Extensions;
using NS2Bot.Models;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NS2Bot.CommandModules
{
    public class PDAModule : InteractionModuleBase<SocketInteractionContext>
    {
        private const string BotUserName = "Аноним";
        private const string BotAvatar = "https://media.moddb.com/cache/images/downloads/1/221/220278/thumb_620x2000/No_data.png";

        [SlashCommand("setpdachannel", "Установить общий-кпк канал")]
        [RequireOwner]
        public async Task SetPublicPDAChannel()
        {
            MainData.configData.PublicPDAChannelId = Context.Channel.Id;
            await RespondAsync("Канал установлен как \"Общий-кпк\"", ephemeral: true);
        }

        [SlashCommand("anonymous", "Отправить анонимное сообщение")]
        public async Task AnonymousMessage([Summary(name: "Сообщение", description: "Текст сообщения")] string message, [Summary(name: "Изображение", description: "Прикрепить изображение")][Optional][DefaultParameterValue(null)] IAttachment? attachment)
        {
            HttpClient client = new HttpClient();

            DiscordWebhookClient pdaWebHook = new DiscordWebhookClient(MainData.publicPdaWebHook);
            pdaWebHook.Log += _ => MainData.logger.LogAsync(_);

            await DeferAsync(ephemeral: true);

            message = message.Replace(@"\n", "\n");
            Task<ulong> messageId;

            if (attachment != null && attachment.Width != null)
                messageId = pdaWebHook.SendFileAsync(stream: client.GetStreamAsync(new Uri(attachment.ProxyUrl)).Result, filename: attachment.Filename, text: message, username: BotUserName, avatarUrl: BotAvatar);
            else
                messageId = pdaWebHook.SendMessageAsync(message, username: BotUserName, avatarUrl: BotAvatar);

            await FollowupAsync("Отправлено", ephemeral: true);

            var pdaEmbedLog = new EmbedBuilder()
                .WithTitle("Анонимное сообщение в кпк")
                .WithDescription(Format.Code(message));
            if (attachment != null && attachment.Width != null)
                pdaEmbedLog.WithImageUrl(attachment.ProxyUrl);
            pdaEmbedLog.AddField("Автор сообщения", MentionUtils.MentionUser(Context.User.Id))
                       .AddField("Ссылка на сообщение", (Context.Guild.GetChannel(MainData.configData.PublicPDAChannelId) as SocketTextChannel).GetMessageAsync(messageId.Result).Result.GetJumpUrl())
                       .WithCurrentTimestamp();

            var logChannel = Context.Guild.GetTextChannel(MainData.configData.PDALogsChannelId);
            await logChannel.SendMessageAsync(embed: pdaEmbedLog.Build());

            client.Dispose();
            pdaWebHook.Dispose();
        }
    }

    [Group("война", "Команды связанные с войной группировок")]
    public class PDAWar : InteractionModuleBase<SocketInteractionContext>
    {
        private const string BotName = "Новостная служба \"Вестник сталкера\"";
        private const string BotAvatar = "https://media.moddb.com/cache/images/downloads/1/221/220278/thumb_620x2000/No_data.png";

        private List<string> WarStartMessages = new List<string>()
        {
            "Сталкера, внимание! Отношения между {group1} и {group2} натянулись до предела. {group1} объявляет войну своему оппоненту. \r\n\r\nНаши источники докладывают, что {group1} использует повязки {l1} цвета, а {group2} - {r1}. Будьте осторожны рядом с такими группами, не словите случайную пулю."
        };

        private List<string> WarNewAllieMessages = new List<string>()
        {
            "Сталкера, внимание! По последним данным группировка {group2} присоединилась к войне на стороне группировки {group1}! \r\n\r\nНа текущий момент {grouplist} воюет против группировки {grouplist2}.\r\nНапоминаем всем, что первая сторона ходит в повязках {l1} цвета, а вторая в повязках {r1} цвета."
        };

        private List<string> WarLeaveMessage = new List<string>()
        {
            "Вестник сталкера на связи! Наши источники сообщают, что {groupleave} выходит из войны! Условия выхода точно нам неизвестны, но думаю ребята из группировки {grouplead} остались довольны. \r\n\r\nВ войне остались {grouplist} с повязками {l1} цвета и {grouplist2} с повязками {r1} цвета. Пожелаем удачи обеим сторонам, а сталкерам посоветуем не вставать у них на дороге."
        };

        private List<string> WarEndMessage = new List<string>()
        {
            "Свежие новости! Группировка {grouplose} капитулировала на условиях группировки {groupwin}! Война окончена, дышим глубже."
        };

        [SlashCommand("начать", "Объявление войны")]
        public async Task StartWar([Summary(name: "Инициатор", "Группировка которая объявила войну")] GroupsEnum initGroup,
                                   [Summary(name: "Цель", "Группировка которой объявили войну")] GroupsEnum targetGroup,
                                   [Summary(name: "Повязка1", "Цвет повязки первой группировки")] ColorsEnum initGroupColor,
                                   [Summary(name: "Повязка2", "Цвет повязки второй группировки")] ColorsEnum targetGroupColor)
        {
            DiscordWebhookClient SystemInfoWebHook = new DiscordWebhookClient(MainData.systemInfoWebHook);

            await DeferAsync(ephemeral: true);
            Random rnd = new Random();
            var selectedMessge = WarStartMessages[rnd.Next(0, WarStartMessages.Count - 1)];

            selectedMessge = selectedMessge.Replace("{group1}", initGroup.GetDescription());
            selectedMessge = selectedMessge.Replace("{group2}", targetGroup.GetDescription());

            ColorReplacer(initGroupColor, targetGroupColor, ref selectedMessge);

            await SystemInfoWebHook.SendMessageAsync(avatarUrl: BotAvatar, username: BotName, text: selectedMessge);

            if (MainData.configData.Wars == null)
                MainData.configData.Wars = new List<ConfigModel.War> { };

            MainData.configData.Wars.Add(new ConfigModel.War()
            {
                Agressor = (uint)initGroup,
                Target = (uint)targetGroup,
                AgreesorColor = (uint)initGroupColor,
                TargetColor = (uint)targetGroupColor,
                AgressorAllies = new List<uint>(),
                TargetAllies = new List<uint>(),
            });

            await FollowupAsync($"Группировка {initGroup.GetDescription()} объявила войну группировке {targetGroup.GetDescription()}", ephemeral: true);
        }

        [SlashCommand("добавить", "Добавить союзника одной из сторон")]
        public async Task AddAlies([Summary(name: "Сторона", "Группировка к которой присоединяется союзник")] GroupsEnum sideGroup,
                                   [Summary(name: "Союзик", "Группировка которая присоединяется к войне")] GroupsEnum newGroup)
        {
            if (MainData.configData.Wars == null)
                return;

            if (sideGroup == newGroup)
            {
                await RespondAsync("Выбраны одинаковые группировки");
                return;
            }

            ConfigModel.War curentWar = MainData.configData.Wars.Where(x => x.Agressor == (uint)sideGroup || x.Target == (uint)sideGroup).FirstOrDefault();
            if (curentWar == null)
            {
                await RespondAsync("Группировка не учавствует ни в одной из войн", ephemeral: true);
                return;
            }

            if (curentWar.AgressorAllies.Contains((uint)newGroup) || curentWar.TargetAllies.Contains((uint)newGroup) || curentWar.AgressorAllies.Contains((uint)sideGroup) || curentWar.TargetAllies.Contains((uint)sideGroup))
            {
                await RespondAsync("Группировка уже учавсвтует в войне", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            DiscordWebhookClient SystemInfoWebHook = new DiscordWebhookClient(MainData.systemInfoWebHook);

            if (curentWar.Agressor == (uint)sideGroup)
                curentWar.AgressorAllies.Add((uint)newGroup);
            if (curentWar.Target == (uint)sideGroup)
                curentWar.TargetAllies.Add((uint)newGroup);

            var groups = GroupList(curentWar);

            Random rnd = new Random();
            var selectedMessge = WarNewAllieMessages[rnd.Next(0, WarNewAllieMessages.Count - 1)];
            selectedMessge = selectedMessge.Replace("{group1}", sideGroup.GetDescription());
            selectedMessge = selectedMessge.Replace("{group2}", newGroup.GetDescription());
            selectedMessge = selectedMessge.Replace("{grouplist}", groups[0]);
            selectedMessge = selectedMessge.Replace("{grouplist2}", groups[1]);

            ColorReplacer((ColorsEnum)curentWar.AgreesorColor, (ColorsEnum)curentWar.TargetColor, ref selectedMessge);

            MainData.configData.Wars.Where(x => x.Agressor == (uint)sideGroup || x.Target == (uint)sideGroup).FirstOrDefault().Update(curentWar);

            await SystemInfoWebHook.SendMessageAsync(avatarUrl: BotAvatar, username: BotName, text: selectedMessge);

            await FollowupAsync("Группировка присоединена к войне", ephemeral: true);
        }

        [SlashCommand("убрать", "убрать группировку из войны")]
        public async Task LeaveWar([Summary(name: "Группировка", "Группировка которая выходит из войны")] GroupsEnum groupLeave)
        {
            if (MainData.configData.Wars == null)
                return;

            ConfigModel.War curentWar = MainData.configData.Wars.Where(x => x.Agressor == (uint)groupLeave || x.Target == (uint)groupLeave || x.AgressorAllies.Contains((uint)groupLeave) || x.TargetAllies.Contains((uint)groupLeave)).FirstOrDefault();
            if (curentWar == null)
            {
                await RespondAsync("Группировка не учавствует ни в одной из войн", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            DiscordWebhookClient SystemInfoWebHook = new DiscordWebhookClient(MainData.systemInfoWebHook);

            var selectedMessge = string.Empty;
            if (curentWar.AgressorAllies.Contains((uint)groupLeave))
            {
                curentWar.AgressorAllies.Remove((uint)groupLeave);

                Random rnd = new Random();
                selectedMessge = WarLeaveMessage[rnd.Next(0, WarNewAllieMessages.Count - 1)];
                ColorReplacer((ColorsEnum)curentWar.AgreesorColor, (ColorsEnum)curentWar.TargetColor, ref selectedMessge);

                var groups = GroupList(curentWar);

                selectedMessge = selectedMessge.Replace("{groupleave}", groupLeave.GetDescription());
                selectedMessge = selectedMessge.Replace("{grouplead}", ((GroupsEnum)curentWar.Target).GetDescription());
                selectedMessge = selectedMessge.Replace("{grouplist}", groups[0]);
                selectedMessge = selectedMessge.Replace("{grouplist2}", groups[1]);

                await FollowupAsync("Группировка удалена из войны", ephemeral: true);
            }
            else if (curentWar.TargetAllies.Contains((uint)groupLeave))
            {
                curentWar.TargetAllies.Remove((uint)groupLeave);

                Random rnd = new Random();
                selectedMessge = WarLeaveMessage[rnd.Next(0, WarNewAllieMessages.Count - 1)];
                ColorReplacer((ColorsEnum)curentWar.AgreesorColor, (ColorsEnum)curentWar.TargetColor, ref selectedMessge);

                var groups = GroupList(curentWar);

                selectedMessge = selectedMessge.Replace("{groupleave}", groupLeave.GetDescription());
                selectedMessge = selectedMessge.Replace("{grouplead}", ((GroupsEnum)curentWar.Agressor).GetDescription());
                selectedMessge = selectedMessge.Replace("{grouplist}", groups[0]);
                selectedMessge = selectedMessge.Replace("{grouplist2}", groups[1]);

                await FollowupAsync("Группировка удалена из войны", ephemeral: true);
            }
            else if (curentWar.Agressor == ((uint)groupLeave))
            {
                Random rnd = new Random();
                selectedMessge = WarEndMessage[rnd.Next(0, WarNewAllieMessages.Count - 1)];

                selectedMessge = selectedMessge.Replace("{grouplose}", groupLeave.GetDescription());
                selectedMessge = selectedMessge.Replace("{groupwin}", ((GroupsEnum)curentWar.Target).GetDescription());

                MainData.configData.Wars.Remove(curentWar);

                await FollowupAsync("Война окончена", ephemeral: true);
            }
            else if (curentWar.Target == ((uint)groupLeave))
            {
                Random rnd = new Random();
                selectedMessge = WarEndMessage[rnd.Next(0, WarNewAllieMessages.Count - 1)];

                selectedMessge = selectedMessge.Replace("{grouplose}", groupLeave.GetDescription());
                selectedMessge = selectedMessge.Replace("{groupwin}", ((GroupsEnum)curentWar.Agressor).GetDescription());

                MainData.configData.Wars.Remove(curentWar);

                await FollowupAsync("Война окончена", ephemeral: true);
            }

            await SystemInfoWebHook.SendMessageAsync(avatarUrl: BotAvatar, username: BotName, text: selectedMessge);
        }

        [SlashCommand("инфа", "получить информацию о войнах")]
        public async Task GetWarsInfo()
        {
            await DeferAsync();
            List<string[]> sides = new List<string[]>();
            foreach(var war in MainData.configData.Wars)
                sides.Add(GroupList(war));

            var embedWar = new EmbedBuilder()
                .WithTitle("Текущие войны");

            foreach(var side in sides)
                embedWar.AddField("===============", side[0] + "\nПротив\n" + side[1]);

            await FollowupAsync(embed: embedWar.Build());
        }

        private string[] GroupList(ConfigModel.War war)
        {
            string[] response = new string[2];

            response[0] = Format.Bold(((GroupsEnum)war.Agressor).GetDescription());
            response[1] = Format.Bold(((GroupsEnum)war.Target).GetDescription());

            if (war.AgressorAllies.Any())
            {
                response[0] = string.Join(" ", response[0], "со своими союзниками: ");
                var allies = string.Join(", ", war.AgressorAllies.SkipLast(1).Select(x => Format.Bold(((GroupsEnum)x).GetDescription())));
                if (string.IsNullOrEmpty(allies))
                    response[0] += Format.Bold(((GroupsEnum)war.AgressorAllies.Last()).GetDescription());
                else
                    response[0] += allies + " и " + Format.Bold(((GroupsEnum)war.AgressorAllies.Last()).GetDescription());
            }

            if (war.TargetAllies.Any())
            {
                response[1] = string.Join(" ", response[1], "и их союзников: ");
                var allies = string.Join(", ", war.TargetAllies.SkipLast(1).Select(x => Format.Bold(((GroupsEnum)x).GetDescription())));
                if (string.IsNullOrEmpty(allies))
                    response[1] += Format.Bold(((GroupsEnum)war.TargetAllies.Last()).GetDescription());
                else
                    response[1] += allies + " и " + Format.Bold(((GroupsEnum)war.TargetAllies.Last()).GetDescription());
            }

            return response;
        }

        private void ColorReplacer(ColorsEnum leftColor, ColorsEnum rightColor, ref string msg)
        {
            var leftGroupColor = ColorsDeclinaton.GetColorDict(leftColor);
            msg = msg.Replace("{l0}", leftGroupColor[DeclensionsEnum.Im]);
            msg = msg.Replace("{l1}", leftGroupColor[DeclensionsEnum.Rod]);

            var rightGroupColor = ColorsDeclinaton.GetColorDict(rightColor);
            msg = msg.Replace("{r0}", rightGroupColor[DeclensionsEnum.Im]);
            msg = msg.Replace("{r1}", rightGroupColor[DeclensionsEnum.Rod]);
        }
    }
}