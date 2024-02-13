using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace NS2Bot.Handlers
{
    public class PrefixHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IConfigurationRoot _config;

        public PrefixHandler(DiscordSocketClient client, CommandService commandService, IConfigurationRoot config)
        {
            _client = client;
            _commandService = commandService;
            _config = config;
        }

        public async Task InitializeAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
        }

        public void AddModule<T>()
        {
            _commandService.AddModuleAsync<T>(null);
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            var msg = message as SocketUserMessage;
            if (msg == null) 
                return;

            int argPos = 0;
            SocketGuildUser socketGuildUser = message.Author as SocketGuildUser;

            if (!(msg.HasCharPrefix(_config["prefix"][0], ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos)) || message.Author.IsBot) 
                return;

            var context = new SocketCommandContext(_client, msg);

            await _commandService.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);
        }
    }
}
