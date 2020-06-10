using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ChatBot.Properties;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Services
{
    public class CommandHandlingService
    {
        private readonly IServiceProvider _services;
        private readonly CommandService _commands;
        private readonly DiscordShardedClient _client;
        private readonly LoggingService _logger;

        public CommandHandlingService(IServiceProvider services, CommandService commands, DiscordShardedClient client, LoggingService logger)
        {
            _services = services;
            _commands = commands;
            _client = client;
            _logger = logger;
        }

        public async Task InitalizeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services).ConfigureAwait(false);

            _client.MessageReceived += MessageReceivedAsync;
            _commands.CommandExecuted += CommandExecutedAsync;
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message))
                return;
            if (message.Source != MessageSource.User)
                return;

            var context = new ShardedCommandContext(_client, message);

            // This value holds the offset where the prefix ends
            var argPos = 0;

            if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos) && !message.HasStringPrefix(Resources.Prefix, ref argPos))
            {
                return;
            }

            await _commands.ExecuteAsync(context, argPos, _services).ConfigureAwait(false);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
            {
                //TODO: Implement state machines later
                //if (CheckStateMachines(context))
                await context.Channel.SendMessageAsync($"Command not found. Use `{Resources.Prefix}help` for a list of commands.");
                return;
            }

            // the command was succesful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
            {
                await _logger.LogMessage(LogSeverity.Info, "CHS", $"{context.User.Username}#{context.User.Discriminator} successfully executed {command.Value.Name}");
                return;
            }

            // the command failed, let's notify the user that something happened.
            // TODO: Rework error messages, filter this into the CommandErrorResponder (build CER into this file)

            await context.Channel.SendMessageAsync($"error: {((ExecuteResult)result).Exception}").ConfigureAwait(false);
        }
    }
}
