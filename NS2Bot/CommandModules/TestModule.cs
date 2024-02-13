using Discord;
using Discord.Interactions;

namespace NS2Bot.CommandModules
{
    public class TestModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("say", "jopa")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task Say(string text) => ReplyAsync(text);

        [SlashCommand("components", "lookshit")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Components()
        {
            var button = new ButtonBuilder()
            {
                Label = "Button",
                CustomId = "customButton",
                Style = ButtonStyle.Danger
            };

            var menu = new SelectMenuBuilder()
            {
                CustomId = "startmenu",
                Placeholder = "Sample Menu"
            };

            menu.AddOption("First", "first");
            menu.AddOption("Second", "second");

            var component = new ComponentBuilder();
            component.WithButton(button);
            component.WithSelectMenu(menu);

            await RespondAsync("teset", components: component.Build());
        }
    }
}
