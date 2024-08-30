using Microsoft.Extensions.DependencyInjection;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.BuisnessLogic.Services;
using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Group;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Entities.Radio;

namespace NS.Bot.BuisnessLogic
{
    public static class Installer
    {
        public static void AddBuisnessServices(this IServiceCollection container)
        {
            #region BaseCrud

            container.AddScoped<IBaseService<GuildEntity>, BaseService<GuildEntity>>();
            container.AddScoped<IBaseService<GuildMember>, BaseService<GuildMember>>();
            container.AddScoped<IBaseService<MemberEntity>, BaseService<MemberEntity>>();
            container.AddScoped<IBaseService<GroupEntity>,  BaseService<GroupEntity>>();
            container.AddScoped<IBaseService<RadioEntity>, BaseService<RadioEntity>>();
            container.AddScoped<IBaseService<RadioSettings>, BaseService<RadioSettings>>();

            #endregion

            container.AddScoped<IGuildService, GuildService>();
            container.AddScoped<IGuildMemberService, GuildMemberService>();
            container.AddScoped<ITicketService, TicketService>();
            container.AddScoped<IGroupService, GroupService>();
            container.AddScoped<IMemberService, MemberService>();
            container.AddScoped<IRadioSettingsService, RadioSettingsService>();
        }
    }
}