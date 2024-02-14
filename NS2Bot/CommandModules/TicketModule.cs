using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using NS2Bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NS2Bot.CommandModules
{
    public class TicketModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ulong newHelperTicketsId = 1207408294885330975;
        private readonly ulong oldTicketsId = 1207428755862323260;

        [SlashCommand("createticketmenu", "Создает меню тикетов в данном канале")]
        [RequireOwner]
        public async Task CreateTicketMenu()
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Здесь вы можете создать тикет с обращением к администрации")
                .WithColor(Color.Green);

            var buttonBuilder = new ButtonBuilder() 
            {
                CustomId = "createHelperTicket",
                Label = "Создать обращение к хелперу",
                Style = ButtonStyle.Danger
            };

            var component = new ComponentBuilder();
            component.WithButton(buttonBuilder);

            await RespondAsync(embed: embedBuilder.Build(), components: component.Build());
        }

        [ComponentInteraction("createHelperTicket")]
        public async Task HelperButtonInput()
        {
            FileStream stream = new FileStream("config.json", FileMode.OpenOrCreate);
            TicketCountModel model = JsonSerializer.Deserialize<TicketCountModel>(JsonDocument.Parse(stream));

            var channel = await Context.Guild.CreateTextChannelAsync($"Хелпер-тикет-{model.HelperTicketsCount}", prop => prop.CategoryId = newHelperTicketsId);
            await channel.SyncPermissionsAsync();
            await channel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel:PermValue.Allow));

            var embedBuilder = new EmbedBuilder()
                .WithTitle("Test Title")
                .WithDescription("Test Description")
                .WithColor(Color.Blue);

            var buttonBuilder = new ButtonBuilder()
            {
                CustomId = $"closeHelperTicket",
                Label = "Закрыть обращение",
                Style = ButtonStyle.Primary
            };

            var component = new ComponentBuilder();
            component.WithButton(buttonBuilder);

            await channel.SendMessageAsync(embed: embedBuilder.Build(), components: component.Build());
            await RespondAsync();

            model.HelperTicketsCount++;
            stream.Dispose();
            File.WriteAllText("config.json",JsonSerializer.Serialize<TicketCountModel>(model));
        }

        [ComponentInteraction("closeHelperTicket")]
        public async Task CloseHelperTicket()
        {
            var channel = Context.Guild.GetTextChannel(Context.Channel.Id);
            await channel.ModifyAsync(prop => prop.CategoryId = oldTicketsId);
            await channel.RemovePermissionOverwriteAsync(Context.User);
            await channel.SyncPermissionsAsync();

            await RespondAsync();
        }
    }
}
