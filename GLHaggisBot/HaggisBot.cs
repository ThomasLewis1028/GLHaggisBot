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
        // private readonly long _haggisId;
        // private readonly string _mp2Sheet;

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
                        _logger.Info("Getting Member Activity: " + sm.Content);
                        _mp2Bot.GetActivity(sm);
                        break;
                    case var _ when Regex.GuildActivity.IsMatch(sm.Content):
                        _logger.Info("Getting Guild Activity: " + sm.Content);
                        _mp2Bot.GetAllActivity(sm);
                        break;
                    case var _ when Regex.UpdateProbation.IsMatch(sm.Content):
                        _logger.Info("Updating Probation: " + sm.Content);
                        await _mp2Bot.UpdateProbation(_client);
                        break;
                    case var _ when Regex.Help.IsMatch(sm.Content):
                        _logger.Info("Sending help list: " + sm.Content);
                        await SendHelp(sm);
                        break;
                }
            }
            catch (Exception e)
            {
                await sm.Channel.SendMessageAsync(e.Message);
            }
        }

        private async Task SendHelp(SocketMessage sm)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.Title = "Help";
            eb.Description = "All commands are case insensitive and are preceded with ;\n" +
                             "<> indicates required parameter \n" +
                             "[] indicates optional parameter \n" +
                             "() indicates either or parameter";
            eb.Color = Color.Gold;
            eb.AddField("Help", "help");

            eb.AddField("Guild Activity",
                "Get Member Activity - (ma | memberActivity) [(@<user> | <ally-code>)]\n" +
                "Get Guild Activity - (ga | guildActivity)\n");


            _logger.Info("Sending help list: " + sm.Content);
            await sm.Channel.SendMessageAsync(null, false, eb.Build());
        }
    }
}