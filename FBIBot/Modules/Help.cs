using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private static readonly string help = "admin\n" +
                    "  - Displays administrator commands\n\n" +
                    "mod\n" +
                    "  - Displays moderator commands\n\n" +
                    "config\n" +
                    "  - Displays configuration commands";
        private static readonly string admin = "WIP";
        private static readonly string mod = "WIP";
        private static readonly string config = "setprefix\n" +
            $"  - Sets the bot prefix; default is {CommandHandler.prefix}\n\n" +
            "setverify [role mention / role ID]\n" +
            "  - Sets the role for verified members\n\n" +
            "verifyall\n" +
            "  - Verifies all current members";

        [Command("help")]
        public async Task HelpAsync(params string[] args)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("The FBI")
                .WithCurrentTimestamp();
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Prefix")
                .WithValue(await Config.Prefix.GetPrefixAsync(Context.Guild) +
                "\n**or**\n" +
                Context.Client.CurrentUser.Mention + "\n\u200b")
            };

            bool jumpToHelp = false;

            Help:
            if (args.Length == 0 || jumpToHelp)
            {
                fields.Add(new EmbedFieldBuilder()
                    .WithIsInline(true)
                    .WithName("`help` Parameters")
                    .WithValue(help));
            }
            else
            {
                EmbedFieldBuilder field = new EmbedFieldBuilder()
                    .WithIsInline(true);

                switch (args[0])
                {
                case "admin":
                    field.WithName("Administrator Commands")
                        .WithValue(admin);
                    break;
                case "mod":
                    field.WithName("Moderator Commands")
                        .WithValue(mod);
                    break;
                case "config":
                    field.WithName("Configuration Commands")
                        .WithValue(config);
                    break;
                default:
                    jumpToHelp = true;
                    goto Help;
                }

                fields.Add(field);
            }

            embed.WithFields(fields);

            await Context.Channel.SendMessageAsync("Need a little democracy, freedom, and justice?\n" +
                "No? Just want my commands?\n" +
                "Fine, here you go.", false, embed.Build());
        }
    }
}
