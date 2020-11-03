using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using Color = Discord.Color;

namespace GLHaggisBot
{
    public class Mp2Bot
    {
        static readonly string[] Scopes = {SheetsService.Scope.SpreadsheetsReadonly};
        private readonly GoogleCredential _credential;
        private readonly String _spreadsheetId;
        // private readonly ulong _mutinyRole;
        private readonly ulong _mp2Role;
        // private readonly ulong _sithApprentices;
        private readonly ulong _knightsOfRen;
        // private readonly ulong _td;
        private readonly ulong _mutinyGuild;
        private readonly ulong _mp2General;
        private readonly ulong _mp2Probation;
        private readonly ulong _mp2Mediation;
        private readonly ulong _mp2Raider;
        private readonly SheetsService _service;
        private static readonly RegularExpressions Regex = new RegularExpressions();
        private readonly String _activityList = "ACTIVITY LIST!A3:M52";
        private readonly String _members = "MEMBERS!A2:F51";


        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        // Properties file
        private static readonly JObject Prop =
            JObject.Parse(
                File.ReadAllText(@"properties.json"));

        public Mp2Bot()
        {
            _spreadsheetId = (string) Prop.GetValue("mp2Sheet");
            _mp2Role = (ulong) Prop.GetValue("mutinyPartDeux");
            // _mutinyRole = (ulong) Prop.GetValue("mutiny");
            _mutinyGuild = (ulong) Prop.GetValue("mutinyGuild");
            // _sithApprentices = (ulong) Prop.GetValue("sithApprentices");
            _knightsOfRen = (ulong) Prop.GetValue("knightsOfRen");
            // _td = (ulong) Prop.GetValue("td");
            _mp2General = (ulong) Prop.GetValue("mp2General");
            _mp2Probation = (ulong) Prop.GetValue("mp2Probation");
            _mp2Mediation = (ulong) Prop.GetValue("mp2Mediation");
            _mp2Raider = (ulong) Prop.GetValue("mp2Raider");

            var applicationName = (string) Prop.GetValue("sheetName");

            try
            {
                using var stream = new FileStream(@"credentials.json", FileMode.Open, FileAccess.Read);
                _credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);

                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("FAILED TO GET CREDENTIALS\n" + e.Message);
            }

            try
            {
                // Create Google Sheets API service.
                _service = new SheetsService(new BaseClientService.Initializer
                {
                    ApplicationName = applicationName,
                    HttpClientInitializer = _credential
                    // ApiKey = _apiKey
                });

                _logger.Info("API Successfully Retrieved");
            }
            catch (Exception e)
            {
                _logger.Info("FAILED TO GET API: \n" + e.Message);
            }
        }

        public async void GetActivity(SocketMessage sm)
        {
            // Define request parameters.
            String userName = null;
            String allyCode = null;

            if (sm.MentionedUsers.Count > 0)
                userName = sm.MentionedUsers.First().Username + "#" + sm.MentionedUsers.First().Discriminator;
            else if (Regex.AllyCode.IsMatch(sm.Content))
                allyCode = sm.Content.Split(' ')[1];
            else
                userName = sm.Author.Username + "#" + sm.Author.Discriminator;

            SpreadsheetsResource.ValuesResource.GetRequest memberRequest =
                _service.Spreadsheets.Values.Get(_spreadsheetId, _members);

            ValueRange memberResponse = await memberRequest.ExecuteAsync();
            IList<IList<Object>> memberValues = memberResponse.Values;

            IList<IList<Object>> inGameName;
            if (!String.IsNullOrEmpty(userName))
            {
                if (memberValues != null && memberValues.Count > 0)
                    inGameName = memberValues.Where(m => m[1].ToString() == userName).ToList();
                else
                {
                    await sm.Channel.SendMessageAsync(userName + " not found");
                    return;
                }
            }
            else if (!String.IsNullOrEmpty(allyCode))
            {
                if (!allyCode.Contains('-'))
                    allyCode = allyCode.Insert(3, "-").Insert(7, "-");

                if (memberValues != null && memberValues.Count > 0)
                    inGameName = memberValues.Where(m => m[2].ToString() == allyCode).ToList();
                else
                {
                    await sm.Channel.SendMessageAsync(allyCode + " not found");
                    return;
                }
            }
            else
            {
                await sm.Channel.SendMessageAsync("An error occurred");
                return;
            }


            SpreadsheetsResource.ValuesResource.GetRequest activityRequest =
                _service.Spreadsheets.Values.Get(_spreadsheetId, _activityList);

            ValueRange activityResponse = await activityRequest.ExecuteAsync();
            IList<IList<Object>> activityValues = activityResponse.Values;
            if (activityValues != null && activityValues.Count > 0)
            {
                foreach (var ign in inGameName)
                {
                    var member = activityValues.First(m => m[0].ToString() == ign[0].ToString());
                    EmbedBuilder eb = new EmbedBuilder
                    {
                        Title = ign[0] + " - " + memberValues.First(m => m[0] == ign[0])[2],
                        Description = "Note that each section is capped at 60%, 5%+15% and 20% respectively"
                    };
                    var score = Double.Parse(member[12].ToString()?.Split('%')[0]!);
                    eb.Color = Math.Abs(score - 100.00) < 0.01
                        ? new Color(47, 62, 80)
                        : score < 100.00 && score >= 85.00
                            ? new Color(34, 73, 54)
                            : score < 85.00 && score > 50.00
                                ? new Color(100, 0, 0)
                                : new Color(40, 40, 40);
                    eb.AddField("Galactic Power", member[2]);
                    eb.AddField("Tickets - " + member[4], member[3] + " Missed");
                    eb.AddField("Territory War - " + member[6] + " + " + member[9],
                        member[5] + " Joined | " + member[7] + " Defense Banners | " + member[8] + " Offense Banners");
                    eb.AddField("Territory Battle - " + member[11], member[10] + " Participation");
                    eb.AddField("Overall Score", member[12] +
                                                 (score < 85.00
                                                     ? " - ON PROBATION"
                                                     : ""));

                    await sm.Channel.SendMessageAsync(null, false, eb.Build());
                }
            }
            else
            {
                await sm.Channel.SendMessageAsync(userName == null ? allyCode : userName + " not found");
            }
        }

        public async void GetAllActivity(SocketMessage sm)
        {
            SpreadsheetsResource.ValuesResource.GetRequest activityRequest =
                _service.Spreadsheets.Values.Get(_spreadsheetId, _activityList);

            // Prints the names and majors of students in a sample spreadsheet:
            ValueRange activityResponse = await activityRequest.ExecuteAsync();
            IList<IList<Object>> activityValues = activityResponse.Values;

            if (activityValues != null && activityValues.Count > 0)
            {
                EmbedBuilder eb = new EmbedBuilder
                {
                    Title = "Guild Participation",
                    Color = new Color(255, 255, 0)
                };
                StringBuilder sb = new StringBuilder();
                sb.Append("```");

                foreach (var member in activityValues.Take(10))
                {
                    sb.Append($"{member[0],-20} {"|",0} {member[12],15}\n");
                }

                sb.Append("```");
                eb.AddField("================ 1 - 10 ===============", sb);

                sb = new StringBuilder();
                sb.Append("```");
                foreach (var member in activityValues.Skip(10).Take(10))
                {
                    sb.Append($"{member[0],-20} {"|",0} {member[12],15}\n");
                }

                sb.Append("```");
                eb.AddField("================ 11 - 20 ================", sb);

                sb = new StringBuilder();
                sb.Append("```");
                foreach (var member in activityValues.Skip(20).Take(10))
                {
                    sb.Append($"{member[0],-20} {"|",0} {member[12],15}\n");
                }

                sb.Append("```");
                eb.AddField("=============== 21 - 30 ===============", sb);

                sb = new StringBuilder();
                sb.Append("```");
                foreach (var member in activityValues.Skip(30).Take(10))
                {
                    sb.Append($"{member[0],-20} {"|",0} {member[12],15}\n");
                }

                sb.Append("```");
                eb.AddField("=============== 31 - 40 ===============", sb);

                sb = new StringBuilder();
                sb.Append("```");
                foreach (var member in activityValues.Skip(40).Take(10))
                {
                    sb.Append($"{member[0],-20} {"|",0} {member[12],15}\n");
                }

                sb.Append("```");
                eb.AddField("=============== 41 - 50 ===============", sb);


                await sm.Channel.SendMessageAsync(null, false, eb.Build());
            }
            else
            {
                await sm.Channel.SendMessageAsync("No Data Found");
            }
        }

        public async Task ChangeRaidRole(DiscordSocketClient dsc, SocketMessage sm)
        {
            var guild = dsc.GetGuild(_mutinyGuild);
            await guild.DownloadUsersAsync();

            if (sm.Author is IGuildUser user && user.RoleIds.Contains(_mp2Role))
            {
                if (user.RoleIds.Contains(_mp2Raider))
                {
                    await user.RemoveRoleAsync(guild.GetRole(_mp2Raider));
                    await sm.Channel.SendMessageAsync($"<@!{sm.Author.Id}> You no longer have the MP2-Raider role");
                }
                else
                {
                    await user.AddRoleAsync(guild.GetRole(_mp2Raider));
                    await sm.Channel.SendMessageAsync($"<@!{sm.Author.Id}> You now have the MP2-Raider role");
                }
            }
            else
                await sm.Channel.SendMessageAsync("You are not in MP2");
        }

        public async Task UpdateProbation(DiscordSocketClient dsc, SocketMessage sm)
        {
            if (sm.Author is IGuildUser user && user.RoleIds.Contains(_knightsOfRen))
            {
                await UpdateProbation(dsc);
                await sm.Channel.SendMessageAsync("Updated Probation");
            }
            else
                await sm.Channel.SendMessageAsync("Only the Knights of Ren can use this command");
        }

        public async Task UpdateProbation(DiscordSocketClient dsc)
        {
            SpreadsheetsResource.ValuesResource.GetRequest memberRequest =
                _service.Spreadsheets.Values.Get(_spreadsheetId, _members);

            ValueRange memberResponse = await memberRequest.ExecuteAsync();
            IList<IList<Object>> memberValues = memberResponse.Values;

            SpreadsheetsResource.ValuesResource.GetRequest activityRequest =
                _service.Spreadsheets.Values.Get(_spreadsheetId, _activityList);

            ValueRange activityResponse = await activityRequest.ExecuteAsync();
            IList<IList<Object>> activityValues = activityResponse.Values
                .Where(m => Double.Parse(m[12].ToString()?.Split('%')[0]!) < 85.00).ToList();


            var guild = dsc.GetGuild(_mutinyGuild);
            var members = guild.Roles.First(r => r.Id == _mp2Role).Members as IEnumerable<IGuildUser>;

            foreach (var member in members)
            {
                var username = member.Username + "#" + member.Discriminator;
                var ign = memberValues.Where(m => (string) m[1] == username).ToList();

                if (activityValues.Any(m => ign.Any(a => a.Contains(m[0]))))
                {
                    if (member.RoleIds.Contains(_mp2Probation)) continue;

                    await member.AddRoleAsync(guild.GetRole(_mp2Probation));
                    var mediationChannel = (ISocketMessageChannel) dsc.GetChannel(_mp2Mediation);
                    await mediationChannel.SendMessageAsync(
                        $"<@{member.Id}> You are now on probation. Use ;ma to view your current activity.");
                }
                else
                {
                    if (!member.RoleIds.Contains(_mp2Probation)) continue;

                    await member.RemoveRoleAsync(guild.GetRole(_mp2Probation));
                    var mp2GeneralChannel = (ISocketMessageChannel) dsc.GetChannel(_mp2General);
                    await mp2GeneralChannel.SendMessageAsync(
                        $"<@{member.Id}> You are no longer on probation!");
                }
            }
        }

        public async Task GetRaidTimes(SocketMessage sm)
        {
            EmbedBuilder eb = new EmbedBuilder
            {
                Color = new Color(139, 69, 19),
                Title = "MP2 Raid Times",
                Description = "All raid times are in UTC"
            };
            eb.AddField("Times", "HSTR: 20:00, 23:00, 02:00 - Rotating\n" +
                            "HPit: 20:00\n" +
                            "HAAT: 20:00");

            await sm.Channel.SendMessageAsync($"<@!{sm.Author.Id}>", false, eb.Build());
        }
    }
}