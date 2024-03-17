using Newtonsoft.Json;

namespace NS2Bot.Models
{
    public class ConfigModel
    {
        public Categories Category { get; set; }
        public int HelperTicketsCount { get; set; }
        public Dictionary<ulong,ulong> MessageChannelTickerPair { get; set; }
        public ulong PDALogsChannelId { get; set; } 
        public ulong PublicPDAChannelId { get;set; }
        public List<Group> Groups { get; set; }
        public List<War> Wars { get; set; }
        public bool IsRadioEnabled { get; set; }
        public class Categories
        {
            public ulong NewHelperTicketsCategoryId { get; set; }
            public ulong HelperTicketsChannelId { get; set; }
            public ulong OldTicketsCategoryId { get; set; }
            public ulong RadioInitChannelId { get; set; }
            public List<ulong> ActiveRadios { get; set; }
        }
        public class Group
        {
            public uint Id {  get; set; }
            public ulong Leader { get; set; }
            public List<ulong> Members { get; set; }
        }
        public class War
        {
            public uint Agressor { get; set; }
            public uint Target { get; set; }
            public List<uint> AgressorAllies { get; set; }
            public List<uint> TargetAllies { get; set; }
            public uint AgreesorColor { get; set; }
            public uint TargetColor { get; set; }

            public void Update(War newdata)
            {
                Agressor = newdata.Agressor;
                Target = newdata.Target;
                AgressorAllies = newdata.AgressorAllies;
                TargetAllies = newdata.TargetAllies;
            }
        }
    }
}
