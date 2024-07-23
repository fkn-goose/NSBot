using Discord;
using Discord.Interactions;
using NS.Bot.Shared.Enums;

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

        public class CuratorTicketModal : IModal
        {
            [InputLabel("Подробно опишите суть обращения")]
            [ModalTextInput("reason", TextInputStyle.Paragraph, placeholder: "Выдача/восстановление доната и вещей, вопрос по правилам, разбор ситуации и т.д.", maxLength: 200)]
            public string Reason { get; set; }
            public string Title => "Создание обращения к курации";
            public TicketTypeEnum TicketTypeEnum = TicketTypeEnum.Curator;
        }

        public class AdminTicketModal : IModal
        {
            [InputLabel("Подробно опишите суть обращения")]
            [ModalTextInput("reason", TextInputStyle.Paragraph, placeholder: "Вопросы по поводу занятия ГП, жалобы на курацию, частные случаи и т.д.", maxLength: 200)]
            public string Reason { get; set; }
            public string Title => "Создание обращения к администрации";
            public TicketTypeEnum TicketTypeEnum = TicketTypeEnum.Admin;
        }

        public class BaseTicket
        {
            public BaseTicket(HelperTicketModal modal)
            {
                Title = modal.Title;
                Reason = modal.Reason;
                TicketType = modal.TicketTypeEnum;
            }

            public BaseTicket(CuratorTicketModal modal)
            {
                Title = modal.Title;
                Reason = modal.Reason;
                TicketType = modal.TicketTypeEnum;
            }

            public BaseTicket(AdminTicketModal modal)
            {
                Title = modal.Title;
                Reason = modal.Reason;
                TicketType = modal.TicketTypeEnum;
            }
            public string Title { get; set; }
            public string Reason { get; set; }
            public TicketTypeEnum TicketType { get; set; }
        }
    }
}
