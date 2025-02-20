﻿using NS.Bot.Shared.Entities.Group;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Enums;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface IGroupService : IBaseService<GroupEntity>
    {
        /// <summary>
        /// Получить группировку по названию
        /// </summary>
        /// <param name="groupEnum">Название группировки</param>
        /// <param name="currentGuild">Сервер группировки</param>
        /// <returns>Группировка</returns>
        Task<GroupEntity> GetGroupByEnum(GroupEnum groupEnum, GuildEntity currentGuild);
    }
}
