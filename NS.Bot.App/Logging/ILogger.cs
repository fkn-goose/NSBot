using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NS.Bot.App.Logging
{
    public interface ILogger
    {
        public Task LogAsync(LogMessage message);
    }
}
