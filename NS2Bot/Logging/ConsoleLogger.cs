using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NS2Bot.Logging
{
    public class ConsoleLogger : Logger
    {
        public override async Task LogAsync(LogMessage message)
        {
            await Task.Run(() => Console.WriteLine($"guid:{_guid} : " + message));
        }
    }
}
