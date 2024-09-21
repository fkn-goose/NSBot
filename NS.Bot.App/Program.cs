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
            var builder = Host.CreateDefaultBuilder(args);

            var path = Path.GetFullPath("appsettings.json");
            var config = new ConfigurationBuilder()
#if DEBUG
                .AddJsonFile(path, false)
#else
                .AddJsonFile("appsettings.json", false)
#endif
                .Build();

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

            await provider.GetRequiredService<InteractionHandler>().InitializeAsync();

            warnSettings = warnSettingsService.GetAll().ToList();

            var dataTimer = new System.Timers.Timer(30000);
            dataTimer.Elapsed += (sender, e) => WarnRemover(sender, e, warnService, guildService);
            dataTimer.AutoReset = true;
            dataTimer.Start();

            _client.MessageReceived += (message) => RndMessagesDelete(message, radioSettingsService);
            _client.UserVoiceStateUpdated += (user, before, after) => VoiceRemover(user, before, after, radioSettingsService, radioService);

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

        private async Task RndMessagesDelete(SocketMessage message, IRadioSettingsService radioSettingsService)
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

            System.Timers.Timer dataTimer = new System.Timers.Timer(5000);
            dataTimer.AutoReset = false;
            dataTimer.Elapsed += (sender, e) => { DeleteRndMessage(sender, e, msg); };
            dataTimer.Start();
        }

        private void DeleteRndMessage(object sender, ElapsedEventArgs e, RestUserMessage msg)
        {
            msg.DeleteAsync().Wait();
        }

        private static async Task VoiceRemover(SocketUser user, SocketVoiceState before, SocketVoiceState after, IRadioSettingsService radioSettingsService, IBaseService<RadioEntity> radioService)
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
                await radioService.Delete(voiceToDelete);
                await before.VoiceChannel.DeleteAsync();
            }
        }

        private async void WarnRemover(object? sender, ElapsedEventArgs e, IBaseService<WarnEntity> warnService, IBaseService<GuildEntity> guildService)
        {
            var expiredWarns = warnService.GetAll().Where(x => x.IsActive && !x.IsVerbal && !x.IsPermanent && !x.IsReadOnly && x.ToDate < DateTime.UtcNow).ToList();
            var expiredROs = warnService.GetAll().Where(x => x.IsActive && !x.IsVerbal && !x.IsPermanent && x.IsReadOnly && x.ToDate < DateTime.UtcNow).ToList();
            if (!expiredWarns.Any() && !expiredROs.Any())
                return;

            foreach (var guild in guildService.GetAll().ToList())
            {
                var discrodGuild = _client.GetGuild(guild.GuildId);
                guilds.Add(discrodGuild);
            }

            if (!guilds.Any())
                return;

            foreach (var warn in expiredWarns)
            {
                warn.IsActive = false;
                var warnCount = warn.IssuedTo.Warns.Count;

                foreach (var guild in guilds)
                {
                    var usr = guild.GetUser(warn.IssuedTo.DiscordId);
                    if (usr == null)
                        continue;

                    var currentSettings = warnSettings.FirstOrDefault(x => x.RelatedGuild.GuildId == guild.Id);
                    if (currentSettings == null)
                        continue;

                    var chnl = guild.GetTextChannel(currentSettings.WarnChannelId);
                    if (chnl != null)
                    {
                        var msg = await chnl.GetMessageAsync(warn.MessageId);
                        var embed = msg.Embeds.First().ToEmbedBuilder();
                        var removeField = embed.Fields.FirstOrDefault(x => x.Name == "Истекает");
                        if (removeField != null)
                        {
                            embed.Fields.Remove(removeField);
                            embed.AddField("Истекает", "Срок предупреждения истёк");
                            embed.WithColor(Color.Green);
                            await chnl.ModifyMessageAsync(warn.MessageId, msg => { msg.Embeds = new Embed[] { embed.Build() }; });
                        }
                    }
                    if (warnCount == 1)
                        await usr.RemoveRoleAsync(currentSettings.FirstWarnRoleId);
                    else if (warnCount == 2)
                        await usr.RemoveRoleAsync(currentSettings.SecondWarnRoleId);
                    else if (warnCount == 3)
                        await usr.RemoveRoleAsync(currentSettings.ThirdWarnRoleId);
                    warnService.UpdateAsync(warn);
                }
            }

            foreach (var warn in expiredROs)
            {
                warn.IsActive = false;
                var warnCount = warn.IssuedTo.Warns.Count;

                foreach (var guild in guilds)
                {
                    var usr = guild.GetUser(warn.IssuedTo.DiscordId);
                    if (usr == null)
                        continue;

                    var currentSettings = warnSettings.FirstOrDefault(x => x.RelatedGuild.GuildId == guild.Id);
                    if (currentSettings == null)
                        continue;

                    var chnl = guild.GetTextChannel(currentSettings.WarnChannelId);
                    if (chnl != null)
                    {
                        var msg = await chnl.GetMessageAsync(warn.MessageId);
                        var embed = msg.Embeds.First().ToEmbedBuilder();
                        var removeField = embed.Fields.FirstOrDefault(x => x.Name == "Истекает");
                        if (removeField != null)
                        {
                            embed.Fields.Remove(removeField);
                            embed.AddField("Истекает", "Срок ReadOnly истёк");
                            embed.WithColor(Color.Green);
                            await chnl.ModifyMessageAsync(warn.MessageId, msg => { msg.Embeds = new Embed[] { embed.Build() }; });
                        }
                    }
                    await usr.RemoveRoleAsync(currentSettings.ReadOnlyRoleId);
                    warnService.UpdateAsync(warn);
                }
            }
        }
    }
}
