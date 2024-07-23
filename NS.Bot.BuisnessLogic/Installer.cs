using Microsoft.Extensions.DependencyInjection;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.BuisnessLogic.Services;

namespace NS.Bot.BuisnessLogic
{
    public static class Installer
    {
        public static void AddBuisnessServices(this IServiceCollection container)
        {
            container.AddScoped<IGuildService, GuildService>();
            container.AddScoped<ITicketService, TicketService>();
        }
    }
}