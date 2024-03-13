using Newtonsoft.Json;

namespace NS2Bot.Models
{
    public class ConfigModel
    {
        public Categories Category { get; set; }
        public int HelperTicketsCount { get; set; }
        public Dictionary<ulong,ulong> MessageChannelTickerPair { get; set; }
        public ulong PDALogsChannelId { get; set; } 
        public List<Group> Groups { get; set; }
        public class Categories
        {
            public ulong NewHelperTicketsCategoryId { get; set; }
            public ulong HelperTicketsChannelId { get; set; }
            public ulong OldTicketsCategoryId { get; set; }
        }
        public class Group
        {
            public string Name { get; set; }
            public uint Id {  get; set; }
            public List<ulong> Members { get; set; }
        }
    }
}
