using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private const string help = "mod\n" +
            "  - Displays page 1 of moderation commands\n\n" +
            "mod2\n" +
            "  - Displays page 2 of moderation commands\n\n" +
            "config\n" +
            "  - Displays page 1 of bot configuration commands\n\n" +
            "config2\n" +
            "  - Displays page 2 of bot configuration commands\n\n" +
            "automod\n" +
            "  - Displays page 1 of automod configuration commands\n\n" +
            "automod2\n" +
            "  - Displays page 2 of automod configuration commands";

        private const string mod = "slowmode [seconds]\n" +
            "  - Enables slowmode in the chat; max time is 21600 seconds; *Disables slowmode if no time is given*\n\n" +
            "modifyreason [mod log ID] [reason (optional)]\n" +
            "  - Modifies the reason for the given mod log\n\n" +
            "warn [user mention / user ID] [reason (optional)]\n" +
            "  - Gives the user a warning to stop protesting capitalism\n\n" +
            "tempwarn [user mention / user ID] [hours] [reason (optional)]\n" +
            "  - Gives the user a temporary warning to stop protesting capitalism\n\n" +
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

        private const string mod2 = "arrest [user mention / user ID] [minutes (optional)]\n" +
            "  - Sends the user to Guantanamo Bay for a bit; **This command ignores modifymutedroles and creates its own role and channel**\n\n" +
            "free [user mention / user ID]\n" +
            "  - Frees the user from Guantanamo Bay because the Constitution exists\n\n" +
            "kick [user mention / user ID] [reason (optional)]\n" +
            "  - Deports the criminal to probably Europe\n\n" +
            "tempban [user mention / user ID] [days] [prune days (optional)] [reason (optional)]\n" +
            "  - Temporarily exiles the user to Mexico\n\n" +
            "ban [user mention / user ID] [prune days (optional)] [reason (optional)]\n" +
            "  - Gives the communist the ~~ban~~ freedom hammer\n\n" +
            "unban [user mention / user ID]\n" +
            "  - Permits the now-ex-KGB spy to reenter the server\n\n" +
            "purge [count (default: 100, max: 1000)]\n" +
            "  - Shreds, burns, and disposes of a number of messages from the channel\n\n" +
            "purge [user mention] [count (default: 10, max: 100)]\n" +
            "  - Shreds, burns, and disposes of a number of messages from a user in the channel";

        private static readonly string config = "config\n" +
            "  - Displays the current bot configuration\n\n" +
            "setprefix [prefix]\n" +
            $"  - Sets the bot prefix; default is {CommandHandler.prefix}\n\n" +
            "setverify [role mention / role ID] [true / **false** (default)]\n" +
            "  - Sets the verification role and, if true, slowly gives out the role to (and removes the old role from) democracy-loving citizens; *Unsets if no role is given*\n\n" +
            "verifyall\n" +
            "  - Grants citizenship all current freedom-loving Americans\n\n" +
            "unverify [user mention / user ID] [reason (optional)]\n" +
            "  - Removes the verification role from the user and removes the user from the list of verified users\n\n" +
            "setmute [role mention / role ID]\n" +
            "  - Sets the role for members under house arrest (muted); *Unsets if no role is given*\n\n" +
            "modify-muted-roles [true/enable / false/disable]\n" +
            "  - When enabled, allows the bot to remove and save the roles of muted members; enable this unless you have manually configured the server's muted role\n\n" +
            "raidmode\n" +
            "  - When enabled, sets the server verification level to High (Tableflip) and kicks any joining members; **Toggle enable/disable**";

        private const string config2 = "addmod [role mention / role ID]\n" +
            "  - Adds the role to a list of assistants of the bureau\n\n" +
            "removemod [role mention / role ID]\n" +
            "  - Removes the role from the list of assistants of the bureau out of suspicion\n\n" +
            "addadmin [role mention / role ID]\n" +
            "  - Adds the role to a list of local directors of the bureau\n\n" +
            "removeadmin [role mention / role ID]\n" +
            "  - Removes the role from the list of local directors of the bureau due to presidential disapproval\n\n" +
            "setmodlog [channel mention / channel ID]\n" +
            "  - Sets the channel for the Mod Log; *Unsets if no channel is given*\n\n" +
            "setcaptchalog [channel mention / channel ID]\n" +
            "  - Sets the channel for the CAPTCHA Log; *Unsets if no channel is given*\n\n" +
            "setwelcome [channel mention / channel ID]\n" +
            "  - Sets the channel for welcome messages; *Unsets if no channel is given*\n\n" +
            "clearmodlog [clear messages (true / **false** [default])]\n" +
            "  - Clears the Mod Log numbers and, if specified, all Mod Log messages; **Clears all warnings**";

        private const string automod = "auto-surveillance [true/enable / false/disable]\n" +
            "  - Permits the FBI to perform surveillance operations on server members ~~(we recommend you enable this)~~\n\n" +
            "anti-zalgo [true/enable / false/disable]\n" +
            "  - When enabled, the FBI will detect if a message was leaked from Area 51 and take it down with a warning\n\n" +
            "anti-spam [true/enable / false/disable]\n" +
            "  - When enabled, the FBI will detect if users send multiple identical messages and take them down with a warning\n\n" +
            "anti-singlespam [true/enable / false/disable]\n" +
            "  - When enabled, the FBI will detect if the user sends one big, spammy message and takes it down with a warning";

        private const string automod2 = "anti-massmention [true/enable / false/disable]\n" +
            "  - When enabled, the FBI will take down, with a warning, messages mentioning all the rich people the user wants to eat (5+)\n\n" +
            "anti-caps [true/enable / false/disable]\n" +
            "  - When enabled, the FBI will take down, with a warning, VERY LOUD PROTESTS\n\n" +
            "anti-invite [true/enable / false/disable]\n" +
            "  - When enabled, the FBI will detect invites to the socialist party and *kindly* remove them\n\n" +
            "anti-link [true/enable / false/disable]\n" +
            "  - When enabled, the FBI will dispose of all messages containing links to communist propaganda websites";

        [Command("help")]
        public async Task HelpAsync(string param = null)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("The FBI")
                .WithCurrentTimestamp();

            string prefix = CommandHandler.prefix;
            if (Context.Guild != null)
            {
                prefix = await configDatabase.Prefixes.GetPrefixAsync(Context.Guild);
            }
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Prefix")
                    .WithValue(prefix +
                        "\n**or**\n" +
                        Context.Client.CurrentUser.Mention + "\n\u200b")
            };

            EmbedFieldBuilder field = new EmbedFieldBuilder()
                .WithIsInline(true);
            switch (param)
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
                field.WithName("AutoMod Commands - Page 1")
                    .WithValue(automod);
                break;
            case "automod2":
                field.WithName("Automod Commands - Page 2")
                    .WithValue(automod2);
                break;
            default:
                field.WithName("`help` Parameters")
                    .WithValue(help);
                break;
            }
            fields.Add(field);
            embed.WithFields(fields);

            await Context.Channel.SendMessageAsync("Need a little democracy, freedom, and justice?", false, embed.Build());
        }
    }
}
