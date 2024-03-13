using Discord;
using Discord.Interactions;

namespace NS2Bot.CommandModules
{
    public class GroupModule : InteractionModuleBase<SocketInteractionContext>
    {
        //[SlashCommand("addgroup", "test")]
        ////[RequireUserPermission(Discord.GuildPermission.Administrator)]
        //[RequireOwner]
        //public async Task AddNewGroup([Summary(name: "Название группировки")] string groupName)
        //{
        //    uint groupId;
        //    if (MainData.configData.Groups == null)
        //    {
        //        MainData.configData.Groups = new List<Models.ConfigModel.Group>();
        //        groupId = 1;
        //    }
        //    else
        //        groupId = MainData.configData.Groups.Last().Id + 1;

        //    MainData.configData.Groups.Add(new Models.ConfigModel.Group()
        //    {
        //        Name = groupName,
        //        Id = groupId
        //    });

        //    await RespondAsync($"Создана группировка {groupName}", ephemeral: true);
        //}

        [SlashCommand("getgroups", "test")]
        [RequireOwner]
        public async Task GetGroups()
        {
            string groupList = string.Empty;
            foreach (var group in MainData.configData.Groups)
                groupList += group.Name + $" ID:{group.Id}\n";
            var groupEmbed = new EmbedBuilder()
            .WithTitle("Список группировок")
            .WithDescription(groupList)
            .WithColor(Color.Blue);

            await RespondAsync(embed: groupEmbed.Build());
        }

        [SlashCommand("addmember", "test")]
        [RequireOwner]
        public async Task AddToGroup([Summary(name: "Айди группировки")] uint groupId, Discord.IUser user)
        {
            await RespondAsync("test", ephemeral: true);
        }

        [SlashCommand("removegroup", "test")]
        //[RequireUserPermission(Discord.GuildPermission.Administrator)]
        [RequireOwner]
        public async Task RemoveGroup([Summary(name: "Айди группы")] uint groupId)
        {
            var removeGroup = MainData.configData.Groups.Where(x => x.Id == groupId).FirstOrDefault();
            if (removeGroup == null)
                await RespondAsync("Группировки с таким айди нет", ephemeral: true);

            MainData.configData.Groups.Remove(removeGroup);
            await RespondAsync("Группировка успшено удалена", ephemeral: true);
        }
    }
}
