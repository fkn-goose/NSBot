using Discord;
using Discord.Interactions;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Models;
using System.Text.Json;

namespace NS.Bot.App.Commands
{
    [Group("steamid", "взаимодействие со steamID пользователей")]
    public class SteamIDModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IMemberService _memberService;
        private readonly IGuildService _guildService;
        private readonly IGuildMemberService _guildMemberService;
        private readonly AppsettingsModel _appsettings;
        public SteamIDModule(IMemberService memberService, IGuildService guildService, IGuildMemberService guildMemberService, AppsettingsModel appsettings)
        {
            _memberService = memberService;
            _guildService = guildService;
            _guildMemberService = guildMemberService;
            _appsettings = appsettings;
        }

        [SlashCommand("получить", "Получить steamid игрока")]
        public async Task GetMemberSteamId([Summary("Пользователь")] IGuildUser user)
        {
            await DeferAsync(ephemeral: true);

            var currentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            if (currentGuild == null)
            {
                await FollowupAsync("Не найден сервер", ephemeral: true);
                return;
            }

            var guildMember = await _guildMemberService.GetByDiscordIdAsync(user.Id, currentGuild.GuildId);
            if (guildMember == null)
            {
                var member = await _memberService.GetByDiscordIdAsync(user.Id);
                member ??= await _memberService.CreateMemberAsync(user.Id, currentGuild);
                guildMember = await _guildMemberService.GetByMemberAsync(member, currentGuild);
            }

            var response = guildMember.Member.SteamId?.ToString() ?? "SteamID не указан";
            await FollowupAsync(response, ephemeral: true);
        }

        [SlashCommand("установить", "Установить steamid игрока")]
        public async Task SetMemberStemID([Summary("Пользователь")] IGuildUser user, [Summary("SteamID")][MinLength(17)][MaxLength(17)] string steamID)
        {
            if(!steamID.All(char.IsDigit))
            {
                await RespondAsync("SteamID состоит только из цифр", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            var currentGuild = await _guildService.GetByDiscordId(Context.Guild.Id);
            if (currentGuild == null)
            {
                await FollowupAsync("Не найден сервер", ephemeral: true);
                return;
            }

            if(!await ValidateSteamID(steamID))
            {
                await FollowupAsync("SteamID некорректен", ephemeral: true);
                return;
            }

            var guildMember = await _guildMemberService.GetByDiscordIdAsync(user.Id, currentGuild.GuildId);
            if (guildMember == null)
            {
                var member = await _memberService.GetByDiscordIdAsync(user.Id);
                member ??= await _memberService.CreateMemberAsync(user.Id, currentGuild);
                guildMember = await _guildMemberService.GetByMemberAsync(member, currentGuild);
            }

            guildMember.Member.SteamId = steamID;
            await _memberService.UpdateAsync(guildMember.Member);
            await FollowupAsync("SteamID установлен", ephemeral: true);
        }

        private async Task<bool> ValidateSteamID(string steamID)
        {
            var steamApiKey = _appsettings.SteamAPIKey;
            if (steamApiKey == null)
                return false;

            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(string.Format("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids=/{1}", steamApiKey, steamID.ToString()));
            if (response == null)
                return false;

            SteamResponse steamResponse = JsonSerializer.Deserialize<SteamResponse>(response);
            if (steamResponse == null)
                return false;

            if (steamResponse.response == null)
                return false;

            if (steamResponse.response.players == null || !steamResponse.response.players.Any())
                return false;

            if(steamResponse.response.players.First().steamid != steamID.ToString())
                return false;

            return true;
        }
    }
}
