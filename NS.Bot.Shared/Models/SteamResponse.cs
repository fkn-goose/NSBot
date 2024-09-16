using System.Collections.Generic;

namespace NS.Bot.Shared.Models
{
    public class SteamResponse
    {
        public Response response { get; set; }

        public class Response
        {
            public List<Players> players { get; set; }
        }
        
        public class Players
        {
            public string steamid { get; set; }
            public int communityvisibilitystate { get; set; }
            public string personaname { get; set; }
            public string profileurl { get; set; }
            public string avatar { get; set; }
            public string avatarmedium { get; set; }
            public string avatarfull { get; set; }
            public string avatarhash { get; set; }
            public int personastate { get; set; }
        }
    }
}
