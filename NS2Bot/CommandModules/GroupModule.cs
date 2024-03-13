using Discord;
using Discord.Interactions;
using NS2Bot.Enums;
using NS2Bot.Extensions;
using System.ComponentModel;

namespace NS2Bot.CommandModules
{
    public class GroupModule : InteractionModuleBase<SocketInteractionContext>
    {
        //[SlashCommand("addgroup", "test")]
        ////[RequireUserPermission(Discord.GuildPermission.Administrator)]
        //[RequireOwner]
        //public async Task AddNewGroup([Summary(name: "Группировка", description:"Название группировки")] GroupsEnum groupName)
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
        //        Name = GetDescription(groupName),
        //        Id = grou
        //    });

        //    await RespondAsync($"Создана группировка {groupName}", ephemeral: true);
        //}

        //[SlashCommand("getgroups", "test")]
        //[RequireOwner]
        //public async Task GetGroups()
        //{
        //    string groupList = string.Empty;
        //    foreach (var group in MainData.configData.Groups)
        //        groupList += group.Name + $" ID:{group.Id}\n";
        //    var groupEmbed = new EmbedBuilder()
        //    .WithTitle("Список группировок")
        //    .WithDescription(groupList)
        //    .WithColor(Color.Blue);

        //    await RespondAsync(embed: groupEmbed.Build());
        //}

        [SlashCommand("addgroupmember", "Добавить игрока в группировку")]
        public async Task AddToGroup([Summary(name: "Группировка", description: "Название группировки")] GroupsEnum groupsEnum, [Summary(name: "Пользователь", description: "Пользователь")] IUser user)
        {
            if (MainData.configData.Groups == null)
                MainData.configData.Groups = new List<Models.ConfigModel.Group>();

            if (MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault() == null)
                MainData.configData.Groups.Add(new Models.ConfigModel.Group()
                {
                    Id = (uint)groupsEnum,
                    Members = new List<ulong>()
                    { }
                });
            
            if (MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault().Members.Contains(user.Id))
            {
                await RespondAsync("Игрок уже в группировке", ephemeral: true);
                return;
            }

            MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault().Members.Add(user.Id);

            await RespondAsync($"Игрок успешно добавлен к группировке {GetValueExtension.GetDescription(groupsEnum)}", ephemeral: true);
        }

        [SlashCommand("removegroupmember", "Убрать игрока из группировки")]
        public async Task RemoveFromGroup([Summary(name: "Группировка", description: "Название группировки")] GroupsEnum groupsEnum, [Summary(name: "Пользователь", description: "Пользователь")] IUser user)
        {
            if (MainData.configData.Groups == null)
                MainData.configData.Groups = new List<Models.ConfigModel.Group>();

            if (MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault() == null)
                MainData.configData.Groups.Add(new Models.ConfigModel.Group()
                {
                    Id = (uint)groupsEnum,
                    Members = new List<ulong>()
                    { }
                });

            if (!MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault().Members.Contains(user.Id))
            {
                await RespondAsync("Игрока нет в группировке", ephemeral: true);
                return;
            }

            MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault().Members.Remove(user.Id);
            if (MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault().Leader == user.Id)
                MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault().Leader = 0;
            await RespondAsync($"Игрок успешно удален из групппировки {GetValueExtension.GetDescription(groupsEnum)}", ephemeral: true);
        }

        [SlashCommand("setleader", "Установить лидера группировки")]
        public async Task SetLeader([Summary(name: "Группировка", description: "Название группировки")] GroupsEnum groupsEnum, [Summary(name: "Лидер", description: "Пользователь")] IUser user)
        {
            if (MainData.configData.Groups == null)
            {
                await RespondAsync("Ебать ты как этого добился", ephemeral: true);
                return;
            }

            if (MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault() == null)
            {
                await RespondAsync("Группировка пуста", ephemeral: true);
                return;
            }

            if (!MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault().Members.Contains(user.Id))
            {
                await RespondAsync("Игрок не принадлежит к группировке", ephemeral: true);
                return;
            }

            MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault().Leader = user.Id;
            await RespondAsync($"Игрок успешно установлен как лидер группировки {GetValueExtension.GetDescription(groupsEnum)}", ephemeral: true);
        }

        [SlashCommand("disbandgroup", "Расформировать группировку")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DisbandGroup([Summary(name: "Группировка", description: "Название группировки")] GroupsEnum groupsEnum)
        {
            if (MainData.configData.Groups == null)
            {
                await RespondAsync("Ебать ты как этого добился", ephemeral: true);
                return;
            }

            if (MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault() == null)
            {
                await RespondAsync("Группировка пуста", ephemeral: true);
                return;
            }

            MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault().Members = new List<ulong>();
            MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault().Leader = 0;

            await RespondAsync($"Группировка {GetValueExtension.GetDescription(groupsEnum)} расформирована", ephemeral: true);
        }

        [SlashCommand("groupmembers", "Список игроков группировки")]
        public async Task GetGroupMembers([Summary(name: "Группировка", description: "Название группировки")] GroupsEnum groupsEnum)
        {
            if (MainData.configData.Groups == null)
            {
                await RespondAsync("Ебать ты как этого добился", ephemeral: true);
                return;
            }

            if (MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault() == null)
            {
                await RespondAsync("Группировка пуста", ephemeral: true);
                return;
            }

            var group = MainData.configData.Groups.Where(x => x.Id == (uint)groupsEnum).FirstOrDefault();

            var members = string.Empty;
            for (int i = 1; i <= group.Members.Count; i++)
                members += i.ToString() + ". " + MentionUtils.MentionUser(group.Members[i - 1]) + "\n";
            var groupEmbed = new EmbedBuilder()
                .WithTitle($"Состав группировки - {GetValueExtension.GetDescription(groupsEnum)}")
                .AddField("Лидер группировки", MentionUtils.MentionUser(group.Leader))
                .AddField("Состав группировки", members)
                .WithColor(Color.Blue);

            await RespondAsync(embed: groupEmbed.Build());
        }

        [UserCommand("getgroup")]
        public async Task GetGroup(IUser user)
        {
            if (MainData.configData.Groups == null)
            {
                await RespondAsync("Ебать ты как этого добился", ephemeral: true);
                return;
            }

            foreach (var group in MainData.configData.Groups)
                foreach (var member in group.Members)
                {
                    if (member == user.Id)
                    {
                        await RespondAsync($"Игрок принадлежит к группировке {GetValueExtension.GetDescription((GroupsEnum)group.Id)}", ephemeral: true);
                        return;
                    }
                }

            await RespondAsync("Игрок не принадлежит ни к одной из группировок", ephemeral: true);
        }
    }
}
