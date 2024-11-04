using Discord;
using Discord.Interactions;
using NS.Bot.Shared.Enums;
using NS.Bot.Shared.Extensions;
using System;

namespace NS2Bot.Extensions
{
    public class ModalExtensions
    {
        public class HelperTicketModal : IModal
        {
            [InputLabel("Подробно опишите суть обращения")]
            [ModalTextInput("reason", TextInputStyle.Paragraph, placeholder: "Помочь с подключением к серверу, проведение собеседования, квенте т.д.", maxLength: 200)]
            public string Reason { get; set; }
            public string Title => "Создание обращения к хелперу";
            public TicketTypeEnum TicketTypeEnum = TicketTypeEnum.Helper;
        }

        public class NickTicketModal : IModal
        {
            [InputLabel("Новый позывной")]
            [ModalTextInput("newNick", TextInputStyle.Short, maxLength: 30)]
            public string Nick { get; set; }
            [InputLabel("Причина смена позывного")]
            [ModalTextInput("reason", TextInputStyle.Paragraph, maxLength: 200)]
            public string Reason { get; set; }
            public string Title => "Смена позывного";
            public TicketTypeEnum TicketTypeEnum = TicketTypeEnum.ChangeNick;
        }

        public class CuratorTicketModal : IModal
        {
            [InputLabel("Подробно опишите суть обращения")]
            [ModalTextInput("reason", TextInputStyle.Paragraph, placeholder: "Вопрос по правилам, разбор ситуации и т.д.", maxLength: 200)]
            public string Reason { get; set; }
            public string Title => "Создание обращения к курации";
            public TicketTypeEnum TicketTypeEnum = TicketTypeEnum.Curator;
        }
        public class ItemsTicketModal : IModal
        {
            [InputLabel("Ваш SteamID. Указывайте SteamID только от вашего аккаунта.")]
            [ModalTextInput("steamID", TextInputStyle.Short, placeholder: "70000000000000000", minLength: 17, maxLength: 17)]
            public string SteamId { get; set; }

            [InputLabel("Список вещей")]
            [ModalTextInput("whatLost", TextInputStyle.Paragraph, maxLength: 500)]
            public string WhatLost { get; set; }

            [InputLabel("Как было утеряно")]
            [ModalTextInput("howLost", TextInputStyle.Paragraph, placeholder: "В случае первого получения бонуса, оставить поле пустым", maxLength: 200)]
            public string Reason { get; set; }

            [InputLabel("Когда было утеряно")]
            [ModalTextInput("whenLost", TextInputStyle.Short, placeholder: "В случае первого получения бонуса, оставить поле пустым", maxLength: 20)]
            public string When { get; set; }
            public string Title => "Получение/восстановление вещей";
            public TicketTypeEnum TicketTypeEnum = TicketTypeEnum.ItemsRestor;
        }

        public class BonusTicketModal : ItemsTicketModal
        {
            public new string Title => "Получение/восстановление бонусов";
            public new TicketTypeEnum TicketTypeEnum = TicketTypeEnum.BonusesRestor;
        }

        public class ComplaintModal : IModal
        {
            [InputLabel("Опишите ситуацию")]
            [ModalTextInput("situation", TextInputStyle.Paragraph, placeholder: "Максимально подробно опишите произошедшую ситуацию", maxLength: 500)]
            public string Reason { get; set; }

            [InputLabel("Когда произошла ситуация")]
            [ModalTextInput("whenSituation", TextInputStyle.Short, placeholder: "Место, время и дата", maxLength: 20)]
            public string When { get; set; }

            [InputLabel("Discord Tag нарушителя")]
            [ModalTextInput("discordTag", TextInputStyle.Short, placeholder: "Если неизвестен - оставьте пустым", minLength: 17, maxLength: 17)]
            [RequiredInput(false)]
            public string DiscordTag { get; set; }
            public string Title => "Создание жалобы";
            public TicketTypeEnum TicketTypeEnum = TicketTypeEnum.Complaint;
        }

        public class AdminTicketModal : IModal
        {
            [InputLabel("Подробно опишите суть обращения")]
            [ModalTextInput("reason", TextInputStyle.Paragraph, placeholder: "Вопросы по поводу занятия ГП и частные случаи", maxLength: 200)]
            public string Reason { get; set; }
            public string Title => "Создание обращения к администрации";
            public TicketTypeEnum TicketTypeEnum = TicketTypeEnum.Admin;
        }
    }
}
