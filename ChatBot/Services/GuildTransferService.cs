using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace ChatBot.Services
{
    public class GuildTransferService
    {
        private readonly LoggingService _logger;
        private readonly DiscordShardedClient _client;

        private struct Config
        {
            [JsonProperty("channels")]
            public List<ulong> ChannelIds { get; private set; }
        }

        private Config ChannelConfig { get; set; }
        private List<SocketTextChannel> ChannelCache { get; set; }

        public GuildTransferService(LoggingService logger, DiscordShardedClient client)
        {
            _logger = logger;
            _client = client;
        }

        public void Initalize()
        {
            _client.MessageReceived += MessageReceived;
            ChannelCache = new List<SocketTextChannel>();

            var json = "";
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();

            ChannelConfig = JsonConvert.DeserializeObject<Config>(json);

        }

        private Task MessageReceived(SocketMessage message)
        {
            // Dont do a thing if a bot sent the message
            if (message.Author.IsBot)
                return Task.CompletedTask;

            if(ChannelConfig.ChannelIds.Contains(message.Channel.Id))
            {
                _logger.LogMessage(Discord.LogSeverity.Debug, "GTS", "Message Received.");

                foreach (ulong id in ChannelConfig.ChannelIds.Where(x => x != message.Channel.Id))
                {
                    var channel = ChannelCache.FirstOrDefault(x => x.Id == id);
                    
                    if(channel == default)
                    {
                        try
                        {
                            channel = (SocketTextChannel)_client.GetChannel(id);
                            ChannelCache.Add(channel);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogMessage(Discord.LogSeverity.Error, "GTS", "Invalid ID in config. Skipping.", ex);

                            continue;
                        }
                    }

                    channel.SendMessageAsync(GetMessageToSend(message));
                }
            }

            return Task.CompletedTask;
        }

        private string GetMessageToSend(SocketMessage message)
        {
            return $"{message.Author.Username} > {message.Content} **[Sent From: {message.Channel.Name} <#{message.Channel.Id}>]**";
        }
    }
}
