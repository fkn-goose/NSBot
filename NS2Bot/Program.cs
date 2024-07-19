using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NS2Bot.Handlers;
using NS2Bot.Logging;
using NS2Bot.Models;
using System.Text.RegularExpressions;
using System.Timers;
using Timer = System.Timers.Timer;

namespace NS2Bot
{
    public static class Model
    {
        public static BotData Data;
        public static ConsoleLogger logger;
        public static string publicPdaWebHook;
        public static string systemInfoWebHook;
        public static Regex radioname;
    }
    public class Program
    {
        private DiscordSocketClient _client;
        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile("config.yml")
                .Build();

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                services.AddSingleton(config)
                .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.MessageContent,
                    LogGatewayIntentWarnings = false,
                    AlwaysDownloadUsers = true,
                    LogLevel = Discord.LogSeverity.Debug
                }))
                .AddTransient<ConsoleLogger>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton(x => new CommandService(new CommandServiceConfig
                {
                    LogLevel = Discord.LogSeverity.Debug,
                    DefaultRunMode = Discord.Commands.RunMode.Async
                })))
                .Build();

            await RunAsync(host);
        }

        public async Task RunAsync(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var commands = provider.GetRequiredService<InteractionService>();
            _client = provider.GetRequiredService<DiscordSocketClient>();
            var config = provider.GetRequiredService<IConfigurationRoot>();

            await provider.GetRequiredService<InteractionHandler>().InitializeAsync();

            Model.logger = new ConsoleLogger();
            var configContext = File.ReadAllText("Data.json");
            Model.Data = JsonConvert.DeserializeObject<BotData>(configContext);

            #region DataInit

            if (Model.Data.Channels == null)
                Model.Data.Channels = new Channels()
                {
                    PDA = new PDA(),
                    Radio = new Radio() { ActiveRadios = new List<ulong>() }
                };

            if (Model.Data.Tickets == null)
                Model.Data.Tickets = new Ticket()
                {
                    TicketSettings = new TicketSettings(),
                    TicketsData = new List<TicketData>()
                };

            if (Model.Data.Groups == null)
                Model.Data.Groups = new List<BotData.Group>
                {
                    new BotData.Group()
                    {
                        Members = new List<ulong>()
                    }
                };

            #endregion

            Model.radioname = new Regex("\\d\\d\\d\\.\\d\\d\\d");

            Timer dataTimer = new Timer(60000);
            dataTimer.Elapsed += TimerEvent;
            dataTimer.AutoReset = true;
            dataTimer.Start();
            await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "RefreshDataEvent", "Save timer started"));

            _client.UserVoiceStateUpdated += VoiceRemover;
            _client.MessageReceived += RndMessagesDelete;

            _client.Log += _ => provider.GetRequiredService<ConsoleLogger>().LogAsync(_);
            commands.Log += _ => provider.GetRequiredService<ConsoleLogger>().LogAsync(_);

            _client.Ready += async () =>
            {
                //if (IsDebug())
                Model.Data.CurrentGuildId = ulong.Parse(config["testGuild"]);
                await commands.RegisterCommandsToGuildAsync(Model.Data.CurrentGuildId, true);
                //else
                //    await commands.RegisterCommandsGloballyAsync(true);
            };

            Model.publicPdaWebHook = config["webhooks:publicPDA"];
            Model.systemInfoWebHook = config["webhooks:systemInfo"];

            await _client.LoginAsync(Discord.TokenType.Bot, config["tokens:discord"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task RndMessagesDelete(SocketMessage message)
        {
            if (!Model.Data.Channels.Radio.IsRadioEnabled)
                return;

            if (message.Author.IsBot || message.Author.IsWebhook)
                return;

            if (message.Channel.Id != Model.Data.Channels.Radio.RadioInitChannelId)
                return;

            await message.DeleteAsync();
            var msg = await message.Channel.SendMessageAsync("Для создания рации **напишите** комманду\n/частота");
            Timer dataTimer = new Timer(5000);
            dataTimer.AutoReset = false;
            dataTimer.Elapsed += (sender, e) => { DeleteRndMessage(sender, e, msg); };
            dataTimer.Start();
        }

        private void DeleteRndMessage(object sender, ElapsedEventArgs e, RestUserMessage msg)
        {
            msg.DeleteAsync().Wait();
        }

        private async void TimerEvent(object? sender, ElapsedEventArgs e)
        {
            if (Model.Data.Tickets.TicketsData.Where(x => x.IsFinished)
                                              .Where(x => DateTime.Now > x.DeleteTime)
                                              .Any())

                foreach (var ticket in Model.Data.Tickets.TicketsData.Where(x => x.IsFinished)
                                                                 .Where(x => DateTime.Now > x.DeleteTime))
                {
                    _client.GetGuild(Model.Data.CurrentGuildId).GetTextChannel(ticket.ChannelId).DeleteAsync();
                    Model.Data.Tickets.TicketsData.Remove(ticket);
                }

            File.WriteAllTextAsync("Data.json", JsonConvert.SerializeObject(Model.Data)).Wait();
            await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "Update", "Data updated!"));
        }

        private async Task VoiceRemover(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (!Model.Data.Channels.Radio.IsRadioEnabled)
                return;

            if (Model.Data.Channels.Radio.ActiveRadios == null)
                return;

            if (before.VoiceChannel == null || before.VoiceChannel == after.VoiceChannel)
                return;

            if (Model.Data.Channels.Radio.ActiveRadios.Contains(before.VoiceChannel.Id) && before.VoiceChannel.ConnectedUsers.Count == 0)
            {
                Model.Data.Channels.Radio.ActiveRadios.Remove(before.VoiceChannel.Id);
                await before.VoiceChannel.DeleteAsync();
                await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"Частота {before.VoiceChannel.Name} удалена"));
            }
        }

        private static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}
