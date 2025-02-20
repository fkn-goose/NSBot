﻿using Discord.Interactions;
using NS.Bot.BuisnessLogic.Interfaces;
using System.Threading.Tasks;

namespace NS.Bot.Commands.CommandModules
{
    public class InitModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IGuildService _guildService;
        public InitModule(IGuildService guildService)
        {
            _guildService = guildService;
        }

        [SlashCommand("init", "Инициализация сервера")]
        public async Task InitServer()
        {
            _guildService.Create(new Shared.Entities.GuildEntity()
            {
                GuildId = Context.Guild.Id,
                Name = Context.Guild.Name,
            });

            await RespondAsync("Сервер инициализирован", ephemeral: true);
        }
    }
}
