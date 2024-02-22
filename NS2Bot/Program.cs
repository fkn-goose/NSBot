using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NS2Bot.Handlers;
using NS2Bot.Logging;
using NS2Bot.Models;

namespace NS2Bot
{
    public class Program
    {
        private DiscordSocketClient _client;
        //Add global variable json
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

            _client.ModalSubmitted += ModalEventHandler;

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

        public async Task ModalEventHandler(SocketModal modal)
        {
            if (modal.Type == InteractionType.ModalSubmit)
            {
                switch (modal.Data.CustomId)
                {
                    case "createTicketMenu":
                        var configContext = File.ReadAllText("config.json");
                        ConfigModel model = JsonConvert.DeserializeObject<ConfigModel>(configContext);
                        int ticketCount = model.HelperTicketsCount;
                        model.HelperTicketsCount++;
                        await File.WriteAllTextAsync("config.json", JsonConvert.SerializeObject(model));

                        var guild = _client.GetGuild(modal.GuildId.Value);
                        var ticketChannel = await guild.CreateTextChannelAsync($"Хелпер-тикет-{ticketCount}", prop => prop.CategoryId = model.Category.NewHelperTicketsCategoryId);

                        await ticketChannel.SyncPermissionsAsync();
                        await ticketChannel.AddPermissionOverwriteAsync(modal.User, new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow));

                        var embedChannelTicket = new EmbedBuilder()
                           .WithTitle($"Тикет #{model.HelperTicketsCount}")
                           .WithDescription(modal.Data.Components.First(x => x.CustomId == "reason").Value)
                           .WithColor(Color.Blue);

                        var buttonCloseTicket = new ButtonBuilder()
                        {
                            CustomId = "closeHelperTicket",
                            Label = "Закрыть обращение",
                            Style = ButtonStyle.Primary
                        };

                        var component = new ComponentBuilder();
                        component.WithButton(buttonCloseTicket);

                        await ticketChannel.SendMessageAsync(embed: embedChannelTicket.Build(), components: component.Build());
                        var ticketMenu = guild.GetTextChannel(model.Category.HelperTicketsChannelId);

                        var embedHelperTicket = new EmbedBuilder()
                            .WithTitle($"Тикет #{ticketCount}")
                            .WithColor(Color.DarkOrange);

                        var buttonTakeTicket = new ButtonBuilder()
                        {
                            CustomId = "takeTicket",
                            Label = "Взять тикет в работу",
                            Style = ButtonStyle.Primary
                        };

                        component = new ComponentBuilder();
                        component.WithButton(buttonTakeTicket);
                        var helperButton = await ticketMenu.SendMessageAsync(embed: embedHelperTicket.Build(), components: component.Build());

                        model.MessageChannelTickerPair.Add(helperButton.Id, ticketChannel.Id);

                        await modal.RespondAsync();
                        break;
                }
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
