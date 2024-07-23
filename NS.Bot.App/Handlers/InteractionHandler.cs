using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;

namespace NS.Bot.App.Handlers
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _serviceProvider;

        public InteractionHandler(DiscordSocketClient client, InteractionService interactionService, IServiceProvider serviceProvider)
        {
            _client = client;
            _interactionService = interactionService;
            _serviceProvider = serviceProvider;
        }

        public async Task InitializeAsync()
        {
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

            _client.InteractionCreated += HandleIntercation;
            _interactionService.SlashCommandExecuted += SlashCommandExecuted;
            _interactionService.ContextCommandExecuted += ContextCommandExecuted;
            _interactionService.ComponentCommandExecuted += ComponentCommandExecuted;
            _interactionService.ModalCommandExecuted += ModalCommandExecuted;
        }

        private Task ModalCommandExecuted(ModalCommandInfo info, IInteractionContext context, IResult result)
        {
            return Task.CompletedTask;
        }

        private Task ComponentCommandExecuted(ComponentCommandInfo info, IInteractionContext context, IResult result)
        {
            return Task.CompletedTask;
        }

        private Task ContextCommandExecuted(ContextCommandInfo info, IInteractionContext context, IResult result)
        {
            return Task.CompletedTask;
        }

        private Task SlashCommandExecuted(SlashCommandInfo info, IInteractionContext context, IResult result)
        {
            return Task.CompletedTask;
        }

        private async Task HandleIntercation(SocketInteraction interaction)
        {
            try
            {
                var ctx = new SocketInteractionContext(_client, interaction);
                await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (interaction.Type == InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}
