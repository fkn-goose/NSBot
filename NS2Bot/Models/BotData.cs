namespace NS2Bot.Models
{
    public class BotData
    {
        public Channels Channels { get; set; }
        public Ticket Helper {  get; set; }
        public Ticket Curator { get; set; }
        public List<Group> Groups { get; set; }
        public List<War> Wars { get; set; }

        public class Group
        {
            public uint Id { get; set; }
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
