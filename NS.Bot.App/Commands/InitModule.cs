using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Group;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Entities.Radio;
using NS.Bot.Shared.Entities.Warn;
using NS.Bot.Shared.Models;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace NS.Bot.App.Commands
{
    [RequireOwner]
    public class InitModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IGuildService _guildService;
        private readonly IBaseService<GroupEntity> _groupService;
        private readonly IWarnSettingsService _warnSettingsService;
        private readonly IRadioSettingsService _radioSettingsService;
        private readonly IBaseService<GuildRoles> _guildRolesService;
        private readonly IConfigurationRoot _config;
        public InitModule(IGuildService guildService, IBaseService<GroupEntity> groupService, IWarnSettingsService warnSettingsService,
            IRadioSettingsService radioSettingsService, IBaseService<GuildRoles> guildRolesService, IConfigurationRoot config)
        {
            _guildService = guildService;
            _groupService = groupService;
            _warnSettingsService = warnSettingsService;
            _radioSettingsService = radioSettingsService;
            _guildRolesService = guildRolesService;
            _config = config;
        }

        [SlashCommand("serverinit", "Инициализация сервера")]
        [RequireOwner]
        public async Task InitServer()
        {
            GuildEntity? curGuild = _guildService.GetAll().FirstOrDefault(x => x.GuildId == Context.Guild.Id);
            if (curGuild != null)
            {
                await RespondAsync("Сервер уже инициализирован", ephemeral: true);
                return;
            }
            await DeferAsync(ephemeral: true);

            await _guildService.CreateOrUpdateAsync(curGuild = new GuildEntity
            {
                GuildId = Context.Guild.Id,
                Name = Context.Guild.Name,
            });

            await _groupService.CreateOrUpdateAsync(new GroupEntity
            {
                GroupType = Shared.Enums.GroupEnum.Loner,
                Guild = curGuild
            });

            await FollowupAsync("Сервер инициализирован", ephemeral: true);
        }
        //Добавить создание группировки одиночек

        [SlashCommand("warninit", "Инициализация предов")]
        [RequireOwner]
        public async Task InitWarnSettings([Summary("Канал_предов")] ITextChannel warnChannel, [Summary("Первый")] IRole firstRole, [Summary("Второй")] IRole secondRole, [Summary("Третий")] IRole thirdRole,
                                           [Summary("Бан")] IRole banRole, [Summary("Ридонли")] IRole readonlyRole,
                                           [Summary("Выговор1")] IRole firstRebukeRole, [Summary("Выговор2")] IRole secondRebukeRole, [Summary("Выговор3")] IRole thirdRebukeRole)
        {
            await DeferAsync(ephemeral: true);

            var guild = await _guildService.GetByDiscordId(Context.Guild.Id);
            if (guild == null)
                return;

            WarnSettings settings = new WarnSettings()
            {
                RelatedGuild = guild,
                WarnChannelId = warnChannel.Id,
                FirstWarnRoleId = firstRole.Id,
                SecondWarnRoleId = secondRole.Id,
                ThirdWarnRoleId = thirdRole.Id,
                FirstRebukeRoleId = firstRebukeRole.Id,
                SecondRebukeRoleId = secondRebukeRole.Id,
                ThirdRebukeRoleId = thirdRebukeRole.Id,
                BanRoleId = banRole.Id,
                ReadOnlyRoleId = readonlyRole.Id
            };

            await _warnSettingsService.CreateOrUpdateAsync(settings);
            await FollowupAsync("Настройки сохранены", ephemeral: true);
        }

        [SlashCommand("radioinit", "Инициализация радио канала")]
        [RequireOwner]
        public async Task InitRadioChannel()
        {
            await DeferAsync(ephemeral: true);
            var settings = new RadioSettings();

            settings = await _radioSettingsService.GetRadioSettingsAsync(Context.Guild.Id);
            var category = ((SocketTextChannel)Context.Channel).Category;
            settings.CommandChannelId = Context.Channel.Id;
            settings.RadiosCategoryId = category.Id;
            settings.IsEnabled = true;

            await _radioSettingsService.UpdateAsync(settings);

            await Context.Channel.SendMessageAsync("Для создания или подключения к частоте **напишите** команду /частота");
            await FollowupAsync("Канал выбран как создание частот", ephemeral: true);
        }

        [SlashCommand("rolesinit", "Инициализация ролей")]
        [RequireOwner]
        public async Task InitRoles([Summary("игрок")] IRole player, [Summary("хелпер")] IRole helper, [Summary("младший_куратор")] IRole juniorCurator,
                                    [Summary("куратор")] IRole curator, [Summary("старший_куратор")] IRole seniorCurator, [Summary("рп_админ")] IRole rpAdmin,
                                    [Summary("зга")] IRole deputy, [Summary("га")] IRole chief, [Summary("младший_ивентолог")] IRole juniorEvent,
                                    [Summary("ивентолог")] IRole eventmaster, [Summary("гланвый_ивентолог")] IRole chiefEvent)
        {
            await DeferAsync(ephemeral: true);

            var guild = await _guildService.GetByDiscordId(Context.Guild.Id);
            if (guild == null)
                return;

            GuildRoles roles = new GuildRoles()
            {
                RelatedGuild = guild,
                PlayerRoleId = player.Id,
                HelperRoleId = helper.Id,
                JuniorCuratorRoleId = juniorCurator.Id,
                CuratorRoleId = curator.Id,
                SeniorCuratorRoleId = seniorCurator.Id,
                RPAdminRoleId = rpAdmin.Id,
                ChiefAdminDeputyRoleId = deputy.Id,
                ChiefAdminRoleId = chief.Id,
                JuniorEventmasterRoleId = juniorEvent.Id,
                EventmasterRoleId = eventmaster.Id,
                ChiefEventmasterRoleId = chiefEvent.Id
            };

            await _guildRolesService.CreateOrUpdateAsync(roles);

            await FollowupAsync("Роли сохранены", ephemeral: true);
        }

        [SlashCommand("datainit", "Инициализация доп. данных")]
        [RequireOwner]
        public async Task InitData([Summary("Зона")] IVoiceChannel zoneChannel, [Summary("ЖДК")] IVoiceChannel jdk, [Summary("ЖДХ")] IVoiceChannel jdh, [Summary("Канал_предов")] ITextChannel warnChannel)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            string json = File.ReadAllText(filePath);

            if (json == null)
            {
                await RespondAsync("Не найден appsettings.json", ephemeral: true);
                return;
            }

            AppsettingsModel? settings = JsonSerializer.Deserialize<AppsettingsModel>(json);

            if (settings == null)
            {
                await RespondAsync("Не считан appsettings.json", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            var allSettings = settings.GuildDatas;
            if (allSettings == null)
                allSettings = new List<GuildData>();

            var curSettings = allSettings.FirstOrDefault(x => x.RelatedGuildId == Context.Guild.Id);
            var oldsettings = new GuildData();
            bool isNew = false;
            if (curSettings == null)
            {
                isNew = true;
                curSettings = new GuildData();
            }
            else
                oldsettings = curSettings;

            var guild = await _guildService.GetByDiscordId(Context.Guild.Id);
            if (guild == null)
                return;

            curSettings = new GuildData()
            {
                RelatedGuildId = Context.Guild.Id,
                ZoneVoiceId = zoneChannel.Id,
                JDKVoiceId = jdk.Id,
                JDHVoiceId = jdh.Id,
                WarnChannelId = warnChannel.Id,
            };

            if (isNew)
                allSettings.Add(curSettings);
            else
            {
                allSettings.Remove(oldsettings);
                allSettings.Add(curSettings);
            }

            settings.GuildDatas = allSettings;

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
            };

            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "appsettings_temp.json"), JsonSerializer.Serialize(settings, options));
            File.Delete(filePath);
            File.Move(Path.Combine(AppContext.BaseDirectory, "appsettings_temp.json"), filePath);
            _config.Reload();

            await FollowupAsync("Настройки успешно сохранены", ephemeral: true);
        }

        [SlashCommand("ticketsinit", "Инициализация тикетов")]
        [RequireOwner]
        public async Task SetTicketLogChannel([Summary(name: "Категория_хелперов", description: "Категория хелперов")] ICategoryChannel? newHelperCategory, 
                                              [Summary(name: "Канал_тикетов_хелперов", description: "Канал тикетов хелперов")] ITextChannel? newHelperChannel,
                                              [Summary(name: "З_Канал_тикетов_хелперов", description: "Канал тикетов хелперов")] ITextChannel? oldHelperChannel,
                                              [Summary(name: "Категория_тикетов_кураторов", description: "Категория тикетов кураторов")] ICategoryChannel? newCuratorCategory, 
                                              [Summary(name: "Канал_тикетов_кураторов", description: "Канал тикетов кураторов")] ITextChannel? newCuraturChannel,
                                              [Summary(name: "З_Канал_тикетов_кураторов", description: "Канал тикетов кураторов")] ITextChannel? oldCuraturChannel,
                                              [Summary(name: "Категория_тикетов_админов", description: "Категория тикетов админов")] ICategoryChannel? newAdminCategory, 
                                              [Summary(name: "Канал_тикетов_админов", description: "Канал тикетов админов")] ITextChannel? newAdminChannel,
                                              [Summary(name: "З_Канал_тикетов_админов", description: "Канал тикетов админов")] ITextChannel? oldAdminChannel,
                                              [Summary(name: "Категория_закрытых_тикетов", description: "Категория закрытых тикетов")] ICategoryChannel? closedTicketCategory,
                                              [Summary(name: "Канал_для_логов_тикетов", description: "Канал для логов тикетов")] ITextChannel? logChannel,
                                              [Summary(name: "Канал_для_восст_бонусов", description: "Канал для восстановления бонусов")] ITextChannel? bonusTicketChannel,
                                              [Summary(name: "Канал_для_восст_вещей", description: "Канал для восстановления вещей")] ITextChannel? itemsTicketChannel,
                                              [Summary(name: "Канал_для_жалоб_на_игрока", description: "Канал для жалоб на игрока")] ITextChannel? complaintPlayerTicketChannel,
                                              [Summary(name: "Канал_для_жалоб_на_админа", description: "Канал для жалоб на админа")] ITextChannel? complaintAdminTicketChannel,
                                              [Summary(name: "Канал_для_новых_позывных", description: "Канал для новых позывных")] ITextChannel? nickTicketTicketChannel,
                                              [Summary(name: "З_Канал_для_восст_бонусов", description: "Канал для восстановления бонусов")] ITextChannel? closedBonusTicketChannel,
                                              [Summary(name: "З_Канал_для_восст_вещей", description: "Канал для восстановления вещей")] ITextChannel? closedItemsTicketChannel,
                                              [Summary(name: "З_Канал_для_жалоб_на_игрока", description: "Канал для жалоб на игрока")] ITextChannel? closedComplaintPlayerTicketChannel,
                                              [Summary(name: "З_Канал_для_жалоб_на_админа", description: "Канал для жалоб на админа")] ITextChannel? closedComplaintAdminTicketChannel,
                                              [Summary(name: "З_Канал_для_новых_позывных", description: "Канал для новых позывных")] ITextChannel? closedNickTicketChannel)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            string json = File.ReadAllText(filePath);

            if (json == null)
            {
                await RespondAsync("Не найден appsettings.json", ephemeral: true);
                return;
            }

            AppsettingsModel? settings = JsonSerializer.Deserialize<AppsettingsModel>(json);

            if(settings == null)
            {
                await RespondAsync("Не считан appsettings.json", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            var allSettings = settings.TicketSettings;
            if (allSettings == null)
                allSettings = new List<TicketSettingsModel>();

            var curSettings = allSettings.FirstOrDefault(x => x.RelatedGuildId == Context.Guild.Id);
            var oldsettings = new TicketSettingsModel();
            bool isNew = false;
            if (curSettings == null)
            {
                isNew = true;
                curSettings = new TicketSettingsModel();
            }
            else
                oldsettings = curSettings;

            curSettings.RelatedGuildId = Context.Guild.Id;
            curSettings.RelatedGuildName = Context.Guild.Name;

            curSettings.HelperNewTicketsCategoryId = newHelperCategory != null ? newHelperCategory.Id : curSettings.HelperNewTicketsCategoryId;
            curSettings.HelperNewTicketsChannelId = newHelperChannel != null ? newHelperChannel.Id : curSettings.HelperNewTicketsChannelId;
            curSettings.HelperOldTicketsChannelId = oldHelperChannel != null ? oldHelperChannel.Id : curSettings.HelperOldTicketsChannelId;

            curSettings.CuratorNewTicketsCategoryId = newCuratorCategory != null ? newCuratorCategory.Id : curSettings.CuratorNewTicketsCategoryId;
            curSettings.CuratorNewTicketsChannelId = newCuraturChannel != null ? newCuraturChannel.Id : curSettings.CuratorNewTicketsChannelId;
            curSettings.CuratorOldTicketsChannelId = oldCuraturChannel != null ? oldCuraturChannel.Id : curSettings.CuratorOldTicketsChannelId;

            curSettings.AdminNewTicketsCategoryId = newAdminCategory != null ? newAdminCategory.Id : curSettings.AdminNewTicketsCategoryId;
            curSettings.AdminNewTicketsChannelId = newAdminChannel != null ? newAdminChannel.Id : curSettings.AdminNewTicketsChannelId;
            curSettings.AdminOldTicketsChannelId = oldAdminChannel != null ? oldAdminChannel.Id : curSettings.CuratorOldTicketsChannelId;

            curSettings.BonusesNewTicketChannel = bonusTicketChannel != null ? bonusTicketChannel.Id : curSettings.BonusesNewTicketChannel;
            curSettings.BonusesOldTicketChannel = closedBonusTicketChannel != null ? closedBonusTicketChannel.Id : curSettings.BonusesOldTicketChannel;

            curSettings.ItemsNewTicketChannel = itemsTicketChannel != null ? itemsTicketChannel.Id : curSettings.ItemsNewTicketChannel;
            curSettings.ItemsOldTicketChannel = closedItemsTicketChannel != null ? closedItemsTicketChannel.Id : curSettings.ItemsOldTicketChannel;

            curSettings.ComplaintPlayerNewTicketChannel = complaintPlayerTicketChannel != null ? complaintPlayerTicketChannel.Id : curSettings.ComplaintPlayerNewTicketChannel;
            curSettings.ComplaintPlayerOldTicketChannel = closedComplaintPlayerTicketChannel != null ? closedComplaintPlayerTicketChannel.Id : curSettings.ComplaintPlayerOldTicketChannel;

            curSettings.ComplaintAdminNewTicketChannel = complaintAdminTicketChannel != null ? complaintAdminTicketChannel.Id : curSettings.ComplaintAdminNewTicketChannel;
            curSettings.ComplaintAdminOldTicketChannel = closedComplaintAdminTicketChannel != null ? closedComplaintAdminTicketChannel.Id : curSettings.ComplaintAdminOldTicketChannel;

            curSettings.NameChangeNewTicketChannel = itemsTicketChannel != null ? itemsTicketChannel.Id : curSettings.NameChangeNewTicketChannel;
            curSettings.NameChangeOldTicketChannel = closedNickTicketChannel != null ? closedNickTicketChannel.Id : curSettings.NameChangeOldTicketChannel;

            curSettings.TicketLogs = logChannel != null ? logChannel.Id : curSettings.TicketLogs;
            curSettings.ClosedTicketsCategoryId = closedTicketCategory != null ? closedTicketCategory.Id : curSettings.ClosedTicketsCategoryId;

            if (isNew)
                allSettings.Add(curSettings);
            else
            {
                allSettings.Remove(oldsettings);
                allSettings.Add(curSettings);
            }

            settings.TicketSettings = allSettings;

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
            };
            File.WriteAllText(filePath, JsonSerializer.Serialize(settings, options));
            _config.Reload();

            await FollowupAsync("Настройки успешно сохранены", ephemeral: true);
        }
    }
}
