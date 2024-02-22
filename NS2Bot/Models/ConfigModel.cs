using Newtonsoft.Json;

namespace NS2Bot.Models
{
    public class ConfigModel
    {
        [JsonProperty]
        public Categories Category { get; set; }
        [JsonProperty]
        public int HelperTicketsCount { get; set; }
        public Dictionary<ulong,ulong> MessageChannelTickerPair { get; set; }

        public class Categories
        {
            [JsonProperty]
            public ulong NewHelperTicketsCategoryId { get; set; }
            [JsonProperty]
            public ulong HelperTicketsChannelId { get; set; }
            [JsonProperty]
            public ulong OldTicketsCategoryId { get; set; }
        }
    }
}
