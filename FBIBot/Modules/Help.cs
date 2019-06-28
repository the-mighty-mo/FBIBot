using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private static readonly string help = "mod\n" +
            "  - Displays page 1 of moderation commands\n\n" +
            "mod2\n" +
            "  - Displays page 2 of moderation commands\n\n" +
            "config\n" +
            "  - Displays page 1 of bot configuration commands\n\n" +
            "config2\n" +
            "  - Displays page 2 of bot configuration commands\n\n" +
            "automod\n" +
            "  - Displays automod configuration commands";
        private static readonly string mod = "slowmode [seconds]\n" +
            "  - Enables slowmode in the chat; max time is 21600 seconds; *Disables slowmode if no time is given*\n\n" +
            "modifyreason [mod log ID] [reason (optional)]\n" +
            "  - Modifies the reason for the given mod log\n\n" +
            "warn [user mention / user ID] [hours (optional)] [reason (optional)]\n" +
            "  - Gives the user a warning to stop protesting capitalism\n\n" +
            "getwarnings [user mention / user ID]\n" +
            "  - Gets the number of warnings and mod logs for the warnings for the given user\n\n" +
            "removewarning [user mention / user ID] [mod log ID]\n" +
            "  - Removes the given warning from the user\n\n" +
            "removewarnings [user mention / user ID] [count (optional)]\n" +
            "  - Removes a number of warnings from the user; removes the oldest first\n\n" +
            "mute [user mention / user ID] [minutes (optional)] [reason (optional)]\n" +
            "  - Puts the user under house arrest so they can't type or speak in chats\n\n" +
            "unmute [user mention / user ID]\n" +
            "  - Frees the house-arrested user";
        private static readonly string mod2 = "arrest [user mention / user ID] [minutes (optional)]\n" +
            "  - Sends the user to Guantanamo Bay for a bit\n\n" +
            "free [user mention / user ID]\n" +
            "  - Frees the user from Guantanamo Bay because the Constitution exists; **This command ignores modifymutedroles and creates its own role and channel**\n\n" +
            "kick [user mention / user ID] [reason (optional)]\n" +
            "  - Deports the criminal to probably Europe\n\n" +
            "tempban [user mention / user ID] [days] [prune days (optional)] [reason (optional)]\n" +
            "  - Temporarily exiles the user to Mexico\n\n" +
            "ban [user mention / user ID] [prune days (optional)] [reason (optional)]\n" +
            "  - Gives the communist the ~~ban~~ freedom hammer\n\n" +
            "unban [user mention / user ID]\n" +
            "  - Permits the now-ex-KGB spy to reenter the server\n\n" +
            "purge [count (default: 100)]\n" +
            "  - Shreds, burns, and disposes of a number of messages from the channel\n\n" +
            "purge [user mention] [count (default: 10)]\n" +
            "  - Shreds, burns, and disposes of a number of messages from a user in the channel";
        private static readonly string config = "config\n" +
            "  - Displays the current bot configuration\n\n" +
            "setprefix\n" +
            $"  - Sets the bot prefix; default is {CommandHandler.prefix}\n\n" +
            "setverify [role mention / role ID]\n" +
            "  - Sets the role for democracy-loving citizens; *Unsets if no role is given*\n\n" +
            "verifyall\n" +
            "  - Grants citizenship all current freedom-loving Americans\n\n" +
            "setmute [role mention / role ID]\n" +
            "  - Sets the role for members under house arrest (muted); *Unsets if no role is given*\n\n" +
            "modify-muted-roles [true/enable / **false/disable** (default)]\n" +
            "  - When enabled, allows the bot to remove and save the roles of muted members; we recommend you enable thus unless you have manually configured the server's muted role";
        private static readonly string config2 = "add-modrole [role mention / role ID]\n" +
            "  - Adds the role to a list of assistants of the agency\n\n" +
            "remove-modrole [role mention / role ID]\n" +
            "  - Removes the role from the list of assistants of the agency out of suspicion\n\n" +
            "add-adminrole [role mention / role ID]\n" +
            "  - Adds the role to a list of local directors of the agency\n\n" +
            "remove-adminrole [role mention / role ID]\n" +
            "  - Removes the role from the list of local directors of the agency due to presidential disapproval\n\n" +
            "setmodlog [channel mention / channel ID]\n" +
            "  - Sets the channel for the Mod Log; *Unsets if no channel is given*\n\n" +
            "clearmodlog [clear messages (true / **false** [default])]\n" +
            "  - Clears the Mod Log numbers and, if specified, all Mod Log messages; **Clears all warnings**";
        private static readonly string automod = "WIP";

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
                case "mod":
                    field.WithName("Moderator Commands - Page 1")
                        .WithValue(mod);
                    break;
                case "mod2":
                    field.WithName("Moderator Commands - Page 2")
                        .WithValue(mod2);
                    break;
                case "config":
                    field.WithName("Configuration Commands - Page 1")
                        .WithValue(config);
                    break;
                case "config2":
                    field.WithName("Configuration Commands - Page 2")
                        .WithValue(config2);
                    break;
                case "automod":
                    field.WithName("Automod Commands")
                        .WithValue(automod);
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
