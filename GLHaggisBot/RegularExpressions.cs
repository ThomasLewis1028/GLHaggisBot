using System.Text.RegularExpressions;

namespace GLHaggisBot
{
    public class RegularExpressions
    {
       public readonly Regex Help =
            new Regex("^!(help)", RegexOptions.IgnoreCase);
       
       public readonly Regex MemberActivity = new Regex("^!ma(| <@!(\\d+)>| \\d{9}| \\d{3}-\\d{3}-\\d{3})$", RegexOptions.IgnoreCase);
       public readonly Regex AllyCode = new Regex("(\\d{9}|\\d{3}-\\d{3}-\\d{3})");
       
       public readonly Regex UpdateProbation = new Regex("^!(updateProbation|probationUpdate|up)$");

        // public readonly Regex TempConv = new Regex("^!temp -?\\d+(.\\d+|)(c|f)$", RegexOptions.IgnoreCase);
        
        // public readonly Regex Subreddit = new Regex("(^| |^/| /)r/[^/ ]+", RegexOptions.IgnoreCase);
        // public readonly Regex Reddit = new Regex("(com)", RegexOptions.IgnoreCase);
    }
}