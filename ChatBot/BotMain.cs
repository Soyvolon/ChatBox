using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ChatBot.Properties;
using ChatBot.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot
{
    public class BotMain
    {
        /// <summary>
        /// Set the LogLevel of the bot. Defaults to Debug
        /// </summary>
        public static LogSeverity LogLevel { get; set; } = LogSeverity.Debug;

        private static DiscordShardedClient Client { get; set; }

        public async Task Start()
        {
            // create the placeholder command service and init module.
            var _commands = new CommandService();
            var init = new Initialize(commands: _commands);

            // Setup the socket config.
            var botConfig = new DiscordSocketConfig
            {
                GuildSubscriptions = false,
                TotalShards = 1,
                LogLevel = LogLevel,
                RateLimitPrecision = RateLimitPrecision.Millisecond,
                UseSystemClock = false
            };

            // Log the client in for the first time and get the Discord recommnded
            // shard count.

            Client = init.BuildInitalBoot().GetRequiredService<DiscordShardedClient>();

            await Client.LoginAsync(TokenType.Bot, Resources.Token).ConfigureAwait(false);

            botConfig.TotalShards = await Client.GetRecommendedShardCountAsync().ConfigureAwait(false);

            await Client.LogoutAsync().ConfigureAwait(false);

            // Recreate the init as to use the new shard config and initalize the services.

            init = new Initialize(_commands, new DiscordShardedClient(botConfig));

            var services = init.BuildServiceProvider();

            Client = services.GetRequiredService<DiscordShardedClient>();

            await services.GetRequiredService<CommandHandlingService>().InitalizeAsync();

            services.GetRequiredService<LoggingService>().Initalize();

            services.GetRequiredService<GuildTransferService>().Initalize();

            // Start the client.

            await Client.LoginAsync(TokenType.Bot, Resources.Token).ConfigureAwait(false);

            await Client.StartAsync();
        }
    }

    public class Initialize
    {
        private readonly CommandService _commands;
        private readonly DiscordShardedClient _client;

        public Initialize(CommandService commands = null, DiscordShardedClient client = null)
        {
            _commands = commands ?? new CommandService();
            _client = client ?? new DiscordShardedClient();
        }

        public IServiceProvider BuildInitalBoot() => new ServiceCollection()
            .AddSingleton(_client)
            .BuildServiceProvider();

        public IServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton<CommandHandlingService>()
            .AddSingleton<LoggingService>()
            // Add other services here.
            .AddSingleton<GuildTransferService>()
            .BuildServiceProvider();
    }
}
