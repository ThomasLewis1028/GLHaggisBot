using System;
using System.Threading.Tasks;

namespace GLHaggisBot
{
    static class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static async Task Main(string[] args)
        {
            try
            {
                var bot = new HaggisBot(args.Length > 0 && args[0] == "-test");
                await bot.MainAsync();


                while (true)
                {
                }
            }
            catch (Exception e)
            {
                Logger.Info(e);
            }
        }
    }
}