using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Enums;
using NS.Bot.Shared.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    public class LogToFileService : ILogToFileService
    {
        private Task BaseLog(LogType logType, string message)
        {
            var fileName = "Log_" + DateTime.Now.ToString("dd.MM.yyyy");
            using (StreamWriter str = File.AppendText($"Logs\\{fileName}.log"))
            {
                str.WriteLine(string.Format("[{0}] [{1}] | {2}", logType.GetDescription(), DateTime.Now.ToString("HH:mm:ss dd.MM.yyyy"), message));
            }

            return Task.CompletedTask;
        }

        public async Task Error(string message)
        {
            await BaseLog(LogType.Error, message);
        }

        public async Task Info(string message)
        {
            await BaseLog(LogType.Info, message);
        }
    }
}
