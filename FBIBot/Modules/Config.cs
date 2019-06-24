using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBIBot.Modules
{
    public class Config : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help(params string[] args)
        {
            if (args.Length == 0 || args[0] != SecurityInfo.botID)
            {
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("The FBI")
                .WithCurrentTimestamp();
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();

            fields.Add(new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Prefix")
                .WithValue(CommandHandler.prefix.ToString() + "\n\u200b")
            );

            bool jumpToHelp = false;

            Help:
            if (args.Length == 1 || jumpToHelp)
            {
                fields.Add(new EmbedFieldBuilder()
                    .WithIsInline(true)
                    .WithName("`\\help 0205` Parameters")
                    .WithValue(
                    "admin\n" +
                    "  - Displays administrator commands\n\n" +
                    "mod\n" +
                    "  - Displays moderator commands\n\n" +
                    "config\n" +
                    "  - Displays configuration commands")
                );
            }
            else
            {
                EmbedFieldBuilder field = new EmbedFieldBuilder()
                    .WithIsInline(true);

                switch (args[1])
                {
                case "admin":
                    field.WithName("Administrator Commands")
                        .WithValue(
                        "WIP");
                    break;
                case "mod":
                    field.WithName("Moderator Commands")
                        .WithValue(
                        "WIP");
                    break;
                case "config":
                    field.WithName("Configuration Commands")
                        .WithValue(
                        "WIP");
                    break;
                default:
                    jumpToHelp = true;
                    goto Help;
                }

                fields.Add(field);
            }

            embed.WithFields(fields);
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
