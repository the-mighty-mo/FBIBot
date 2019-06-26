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
        private static readonly string mod = "mute [user mention / user ID] [minutes (optional)] [reason (optional)]\n" +
            "  - Puts the user under house arrest so they can't type in chat or speak in voice chat\n\n" +
            "unmute [user mention / user ID]\n" +
            "  - Frees the house-arrested user\n\n" +
            "arrest [user mention / user ID] [minutes (optional)]\n" +
            "  - Sends the user to Guantanamo Bay for a bit\n\n" +
            "free [user mention / user ID]\n" +
            "  - Frees the user from Guantanamo Bay because the Constitution exists; **This command ignores modifymutedroles and creates its own role**\n\n" +
            "kick [user mention / user ID] [reason (optional)]\n" +
            "  - Deports the communist to probably Europe\n\n" +
            "tempban [user mention / user ID] [days] [reason (optional)]\n" +
            "  - Temporarily exiles the user to Mexico\n\n" +
            "ban [user mention / user ID] [prune days (optional)] [reason (optional)]\n" +
            "  - Gives the user the ~~ban~~ freedom hammer";
        private static readonly string config = "config\n" +
            "  - Displays the current bot configuration\n\n" +
            "setprefix\n" +
            $"  - Sets the bot prefix; default is {CommandHandler.prefix}\n\n" +
            "setverify [role mention / role ID]\n" +
            "  - Sets the role for democracy-loving citizens\n\n" +
            "verifyall\n" +
            "  - Grants citizenship all current freedom-loving Americans\n\n" +
            "setmute [role mention / role ID]\n" +
            "  - Sets the role for members under house arrest (muted). Unsets if no role is given.\n\n" +
            "modifymutedroles [true/enable / **false/disable** (default)]\n" +
            "  - When enabled, allows the bot to remove and save the roles of muted members; we recommend you enable thus unless you have manually configured the server's muted role";

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
