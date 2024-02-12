using Discord;
using Discord.WebSocket;

namespace NS2Bot
{
    public class Program
    {
        private static DiscordSocketClient _client;
        public async static void Main(string[] args)
        {
            var token = File.ReadAllText("../../../token.txt");

            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}
