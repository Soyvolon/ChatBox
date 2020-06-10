using System;
using System.IO;
using System.Threading.Tasks;

namespace ChatBot
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists("config.json"))
            {
                // Use this section to check for a config file or build a default config if one does not exist.
                //File.WriteAllText("config.json", "");
            }

            var bot = new BotMain();

            bot.Start();

            Task.Delay(-1).GetAwaiter().GetResult();
        }
    }
}
