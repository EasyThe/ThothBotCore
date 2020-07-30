using Discord;
using Discord.Addons.CommandsExtension;
using Discord.Addons.CommandsExtension.Entities;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThothBotCore.Utilities
{
    public static class HelpCommand
    {
        public static CommandServiceInfo GetCommandServiceInfo(this CommandService commandService, string command)
        {
            var commandInfo = commandService.Search(command).Commands.FirstOrDefault().Command;
            var aliases = string.Join(", ", commandInfo.Aliases);
            var parameters = string.Join(", ", commandInfo.GetCommandParameters());
            return new CommandServiceInfo(null, null, aliases, parameters);
        }

        public static Embed GetHelpEmbed(this CommandService commandService, string command, string prefix)
        {
            EmbedBuilder helpEmbedBuilder;

            if (string.IsNullOrEmpty(command))
            {
                helpEmbedBuilder = commandService.GenerateHelpCommandEmbed(prefix);
            }
            else
            {
                helpEmbedBuilder = GenerateSpecificCommandHelpEmbed(commandService, command, prefix);
            }

            helpEmbedBuilder.WithColor(Constants.DefaultBlueColor);
            helpEmbedBuilder.WithFooter(GenerateUsageFooterMessage(prefix));
            return helpEmbedBuilder.Build();
        }

        private static string GenerateUsageFooterMessage(string botPrefix)
         => $"Use {botPrefix}help [command name] for more information.";

        private static IEnumerable<ModuleInfo> GetModulesWithCommands(this CommandService commandService)
            => commandService.Modules.Where(module => module.Commands.Count > 0);

        private static EmbedBuilder GenerateSpecificCommandHelpEmbed(this CommandService commandService, string command, string prefix)
        {
            var isNumeric = int.TryParse(command[command.Length - 1].ToString(), out var pageNum);

            if (isNumeric)
            {
                command = command.Substring(0, command.Length - 2);
            }
            else
            { 
                pageNum = 1;
            }
            var helpEmbedBuilder = new EmbedBuilder();
            var commandSearchResult = commandService.Search(command);

            var commandModulesList = commandService.Modules.ToList();
            var commandsInfoWeNeed = new List<CommandInfo>();
            foreach (var c in commandModulesList) commandsInfoWeNeed.AddRange(c.Commands.Where(h => string.Equals(h.Name, command, StringComparison.CurrentCultureIgnoreCase)));

            if (pageNum > commandsInfoWeNeed.Count || pageNum <= 0)
            {
                pageNum = 1;
            }

            if (!commandSearchResult.IsSuccess || commandsInfoWeNeed.Count <= 0)
            {
                helpEmbedBuilder.WithTitle("Command not found");
                return helpEmbedBuilder;
            }

            var commandInformation = commandsInfoWeNeed[pageNum - 1].GetCommandInfo(prefix);

            helpEmbedBuilder.WithDescription(commandInformation);

            if (commandsInfoWeNeed.Count >= 2)
                helpEmbedBuilder.WithTitle($"Variant {pageNum}/{commandsInfoWeNeed.Count}.\n" +
                                "_______\n");

            return helpEmbedBuilder;
        }

        private static EmbedBuilder GenerateHelpCommandEmbed(this CommandService commandService, string prefix)
        {
            var helpEmbedBuilder = new EmbedBuilder();
            var commandModules = commandService.GetModulesWithCommands();
            helpEmbedBuilder.WithAuthor(x => { x.IconUrl = Constants.botIcon; x.Name = "Available commands"; });

            foreach (var module in commandModules)
            {
                helpEmbedBuilder.WithTitle("⚡ SMITE");
                if (module.Name.Contains("Smite"))
                {
                    var sb = new StringBuilder();
                    foreach (var command in module.Commands)
                    {
                        if (command.Summary != null)
                        {
                            sb.AppendLine($"🔹`{prefix}{command.Name}{(command.Parameters.Count != 0 ? $" {command.Parameters.First().Name}" : "")}` - {command.Summary}");
                        }
                    }
                    helpEmbedBuilder.WithDescription(sb.ToString());
                }
                if (module.Name.Contains("Bot"))
                {
                    var sb = new StringBuilder();
                    foreach (var command in module.Commands)
                    {
                        var parameters = string.Join(", ", command.GetCommandParameters());
                        if (command.Summary != null)
                        {
                            sb.AppendLine($"🔹`{prefix}{command.Name}{(command.Parameters.Count != 0 ? $" {command.Parameters.First().Name}" : "")}` - {command.Summary}");
                        }
                    }
                    sb.AppendLine("\n🆘 [Support server](http://discord.gg/hU6MTbQ)");
                    helpEmbedBuilder.AddField("🤖 Bot", sb.ToString());
                }
            }
            return helpEmbedBuilder;
        }

        public static string GetCommandInfo(this CommandInfo command, string prefix)
        {
            var aliases = string.Join(", ", command.Aliases);
            var name = command.GetCommandNameWithGroup();
            var summary = command.Summary;
            var sb = new StringBuilder()
                .AppendLine($"**Usage**: {prefix}{name}{(command.Parameters.Count != 0 ? $" {command.Parameters.First().Name}" : "")}")
                .AppendLine($"**Description**: {summary}")
                .Append($"**Aliases**: {aliases}");
            return sb.ToString();
        }
    }
}
