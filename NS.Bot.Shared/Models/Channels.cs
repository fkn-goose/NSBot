using System.Collections.Generic;

namespace NS.Bot.Shared.Models
{
    public class Channels
    {
        public PDA PDA { get; set; }
        public Radio Radio { get; set; }
    }

    public class PDA
    {
        public ulong PDALogsChannelId { get; set; }
        public ulong PublicPDAChannelId { get; set; }
    }

    public class Radio
    {
        public ulong RadioInitChannelId { get; set; }
        public List<ulong> ActiveRadios { get; set; }
        public bool IsRadioEnabled { get; set; }
    }
}
