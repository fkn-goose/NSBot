using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NS.Bot.App.Handlers;
using NS.Bot.App.Logging;
using NS.Bot.BuisnessLogic;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Entities.Radio;
using NS.Bot.Shared.Entities.Warn;
using NS.Bot.Shared.Models;
using SteamQuery;
using System.Timers;

namespace NS.Bot.App
{
    public class Program
    {
        private DiscordSocketClient _client;
        private static List<SocketGuild> guilds = new List<SocketGuild>();
        private static List<WarnSettings> warnSettings = new List<WarnSettings>();
        public static Task Main(string[] args) => new Program().MainAsync(args);

        public async Task MainAsync(string[] args)
        {
            try
            {
                await RunAsync(CreateHostBuilder(args).Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            AppsettingsModel appsettings = new AppsettingsModel();
            var builder = Host.CreateDefaultBuilder(args);

            var path = Path.GetFullPath("appsettings.json");
            var config = new ConfigurationBuilder()
#if DEBUG
                .AddJsonFile(path, optional: false, reloadOnChange: true)
#else
                .AddJsonFile("appsettings.json", false)
#endif
                .AddEnvironmentVariables()
                .Build();
            ConfigurationBinder.Bind(config, appsettings);
            return builder.ConfigureServices((_, services) =>
            {
                services.AddSingleton(config);
                services.AddBuisnessServices();
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(config.GetConnectionString("NSDataBase"), b => b.MigrationsAssembly("NS.Bot.Migrations"));
                });
                services.AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.MessageContent,
                    LogGatewayIntentWarnings = false,
                    AlwaysDownloadUsers = true,
                    LogLevel = LogSeverity.Debug,
                    UseInteractionSnowflakeDate = false,
                }));
                services.AddTransient<ConsoleLogger>();
                services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
                services.AddSingleton<InteractionHandler>();
                services.AddSingleton(appsettings);
                services.AddSingleton(x => new CommandService(new CommandServiceConfig
                {
                    LogLevel = Discord.LogSeverity.Debug,
                    DefaultRunMode = Discord.Commands.RunMode.Async
                }));
            });
        }

        public async Task RunAsync(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var commands = provider.GetRequiredService<InteractionService>();
            _client = provider.GetRequiredService<DiscordSocketClient>();
            var config = provider.GetRequiredService<IConfigurationRoot>();

            var radioSettingsService = provider.GetService<IRadioSettingsService>();
            var radioService = provider.GetService<IBaseService<RadioEntity>>();

            var warnService = provider.GetService<IWarnService>();
            var warnSettingsService = provider.GetService<IBaseService<WarnSettings>>();
            var guildService = provider.GetService<IBaseService<GuildEntity>>();
            var logger = provider.GetService<ILogToFileService>();
            var appsettings = provider.GetService<AppsettingsModel>();

            await provider.GetRequiredService<InteractionHandler>().InitializeAsync();

            warnSettings = warnSettingsService.GetAll().ToList();

            var dataTimer = new System.Timers.Timer(300000);
            dataTimer.Elapsed += (sender, e) => WarnRemover(sender, e, warnService, guildService, logger);
            dataTimer.AutoReset = true;
            dataTimer.Start();

            var updateTimer = new System.Timers.Timer(30000);
            updateTimer.Elapsed += (sener, e) => UpdateStatus(sener, e);
            updateTimer.AutoReset = true;
            updateTimer.Start();
            
            _client.UserJoined += (arg) => UserJoined(arg, appsettings.Roles);
            _client.MessageReceived += (message) => RndMessagesDelete(message, radioSettingsService, logger);
            _client.UserVoiceStateUpdated += (user, before, after) => VoiceRemover(user, before, after, radioSettingsService, radioService, logger);

            _client.Log += _ => provider.GetRequiredService<ConsoleLogger>().LogAsync(_);
            commands.Log += _ => provider.GetRequiredService<ConsoleLogger>().LogAsync(_);

            _client.Ready += async () =>
            {
                await commands.RegisterCommandsGloballyAsync(true);
            };

            await _client.LoginAsync(Discord.TokenType.Bot, config["DiscordConnection:NSBotToken"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task UserJoined(SocketGuildUser arg, List<RolesContainer> roles)
        {
            var guildRoles = roles.FirstOrDefault(x=>x.RelatedGuildId == arg.Guild.Id);
            if (guildRoles == null)
                return;

            if (guildRoles.NewPlayerRoleId == 0)
                return;

            await arg.AddRoleAsync(guildRoles.NewPlayerRoleId);
        }

        private async Task RndMessagesDelete(SocketMessage message, IRadioSettingsService radioSettingsService, ILogToFileService logger)
        {
            if (message.Author.IsBot || message.Author.IsWebhook)
                return;

            var channel = message.Channel as SocketGuildChannel;
            var curGuild = channel.Guild;
            RadioSettings settings = await radioSettingsService.GetRadioSettingsAsync(curGuild.Id);

            if (!settings.IsEnabled)
                return;

            if (message.Channel.Id != settings.CommandChannelId)
                return;

            await message.DeleteAsync();
            var msg = await message.Channel.SendMessageAsync("Для подключения к частоте **напишите** комманду\n/частота");

            await logger.Info($"[RNDMSGRADIO_REMOVE] | Автоматическое удаление неправильного сообщения в создании частоты");
            System.Timers.Timer dataTimer = new System.Timers.Timer(5000);
            dataTimer.AutoReset = false;
            dataTimer.Elapsed += (sender, e) => { DeleteRndMessage(sender, e, msg); };
            dataTimer.Start();
        }

        private void DeleteRndMessage(object sender, ElapsedEventArgs e, RestUserMessage msg)
        {
            msg.DeleteAsync().Wait();
        }

        private static async Task VoiceRemover(SocketUser user, SocketVoiceState before, SocketVoiceState after, IRadioSettingsService radioSettingsService, IBaseService<RadioEntity> radioService, ILogToFileService logger)
        {
            if (before.VoiceChannel == null)
                return;

            if (before.VoiceChannel.ConnectedUsers.Count != 0)
                return;

            if (before.VoiceChannel == after.VoiceChannel)
                return;

            var guildUser = user as SocketGuildUser;
            var curGuild = guildUser.Guild;
            RadioSettings settings = await radioSettingsService.GetRadioSettingsAsync(curGuild.Id);

            if (before.VoiceChannel.Category.Id != settings.RadiosCategoryId)
                return;

            if (before.VoiceChannel.Id == settings.CommandChannelId)
                return;


            var activeRadios = radioService.GetAll().Where(x => x.Guild.Id == settings.RelatedGuild.Id).ToList();
            if (activeRadios.Select(x => x.VoiceChannelId).Contains(before.VoiceChannel.Id))
            {
                var voiceToDelete = activeRadios.FirstOrDefault(x => x.VoiceChannelId == before.VoiceChannel.Id);
                await logger.Info($"[RADIO_REMOVE] | Автоматическое удаление голосового канала {before.VoiceChannel.Name}");
                await radioService.Delete(voiceToDelete);
                await before.VoiceChannel.DeleteAsync();
            }
        }

        private async Task WarnRemover(object? sender, ElapsedEventArgs e, IBaseService<WarnEntity> warnService, IBaseService<GuildEntity> guildService, ILogToFileService logger)
        {
            var expiredWarns = warnService.GetAll().Where(x => x.IsActive && !x.IsPermanent && x.WarnType == Shared.Enums.WarnType.Ordinary && x.ToDate < DateTime.UtcNow).ToList() ?? new List<WarnEntity>();
            var expiredRebukes = warnService.GetAll().Where(x => x.IsActive && !x.IsPermanent && x.WarnType == Shared.Enums.WarnType.Rebuke && x.ToDate < DateTime.UtcNow).ToList() ?? new List<WarnEntity>();
            var expiredROs = warnService.GetAll().Where(x => x.IsActive && !x.IsPermanent && x.WarnType == Shared.Enums.WarnType.ReadOnly && x.ToDate < DateTime.UtcNow).ToList() ?? new List<WarnEntity>();
            Console.WriteLine(DateTime.UtcNow.ToString());

            if (!expiredWarns.Any() && !expiredROs.Any())
            {
                await logger.Info($"[WARN_REMOVE] | Не найдены предупреждения или RO");
                return;
            }
            foreach (var guild in guildService.GetAll().ToList())
            {
                var discrodGuild = _client.GetGuild(guild.GuildId);
                guilds.Add(discrodGuild);
            }

            if (!guilds.Any())
            {
                await logger.Info($"[WARN_REMOVE] | Не найдены сервера");
                return;
            }

            foreach (var warn in expiredWarns)
            {
                warn.IsActive = false;
                var warnCount = warnService.GetAll().Where(x => x.IssuedTo.Id == warn.IssuedToId && x.IsActive && !x.IsPermanent && x.WarnType == Shared.Enums.WarnType.Ordinary).ToList().Count;
                foreach (var guild in guilds)
                {
                    if (guild == null)
                    {
                        await logger.Info($"[WARN_REMOVE] | Сервер NULL");
                        continue;
                    }

                    try
                    {
                        var usr = guild.GetUser(warn.IssuedTo.DiscordId);
                        if (usr == null)
                        {
                            await logger.Info($"[WARN_REMOVE] | User NULL");
                            continue;
                        }
                        var currentSettings = warnSettings.FirstOrDefault(x => x.RelatedGuild.GuildId == guild.Id);
                        if (currentSettings == null)
                        {
                            Console.WriteLine($"[WARN_REMOVE] | Не найдены настройки");
                            continue;
                        }

                        var chnl = guild.GetTextChannel(currentSettings.WarnChannelId);
                        if (chnl != null)
                        {
                            var msg = await chnl.GetMessageAsync(warn.MessageId);
                            if (msg != null)
                            {
                                var embed = msg.Embeds.First().ToEmbedBuilder();
                                var removeField = embed.Fields.FirstOrDefault(x => x.Name == "Истекает");
                                if (removeField != null)
                                {
                                    embed.Fields.Remove(removeField);
                                    embed.AddField("Предупреждение снято", "Срок предупреждения истёк");
                                    embed.WithColor(Color.Green);
                                    await chnl.ModifyMessageAsync(warn.MessageId, msg => { msg.Embeds = new Embed[] { embed.Build() }; });
                                }
                            }
                        }

                        if (warnCount == 1)
                            await usr.RemoveRoleAsync(currentSettings.FirstWarnRoleId);
                        else if (warnCount == 2)
                            await usr.RemoveRoleAsync(currentSettings.SecondWarnRoleId);
                        else if (warnCount == 3)
                            await usr.RemoveRoleAsync(currentSettings.ThirdWarnRoleId);
                        await warnService.UpdateAsync(warn);
                        await logger.Info($"[WARN_REMOVE] | Снято предупреждение у пользователя {usr.Nickname}");
                    }
                    catch (Exception ex)
                    {
                        await logger.Error($"[WARN_REMOVE] | {ex.Message}");
                    }
                }
            }

            foreach (var warn in expiredRebukes)
            {
                warn.IsActive = false;
                var warnCount = warnService.GetAll().Where(x => x.IssuedTo.Id == warn.IssuedToId && x.IsActive && !x.IsPermanent && x.WarnType == Shared.Enums.WarnType.Rebuke).ToList().Count;
                foreach (var guild in guilds)
                {
                    if (guild == null)
                    {
                        await logger.Info($"[REBUKE_REMOVE] | Сервер NULL");
                        continue;
                    }

                    try
                    {
                        var usr = guild.GetUser(warn.IssuedTo.DiscordId);
                        if (usr == null)
                        {
                            await logger.Info($"[REBUKE_REMOVE] | User NULL");
                            continue;
                        }
                        var currentSettings = warnSettings.FirstOrDefault(x => x.RelatedGuild.GuildId == guild.Id);
                        if (currentSettings == null)
                        {
                            Console.WriteLine($"[REBUKE_REMOVE] | Не найдены настройки");
                            continue;
                        }

                        var chnl = guild.GetTextChannel(currentSettings.WarnChannelId);
                        if (chnl != null)
                        {
                            var msg = await chnl.GetMessageAsync(warn.MessageId);
                            if (msg != null)
                            {
                                var embed = msg.Embeds.First().ToEmbedBuilder();
                                var removeField = embed.Fields.FirstOrDefault(x => x.Name == "Истекает");
                                if (removeField != null)
                                {
                                    embed.Fields.Remove(removeField);
                                    embed.AddField("Выговор снят", "Срок выговора истёк");
                                    embed.WithColor(Color.Green);
                                    await chnl.ModifyMessageAsync(warn.MessageId, msg => { msg.Embeds = new Embed[] { embed.Build() }; });
                                }
                            }
                        }

                        if (warnCount == 1)
                            await usr.RemoveRoleAsync(currentSettings.FirstRebukeRoleId);
                        else if (warnCount == 2)
                            await usr.RemoveRoleAsync(currentSettings.SecondRebukeRoleId);
                        else if (warnCount == 3)
                            await usr.RemoveRoleAsync(currentSettings.ThirdRebukeRoleId);
                        await warnService.UpdateAsync(warn);
                        await logger.Info($"[REBUKE_REMOVE] | Снят выговор у пользователя {usr.Nickname}");
                    }
                    catch (Exception ex)
                    {
                        await logger.Error($"[REBUKE_REMOVE] | {ex.Message}");
                    }
                }
            }

            foreach (var warn in expiredROs)
            {
                warn.IsActive = false;
                foreach (var guild in guilds)
                {
                    if (guild == null)
                    {
                        await logger.Info($"[RO_REMOVE] | Сервер NULL");
                        continue;
                    }

                    try
                    {
                        var usr = guild.GetUser(warn.IssuedTo.DiscordId);
                        if (usr == null)
                        {
                            await logger.Info($"[RO_REMOVE] | User NULL");
                            continue;
                        }

                        var currentSettings = warnSettings.FirstOrDefault(x => x.RelatedGuild.GuildId == guild.Id);
                        if (currentSettings == null)
                        {
                            Console.WriteLine($"[RO_REMOVE] | Не найдены настройки");
                            continue;
                        }

                        var chnl = guild.GetTextChannel(currentSettings.WarnChannelId);
                        if (chnl != null)
                        {
                            var msg = await chnl.GetMessageAsync(warn.MessageId);
                            if (msg != null)
                            {
                                var embed = msg.Embeds.First().ToEmbedBuilder();
                                var removeField = embed.Fields.FirstOrDefault(x => x.Name == "Истекает");
                                if (removeField != null)
                                {
                                    embed.Fields.Remove(removeField);
                                    embed.AddField("RO снят", "Срок ReadOnly истёк");
                                    embed.WithColor(Color.Green);
                                    await chnl.ModifyMessageAsync(warn.MessageId, msg => { msg.Embeds = new Embed[] { embed.Build() }; });
                                }
                            }
                        }
                        await usr.RemoveRoleAsync(currentSettings.ReadOnlyRoleId);
                        await warnService.UpdateAsync(warn);
                        await logger.Info($"[RO_REMOVE] | Снято RO у пользователя {usr.Nickname}");
                    }
                    catch (Exception ex)
                    {
                        await logger.Error($"[RO_REMOVE] | {ex.Message}");
                    }
                }
            }
        }

        private async Task UpdateStatus(object? sender, ElapsedEventArgs e)
        {
            var server = new GameServer("185.207.214.51:2325");
            await server.PerformQueryAsync();
            await _client.SetCustomStatusAsync(string.Format("Онлайн {0}/{1}", server.Information.OnlinePlayers, server.Information.MaxPlayers));
        }
    }
}
