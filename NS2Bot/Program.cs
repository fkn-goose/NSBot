using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NS2Bot.CommandModules;
using NS2Bot.Handlers;
using NS2Bot.Logging;

namespace NS2Bot
{
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
                    GatewayIntents = Discord.GatewayIntents.AllUnprivileged,
                    LogGatewayIntentWarnings = false,
                    AlwaysDownloadUsers = true,
                    LogLevel = Discord.LogSeverity.Debug
                }))
                .AddTransient<ConsoleLogger>()
                .AddSingleton(x=>new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton(x=>new CommandService(new CommandServiceConfig
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

            _client.Log += _ => provider.GetRequiredService<ConsoleLogger>().Log(_);
            commands.Log += _ => provider.GetRequiredService<ConsoleLogger>().Log(_);

            _client.Ready += async () =>
            {
                if (IsDebug())
                    await commands.RegisterCommandsToGuildAsync(UInt64.Parse(config["testGuild"]), true);
                else
                    await commands.RegisterCommandsGloballyAsync(true);
            };

            await _client.LoginAsync(Discord.TokenType.Bot, config["tokens:discord"]);
            await _client.StartAsync();

            await Task.Delay(-1);
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
