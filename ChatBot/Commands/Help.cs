using Discord;
using Discord.Commands;
using ChatBot.Properties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Commands
{
    public class Help : ModuleBase<ShardedCommandContext>
    {
        public Help()
        {

        }

        /// <summary>
        /// Help command. Fires if there is a command specefied after the word help.
        /// </summary>
        /// <param name="command">Command to find help for.</param>
        /// <returns>Completed Command</returns>
        [Command("help")]
        public async Task HelpAsync(string command)
        {
            string prefix = Resources.Prefix;

            EmbedBuilder b = new EmbedBuilder()
            {
                Color = Color.LighterGrey,
                Title = $"{prefix}{command}",
                Timestamp = DateTime.Now
            };

            switch (command.ToLower())
            {
                // Use this section to create command specific help responses.
                // Example:
                /*
                case "clockin":
                    b.WithDescription("Starts keeping track of your game time.");
                    b.AddField("Example:", $"{prefix}clockin");
                    break;
                */

                default:
                    await HelpAsync();
                    return;
            }

            await ReplyAsync(embed: b.Build());
        }

        /// <summary>
        /// Defualt help response. Fires if no text follows the help command.
        /// </summary>
        /// <returns>Completed Command</returns>
        [Command("help")]
        public async Task HelpAsync()
        {
            string prefix = Resources.Prefix;

            EmbedBuilder b = new EmbedBuilder()
            {
                Color = Color.LighterGrey,
                Title = "Default Command Helper",
                Description = $"Specify a command below with `{prefix}help [command]`"
            };

            b.AddField("Help", $"This is the default help menu. Help users navigate the rest of the help command here!");

            await ReplyAsync(embed: b.Build());
        }
    }
}
