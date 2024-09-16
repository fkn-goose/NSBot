using NS.Bot.BuisnessLogic.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    public class LogToFileService : ILogToFileService
    {
        private async Task BaseLog(string logType, string message)
        {
            var fileName = "Log_" + DateTime.Now.ToShortDateString();
            var currentLogFile = File.Open($"Logs\\{fileName}.log", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            byte[] buffer = new byte[currentLogFile.Length];
            var log = currentLogFile.ReadAsync(buffer, 0, buffer.Length);

            string content = Encoding.UTF8.GetString(buffer);
            content += string.Format("[{0}] [{1}] | {2}\n", logType, DateTime.Now, message);

            buffer = Encoding.UTF8.GetBytes(content.ToString());

            await currentLogFile.WriteAsync(buffer, 0, buffer.Length);
            currentLogFile.Close();

            //Какая-то хуйня с переносом строк
        }

        public async Task Error(string message)
        {
            await BaseLog("Error", message);
        }

        public async Task Info(string message)
        {
            await BaseLog("Info", message);
        }
    }
}
