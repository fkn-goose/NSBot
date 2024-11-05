using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NS.Bot.Shared.Models
{
    public class AppsettingsModel
    {
        public Logging Logging { get; set; }
        public string AllowedHosts { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
        public DiscordConnection DiscordConnection { get; set; }
        public string SteamAPIKey { get; set; }
        public List<TicketSettingsModel> TicketSettings { get; set; }
        public List<GuildData> GuildDatas { get; set; }
    }
    public class Logging
    {
        public LogLevel LogLevel { get; set; }

    }
    public class LogLevel
    {
        public string Default { get; set; }

        [JsonPropertyName("Microsoft.AspNetCore")]
        [ConfigurationKeyName("Microsoft.AspNetCore")]
        public string Microsoft { get; set; }
    }
    public class ConnectionStrings
    {
        public string NSDataBase { get; set; }
    }
    public class DiscordConnection
    {
        public string NSBotToken { get; set; }
    }

}
