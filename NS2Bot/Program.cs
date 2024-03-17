﻿using Discord;
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
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Timers;
using Timer = System.Timers.Timer;

namespace NS2Bot
{
    public static class MainData
    {
        public static ConfigModel configData;
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

            MainData.logger = new ConsoleLogger();
            var configContext = File.ReadAllText("config.json");
            MainData.configData = JsonConvert.DeserializeObject<ConfigModel>(configContext);
            MainData.radioname = new Regex("\\d\\d\\d\\.\\d\\d\\d");

            Timer dataTimer = new Timer(60000);
            dataTimer.Elapsed += RefreshDataEvent;
            dataTimer.AutoReset = true;
            dataTimer.Start();
            await MainData.logger.LogAsync(new LogMessage(LogSeverity.Info, "RefreshDataEvent", "Save timer started"));

            _client.ModalSubmitted += ModalEventHandler;
            _client.UserVoiceStateUpdated += VoiceRemover;
            _client.MessageReceived += RndMessagesDelete;

            _client.Log += _ => provider.GetRequiredService<ConsoleLogger>().LogAsync(_);
            commands.Log += _ => provider.GetRequiredService<ConsoleLogger>().LogAsync(_);

            _client.Ready += async () =>
            {
                //if (IsDebug())
                await commands.RegisterCommandsToGuildAsync(UInt64.Parse(config["testGuild"]), true);
                //else
                //    await commands.RegisterCommandsGloballyAsync(true);
            };

            MainData.publicPdaWebHook = config["webhooks:publicPDA"];
            MainData.systemInfoWebHook = config["webhooks:systemInfo"];

            await _client.LoginAsync(Discord.TokenType.Bot, config["tokens:discord"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task RndMessagesDelete(SocketMessage message)
        {
            if (!MainData.configData.IsRadioEnabled)
                return;

            if (message.Author.IsBot || message.Author.IsWebhook)
                return;

            if (message.Channel.Id != MainData.configData.Category.RadioInitChannelId)
                return;

            await message.DeleteAsync();
            var msg = await message.Channel.SendMessageAsync("Для создания рации **напишите** комманду\n/частота");
            Timer dataTimer = new Timer(5000);
            dataTimer.AutoReset = false;
            dataTimer.Elapsed +=(sender, e) => { DeleteRndMessage(sender, e, msg); };
            dataTimer.Start();
        }

        private void DeleteRndMessage(object sender, ElapsedEventArgs e, RestUserMessage msg)
        {
            msg.DeleteAsync().Wait();
        }

        private async void RefreshDataEvent(object? sender, ElapsedEventArgs e)
        {
            File.WriteAllTextAsync("config.json", JsonConvert.SerializeObject(MainData.configData)).Wait();
            await MainData.logger.LogAsync(new LogMessage(LogSeverity.Info, "Update", "Data updated!"));
        }

        private async Task VoiceRemover(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (!MainData.configData.IsRadioEnabled)
                return;

            if (MainData.configData.Category.ActiveRadios == null)
                return;

            if (before.VoiceChannel == null || before.VoiceChannel == after.VoiceChannel)
                return;

            if (MainData.configData.Category.ActiveRadios.Contains(before.VoiceChannel.Id) && before.VoiceChannel.ConnectedUsers.Count == 0)
            {
                MainData.configData.Category.ActiveRadios.Remove(before.VoiceChannel.Id);
                await before.VoiceChannel.DeleteAsync();
                await MainData.logger.LogAsync(new LogMessage(LogSeverity.Info, "VoiceC", $"Частота {before.VoiceChannel.Name} удалена"));
            }
        }

        public Task ModalEventHandler(SocketModal modal)
        {
            if (modal.Type == InteractionType.ModalSubmit)
            {
                switch (modal.Data.CustomId)
                {
                    case "createHelperTicket":
                        Task.Run(async () =>
                        {
                            var ticketCount = MainData.configData.HelperTicketsCount;
                            MainData.configData.HelperTicketsCount++;

                            var guild = _client.GetGuild(modal.GuildId.Value);
                            var ticketChannel = await guild.CreateTextChannelAsync($"Хелпер-тикет-{ticketCount}", prop => prop.CategoryId = MainData.configData.Category.NewHelperTicketsCategoryId);

                            await ticketChannel.SyncPermissionsAsync();
                            await ticketChannel.AddPermissionOverwriteAsync(modal.User, new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow));

                            var embedChannelTicket = new EmbedBuilder()
                               .WithTitle($"Тикет #{ticketCount}")
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
                            var ticketMenu = guild.GetTextChannel(MainData.configData.Category.HelperTicketsChannelId);

                            var embedHelperTicket = new EmbedBuilder()
                                .WithTitle($"Тикет #{ticketCount} [Открыто]")
                                .WithDescription($"Обращение создал - {MentionUtils.MentionUser(modal.User.Id)}")
                                .WithCurrentTimestamp()
                                .WithColor(Color.LightGrey);

                            var buttonTakeTicket = new ButtonBuilder()
                            {
                                CustomId = "takeTicket",
                                Label = "Взять тикет в работу",
                                Style = ButtonStyle.Primary
                            };

                            component = new ComponentBuilder();
                            component.WithButton(buttonTakeTicket);
                            var helperButton = await ticketMenu.SendMessageAsync(embed: embedHelperTicket.Build(), components: component.Build());
                            await modal.RespondAsync($"Тикет создан, {MentionUtils.MentionChannel(ticketChannel.Id)}. Как только появится свободный хелпер, он возьмет его в работу.", ephemeral: true)
                                .ContinueWith(task =>
                                {
                                    if (MainData.configData.MessageChannelTickerPair == null)
                                        MainData.configData.MessageChannelTickerPair = new Dictionary<ulong, ulong>();

                                    //Добавляю ключ "Открытый тикет" к каналу тикета
                                    MainData.configData.MessageChannelTickerPair.Add(helperButton.Id, ticketChannel.Id);
                                });
                        });
                        return Task.CompletedTask;

                    default:
                        return Task.CompletedTask;
                }
            }
            return Task.CompletedTask;
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
