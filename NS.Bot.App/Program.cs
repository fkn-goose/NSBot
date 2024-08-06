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
using System.Timers;

namespace NS.Bot.App
{
    public class Program
    {
        private DiscordSocketClient _client;

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

            await provider.GetRequiredService<InteractionHandler>().InitializeAsync();

            //Model.logger = new ConsoleLogger();
            //Model.radioname = new Regex("\\d\\d\\d\\.\\d\\d\\d");

            //Timer dataTimer = new Timer(60000);
            //dataTimer.Elapsed += TimerEvent;
            //dataTimer.AutoReset = true;
            //dataTimer.Start();
            //await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "RefreshDataEvent", "Save timer started"));

            //_client.UserVoiceStateUpdated += VoiceRemover;
            //_client.MessageReceived += RndMessagesDelete;

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

        //private async Task RndMessagesDelete(SocketMessage message)
        //{
        //    if (!Model.Data.Channels.Radio.IsRadioEnabled)
        //        return;

        //    if (message.Author.IsBot || message.Author.IsWebhook)
        //        return;

        //    if (message.Channel.Id != Model.Data.Channels.Radio.RadioInitChannelId)
        //        return;

        //    await message.DeleteAsync();
        //    var msg = await message.Channel.SendMessageAsync("Для создания рации **напишите** комманду\n/частота");
        //    Timer dataTimer = new Timer(5000);
        //    dataTimer.AutoReset = false;
        //    dataTimer.Elapsed += (sender, e) => { DeleteRndMessage(sender, e, msg); };
        //    dataTimer.Start();
        //}

        private void DeleteRndMessage(object sender, ElapsedEventArgs e, RestUserMessage msg)
        {
            msg.DeleteAsync().Wait();
        }

        //private async void TimerEvent(object? sender, ElapsedEventArgs e)
        //{
        //    if (Model.Data.Tickets.TicketsData.Where(x => x.IsFinished)
        //                                      .Where(x => DateTime.Now > x.DeleteTime)
        //                                      .Any())

        //        foreach (var ticket in Model.Data.Tickets.TicketsData.Where(x => x.IsFinished)
        //                                                         .Where(x => DateTime.Now > x.DeleteTime))
        //        {
        //            _client.GetGuild(Model.Data.CurrentGuildId).GetTextChannel(ticket.ChannelId).DeleteAsync();
        //            Model.Data.Tickets.TicketsData.Remove(ticket);
        //        }

        //    File.WriteAllTextAsync("Data.json", JsonConvert.SerializeObject(Model.Data)).Wait();
        //    await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "Update", "Data updated!"));
        //}

        //private async Task VoiceRemover(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        //{
        //    if (!Model.Data.Channels.Radio.IsRadioEnabled)
        //        return;

        //    if (Model.Data.Channels.Radio.ActiveRadios == null)
        //        return;

        //    if (before.VoiceChannel == null || before.VoiceChannel == after.VoiceChannel)
        //        return;

        //    if (Model.Data.Channels.Radio.ActiveRadios.Contains(before.VoiceChannel.Id) && before.VoiceChannel.ConnectedUsers.Count == 0)
        //    {
        //        Model.Data.Channels.Radio.ActiveRadios.Remove(before.VoiceChannel.Id);
        //        await before.VoiceChannel.DeleteAsync();
        //        await Model.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"Частота {before.VoiceChannel.Name} удалена"));
        //    }
        //}

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
