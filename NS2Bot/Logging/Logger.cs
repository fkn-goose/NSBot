using Discord;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NS2Bot.Logging
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
