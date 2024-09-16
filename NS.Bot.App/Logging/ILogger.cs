using Discord;

namespace NS.Bot.App.Logging
{
    public interface ILogger
    {
        public Task LogAsync(LogMessage message);
    }
}
