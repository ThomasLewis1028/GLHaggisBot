using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Color = Discord.Color;

namespace GLHaggisBot
{
    internal class HaggisBot
    {
        private readonly Mp2Bot _mp2Bot = new Mp2Bot();

        private static readonly RegularExpressions Regex = new RegularExpressions();

        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        // Properties file
        private static readonly JObject Prop =
            JObject.Parse(
                File.ReadAllText(@"properties.json"));

        // Get the token out of the properties folder
        private readonly string _token;

        // Set up emoji for Reactions
        private static readonly Emoji SearchGlass = new Emoji("🔍");
        private static readonly Emoji CheckMark = new Emoji("✅");

        // Discord config files
        private DiscordSocketClient _client;

        public HaggisBot(bool test)
        {
            _token =
                test ? (string) Prop.GetValue("tokenTest") : (string) Prop.GetValue("token");
        }

        public async Task MainAsync()
        {
            var config = new DiscordSocketConfig {MessageCacheSize = 100};
            // _client.Log += message => Console.Out.WriteLine();
            _client = new DiscordSocketClient(config);
            _client.MessageReceived += MessageReceived;
            // _client.ReactionAdded += ReactionAdded;

            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
            await _client.SetGameAsync("Requires Relic 7 Lobot, K-2SO, Chopper, C-3PO, and T3-M4 to unlock at 1*");

            _logger.Info("DiscordBot Connected");

            while (true)
            {
                await Task.Delay(43200000);
                await _mp2Bot.UpdateProbation(_client);
            }

            // ReSharper disable once FunctionNeverReturns
        }

        private async Task MessageReceived(SocketMessage sm)
        {
            if (sm.Author.IsBot)
                return;

            try
            {
                switch (sm.Content)
                {
                    case var _ when Regex.MemberActivity.IsMatch(sm.Content):
                        await SendReaction(sm, SearchGlass);
                        _logger.Info("Getting Member Activity: " + sm.Content);
                        _mp2Bot.GetActivity(sm);
                        await SendReaction(sm, CheckMark);
                        break;
                    case var _ when Regex.GuildActivity.IsMatch(sm.Content):
                        await SendReaction(sm, SearchGlass);
                        _logger.Info("Getting Guild Activity: " + sm.Content);
                        _mp2Bot.GetAllActivity((IUserMessage) sm);
                        await SendReaction(sm, CheckMark);
                        break;
                    case var _ when Regex.UpdateProbation.IsMatch(sm.Content):
                        await SendReaction(sm, SearchGlass);
                        _logger.Info("Updating Probation: " + sm.Content);
                        await _mp2Bot.UpdateProbation(_client, (IUserMessage) sm);
                        await SendReaction(sm, CheckMark);
                        break;
                    case var _ when Regex.AddRaider.IsMatch(sm.Content):
                        await SendReaction(sm, SearchGlass);
                        _logger.Info("Adding Raider Role: " + sm.Content);
                        await _mp2Bot.ChangeRaidRole(_client, (IUserMessage) sm);
                        await SendReaction(sm, CheckMark);
                        break;
                    case var _ when Regex.RaidTimes.IsMatch(sm.Content):
                        await SendReaction(sm, SearchGlass);
                        _logger.Info("Getting Raid Times: " + sm.Content);
                        await _mp2Bot.GetRaidTimes((IUserMessage) sm);
                        await SendReaction(sm, CheckMark);
                        break;
                    case var _ when Regex.Help.IsMatch(sm.Content):
                        _logger.Info("Sending help list: " + sm.Content);
                        await SendHelp((IUserMessage) sm);
                        break;
                    case var _ when Regex.MP2Requirements.IsMatch(sm.Content):
                        _logger.Info("Sending Requirements List: " + sm.Content);
                        await _mp2Bot.MP2Requirements((IUserMessage) sm);
                        await SendReaction(sm, CheckMark);
                        break;
                }
            }
            catch (Exception e)
            {
                await sm.Channel.SendMessageAsync(e.Message);
            }
        }

        private async Task SendHelp(IUserMessage ium)
        {
            var eb = new EmbedBuilder
            {
                Title = "Help",
                Description = "All commands are case insensitive and are preceded with ;\n" +
                              "<> indicates required parameter \n" +
                              "[] indicates optional parameter \n" +
                              "() indicates either or parameter",
                Color = Color.Gold
            };
            eb.AddField("Help", "help");

            eb.AddField("Guild Activity",
                "Get Member Activity - (ma | memberActivity) [(@<user> | <ally-code>)]\n" +
                "Get Guild Activity - (ga | guildActivity)\n");
            eb.AddField("Raids",
                "Add Raider Role - raidRole\n" +
                "Get Raid Times - mp2Raid");
            eb.AddField("Requirements",
                "mp2Req");

            await ium.ReplyAsync(null, false, eb.Build());
        }

        private static async Task SendReaction(SocketMessage sm, Emoji emoji)
        {
            await sm.AddReactionAsync(emoji);
        }
    }
}