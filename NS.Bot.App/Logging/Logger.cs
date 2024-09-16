using Discord;

namespace NS.Bot.App.Logging
{
    public abstract class Logger : ILogger
    {
        public string _guid;
        public Logger()
        {
            _guid = Guid.NewGuid().ToString()[^4..];
        }
        public abstract Task LogAsync(LogMessage message);
    }
}
