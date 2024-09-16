using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using NS.Bot.BuisnessLogic.Interfaces;
using System.Reflection;

namespace NS.Bot.App.Handlers
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogToFileService _logToFileService;

        public InteractionHandler(DiscordSocketClient client, InteractionService interactionService, IServiceProvider serviceProvider, ILogToFileService logToFileService)
        {
            _client = client;
            _interactionService = interactionService;
            _serviceProvider = serviceProvider;
            _logToFileService = logToFileService;
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
            if (result.IsSuccess)
                _logToFileService.Info(string.Format("{0}({1}) успешно использовал {2}", context.User.Username, context.User.Id, info.Name));
            else
                _logToFileService.Error(string.Format("{0}({1}) ошибка при использовании {2}. Ошибка - {3}. Причина - {4}", context.User.Username, context.User.Id, info.Name, result.Error.Value.ToString() ?? "Неизвестна", result.ErrorReason));
            return Task.CompletedTask;
        }

        private Task ComponentCommandExecuted(ComponentCommandInfo info, IInteractionContext context, IResult result)
        {
            if (result.IsSuccess)
                _logToFileService.Info(string.Format("{0}({1}) успешно использовал {2}", context.User.Username, context.User.Id, info.Name));
            else
                _logToFileService.Error(string.Format("{0}({1}) ошибка при использовании {2}. Ошибка - {3}. Причина - {4}", context.User.Username, context.User.Id, info.Name, result.Error.Value.ToString() ?? "Неизвестна", result.ErrorReason));
            return Task.CompletedTask;
        }

        private Task ContextCommandExecuted(ContextCommandInfo info, IInteractionContext context, IResult result)
        {
            if (result.IsSuccess)
                _logToFileService.Info(string.Format("{0}({1}) успешно использовал {2}", context.User.Username, context.User.Id, info.Name));
            else
                _logToFileService.Error(string.Format("{0}({1}) ошибка при использовании {2}. Ошибка - {3}. Причина - {4}", context.User.Username, context.User.Id, info.Name, result.Error.Value.ToString() ?? "Неизвестна", result.ErrorReason));
            return Task.CompletedTask;
        }

        private Task SlashCommandExecuted(SlashCommandInfo info, IInteractionContext context, IResult result)
        {
            if (result.IsSuccess)
                _logToFileService.Info(string.Format("{0}({1}) успешно использовал {2}", context.User.Username, context.User.Id, info.Name));
            else
                _logToFileService.Error(string.Format("{0}({1}) ошибка при использовании {2}. Ошибка - {3}. Причина - {4}", context.User.Username, context.User.Id, info.Name, result.Error.Value.ToString() ?? "Неизвестна", result.ErrorReason));
            return Task.CompletedTask;
        }

        private async Task HandleIntercation(SocketInteraction interaction)
        {
            if(interaction is SocketSlashCommand)
            {
                var slash = interaction as SocketSlashCommand;
                _logToFileService.Info(string.Format("{0} | {1}({2}) использовал {3}", (interaction.Channel as SocketGuildChannel).Guild.Name, slash.User.Username, slash.User.Id, slash.CommandName));
            }

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
