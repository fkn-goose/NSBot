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
                _logToFileService.Info(string.Format("{0} | {1}({2}) успешно завершил модальную-комманду {3}", context.Guild.Name, context.User.Username, context.User.Id, info.Name));
            else
                _logToFileService.Error(string.Format("{0} | {1}({2}) ошибка при использовании модальной-комманды {3}. Ошибка - {4}. Причина - {5}", context.Guild.Name, context.User.Username, context.User.Id, info.Name, result.Error.Value.ToString() ?? "Неизвестна", result.ErrorReason));
            return Task.CompletedTask;
        }

        private Task ComponentCommandExecuted(ComponentCommandInfo info, IInteractionContext context, IResult result)
        {
            if (result.IsSuccess)
                _logToFileService.Info(string.Format("{0} | {1}({2}) успешно использовал {3}", context.Guild.Name, context.User.Username, context.User.Id, info.Name));
            else
                _logToFileService.Error(string.Format("{0} | {1}({2}) ошибка при использовании {3}. Ошибка - {4}. Причина - {5}", context.Guild.Name, context.User.Username, context.User.Id, info.Name, result.Error.Value.ToString() ?? "Неизвестна", result.ErrorReason));
            return Task.CompletedTask;
        }

        private Task ContextCommandExecuted(ContextCommandInfo info, IInteractionContext context, IResult result)
        {
            if (result.IsSuccess)
                _logToFileService.Info(string.Format("{0} | {1}({2}) успешно использовал контекст-комманду {3}", context.Guild.Name, context.User.Username, context.User.Id, info.Name));
            else
                _logToFileService.Error(string.Format("{0} | {1}({2}) ошибка при использовании контекст-комманды {3}. Ошибка - {4}. Причина - {5}", context.Guild.Name, context.User.Username, context.User.Id, info.Name, result.Error.Value.ToString() ?? "Неизвестна", result.ErrorReason));
            return Task.CompletedTask;
        }

        private Task SlashCommandExecuted(SlashCommandInfo info, IInteractionContext context, IResult result)
        {
            if (result.IsSuccess)
                _logToFileService.Info(string.Format("{0} | {1}({2}) успешно использовал слеш-комманду {3}", context.Guild.Name, context.User.Username, context.User.Id, info.Name));
            else
                _logToFileService.Error(string.Format("{0} | {1}({2}) ошибка при использовании слеш-комманды {3}. Ошибка - {4}. Причина - {5}", context.Guild.Name, context.User.Username, context.User.Id, info.Name, result.Error.Value.ToString() ?? "Неизвестна", result.ErrorReason));
            return Task.CompletedTask;
        }

        private async Task HandleIntercation(SocketInteraction interaction)
        {
            if (interaction.GuildId == null)
            {
                await interaction.RespondAsync("Мои команды не работают в личных сообщениях");
                return;
            }

            if (interaction is SocketSlashCommand)
            {
                var slash = interaction as SocketSlashCommand;
                var slashCommandParameters = slash.Data?.Options?.FirstOrDefault();
                var parameters = string.Empty;

                if (slashCommandParameters != null && slashCommandParameters.Options != null && slashCommandParameters.Options.Any())
                {
                    var objParameters = slashCommandParameters.Options.Select(x => x.Value).ToList();
                    foreach ( var param in objParameters )
                    {
                        if(param is string)
                            parameters += param + " ";
                        if (param is SocketGuildUser)
                            parameters += string.Format("{0}({1}) ", (param as SocketGuildUser).Username, (param as SocketGuildUser).Id);
                        if (param is SocketTextChannel)
                            parameters += string.Format("{0}({1}) ", (param as SocketTextChannel).Name, (param as SocketTextChannel).Id);
                        if (param is SocketVoiceChannel)
                            parameters += string.Format("{0}({1}) ", (param as SocketVoiceChannel).Name,(param as SocketVoiceChannel).Id);
                        if (param is SocketRole)
                            parameters += string.Format("{0}({1}) ", (param as SocketRole).Name, (param as SocketRole).Id);
                    }
                }
                await _logToFileService.Info(string.Format("{0} | {1}({2}) использовал слеш-комманду {3} {4}", (interaction.Channel as SocketGuildChannel).Guild.Name, slash.User.Username, slash.User.Id, slash.CommandName, parameters));
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
