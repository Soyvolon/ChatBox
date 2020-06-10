using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Services
{
    public class LoggingService
    {
        private readonly DiscordShardedClient _client;

        public LogSeverity ServertiyFilter { get; set; }

        public LoggingService(DiscordShardedClient client = null)
        {
            _client = client;
            ServertiyFilter = LogSeverity.Debug;
        }

        public void Initalize()
        {
            if (_client is null) return; // Quit early is the client is null (we only need one to handle client connection).

            _client.ShardConnected += ShardConnected;
            _client.ShardReady += ShardReady;
            _client.ShardDisconnected += ShardDisconnected;
        }

        /// <summary>
        /// Logs a message in the console output
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public Task LogMessage(LogSeverity severity, string source, string message, Exception ex = null)
        {
            return LogMessage(new LogMessage(severity, source, message, ex));
        }

        /// <summary>
        /// Logs a message in the console output
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public Task LogMessage(LogMessage log) //TODO: Expand this method to color code error serverity, etc.
        {
            if (log.Severity <= ServertiyFilter) // filters out unwanted messages
            {
                Console.WriteLine($"[{DateTime.Now}] [{log.Severity}] ({log.Source}): {log.Message}");
                if (log.Exception != null)
                { // prints exception data if it exsists
                    Console.WriteLine(log.Exception);
                }
            }
            return Task.CompletedTask;
        }

        private Task ShardConnected(DiscordSocketClient c)
        {
            LogMessage(LogSeverity.Info, "LSS", $"Shard {c.ShardId} Connected");

            return Task.CompletedTask;
        }

        private Task ShardReady(DiscordSocketClient c)
        {
            LogMessage(LogSeverity.Info, "LSS", $"Shard {c.ShardId} Ready");

            return Task.CompletedTask;
        }

        private Task ShardDisconnected(Exception ex, DiscordSocketClient c)
        {
            LogMessage(LogSeverity.Error, "LSS", $"Shard {c.ShardId} Lost Connection. Reconnecting . . .", ex);

            return Task.CompletedTask;
        }
    }
}
