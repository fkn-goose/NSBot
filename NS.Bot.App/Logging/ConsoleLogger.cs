using Discord;

namespace NS.Bot.App.Logging
{
    public class ConsoleLogger : Logger
    {
        public override async Task LogAsync(LogMessage message)
        {
            await Task.Run(() => Console.WriteLine($"guid:{_guid} : " + message));
        }
    }
}
