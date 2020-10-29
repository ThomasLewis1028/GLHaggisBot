using System.Text.RegularExpressions;

namespace GLHaggisBot
{
    public class RegularExpressions
    {
       public readonly Regex Help =
            new Regex("^;(help)", RegexOptions.IgnoreCase);
       
       public readonly Regex MemberActivity = new Regex("^;(memberActvity|ma)(| <@(!|)(\\d+)>| \\d{9}| \\d{3}-\\d{3}-\\d{3})$", RegexOptions.IgnoreCase);
       public readonly Regex AllyCode = new Regex("(\\d{9}|\\d{3}-\\d{3}-\\d{3})");
       
       public readonly Regex GuildActivity = new Regex("^;(guildActivity|ga)$");
       
       public readonly Regex UpdateProbation = new Regex("^;(updateProbation|probationUpdate|up)$");
       
       public readonly Regex AddRaider = new Regex("^;raidRole(| <@(!|)(\\d+)>| \\d{9}| \\d{3}-\\d{3}-\\d{3})$", RegexOptions.IgnoreCase);

       public readonly Regex RaidTimes = new Regex("^;(mp2Raid)$", RegexOptions.IgnoreCase);

       // public readonly Regex TempConv = new Regex("^!temp -?\\d+(.\\d+|)(c|f)$", RegexOptions.IgnoreCase);

       // public readonly Regex Subreddit = new Regex("(^| |^/| /)r/[^/ ]+", RegexOptions.IgnoreCase);
       // public readonly Regex Reddit = new Regex("(com)", RegexOptions.IgnoreCase);
    }
}