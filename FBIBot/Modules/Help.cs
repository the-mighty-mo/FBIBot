using Discord;
using Discord.Interactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules
{
    public class Help : InteractionModuleBase<SocketInteractionContext>
    {
        private const string general =
            "ping\n" +
            "  - Returns the bot's Server and API latencies\n\n" +
            "verify (CAPTCHA response)\n" +
            "  - Verify that you are not a spy from the CCP\n\u200b";

        private const string help = "Pass in a parameter to the help command to get info about a group of commands.";

        private const string mod1 =
            "warn [user mention/ID] (hours) (reason)\n" +
            "  - Gives the user a warning to stop protesting capitalism\n\n" +
            "get-warnings [user mention/ID]\n" +
            "  - Gets the number of warnings and mod logs for the warnings for the given user\n\n" +
            "remove-warning [user mention/ID] [mod log ID]\n" +
            "  - Removes the given warning from the user\n\n" +
            "remove-warnings [user mention/ID] (count)\n" +
            "  - Removes a number of warnings from the user; removes the oldest first\n\n" +
            "mute [user mention/ID] (minutes) (reason)\n" +
            "  - Puts the user under house arrest so they can't type or speak in chats\n\n" +
            "unmute [user mention/ID]\n" +
            "  - Frees the house-arrested user";

        private const string mod2 =
            "arrest [user mention/ID] (minutes) (reason)\n" +
            "  - Sends the user to Guantanamo Bay for a bit; **This command ignores modifymutedroles and creates its own role and channel**\n\n" +
            "free [user mention/ID]\n" +
            "  - Frees the user from Guantanamo Bay because the Constitution exists\n\n" +
            "kick [user mention/ID] (reason)\n" +
            "  - Deports the criminal to probably Europe\n\n" +
            "ban [user mention/ID] (days) (prune days / \"\") (reason)\n" +
            "  - Gives the communist the ~~ban~~ freedom hammer\n\n" +
            "unban [user mention/ID]\n" +
            "  - Permits the now-ex-KGB spy to reenter the server";

        private const string mod3 =
            "modify-reason [mod log ID] (reason)\n" +
            "  - Modifies the reason for the given mod log\n\n" +
            "purge all [count (default: 100, max: 1000)]\n" +
            "  - Shreds, burns, and disposes of a number of messages from the channel\n\n" +
            "purge user [user mention] [count (default: 10, max: 100)]\n" +
            "  - Shreds, burns, and disposes of a number of messages from a user in the channel\n\n" +
            "slowmode [seconds]\n" +
            "  - Enables slowmode in the chat; max time is 21600 seconds; *Disables slowmode if no time is given*";

        private static readonly string config1 =
            "config\n" +
            "  - Displays the current bot configuration\n\n" +
            "set-role verify [role mention/ID] [true / **false** (default)]\n" +
            "  - Sets the verification role and, if true, slowly gives out the role to (and removes the old role from) democracy-loving citizens; *Unsets if no role is given*\n\n" +
            "verify-all\n" +
            "  - Grants citizenship all current freedom-loving Americans\n\n" +
            "unverify [user mention/ID] (reason)\n" +
            "  - Removes the verification role from the user and removes the user from the list of verified users\n\n" +
            "set-role mute [role mention/ID]\n" +
            "  - Sets the role for members under house arrest (muted); *Unsets if no role is given*\n\n" +
            "modify-muted-roles [true/enable / false/disable]\n" +
            "  - When enabled, allows the bot to remove and save the roles of muted members; enable this unless you have manually configured the server's muted role\n\n" +
            "raid-mode\n" +
            "  - When enabled, sets the server verification level to High (Tableflip) and kicks any joining members; **Toggle enable/disable**";

        private const string config2 =
            "add-role mod [role mention/ID]\n" +
            "  - Adds the role to a list of assistants of the bureau\n\n" +
            "remove-role mod [role mention/ID]\n" +
            "  - Removes the role from the list of assistants of the bureau out of suspicion\n\n" +
            "add-role admin [role mention/ID]\n" +
            "  - Adds the role to a list of local directors of the bureau\n\n" +
            "remove-role admin [role mention/ID]\n" +
            "  - Removes the role from the list of local directors of the bureau due to presidential disapproval\n\n" +
            "set-log mod [channel mention/ID]\n" +
            "  - Sets the channel for the Mod Log; *Unsets if no channel is given*\n\n" +
            "set-log captcha [channel mention/ID]\n" +
            "  - Sets the channel for the CAPTCHA Log; *Unsets if no channel is given*\n\n" +
            "set-log welcome [channel mention/ID]\n" +
            "  - Sets the channel for welcome messages; *Unsets if no channel is given*\n\n" +
            "clear-mod-log [clear messages (true / **false** [default])]\n" +
            "  - Clears the Mod Log numbers and, if specified, all Mod Log messages; **Clears all warnings**";

        private const string automod1 =
            "auto-surveillance [true/enable / false/disable]\n" +
            "  - Permits the FBI to perform surveillance operations on server members ~~(we recommend you enable this for maximum LARP)~~\n\n" +
            "automod zalgo [true/enable / false/disable]\n" +
            "  - When enabled, the FBI will detect if a message was leaked from Area 51 and take it down with a warning\n\n" +
            "automod spam [true/enable / false/disable]\n" +
            "  - When enabled, the FBI will detect if users send multiple identical messages and take them down with a warning\n\n" +
            "automod single-spam [true/enable / false/disable]\n" +
            "  - When enabled, the FBI will detect if the user sends one big, spammy message and take it down with a warning";

        private const string automod2 =
            "automod mass-mention [true/enable / false/disable]\n" +
            "  - When enabled, the FBI will take down, with a warning, messages mentioning all the rich people the user wants to eat (5+)\n\n" +
            "automod caps [true/enable / false/disable]\n" +
            "  - When enabled, the FBI will take down, with a warning, VERY LOUD PROTESTS\n\n" +
            "automod invite [true/enable / false/disable]\n" +
            "  - When enabled, the FBI will detect invites to the socialist party and *kindly* remove them\n\n" +
            "automod link [true/enable / false/disable]\n" +
            "  - When enabled, the FBI will dispose of all messages containing links to communist propaganda websites";

        public enum HelpParam
        {
            [ChoiceDisplay("Mod - Page 1")]
            Mod1,
            [ChoiceDisplay("Mod - Page 2")]
            Mod2,
            [ChoiceDisplay("Mod - Page 3")]
            Mod3,
            [ChoiceDisplay("Config - Page 1")]
            Config1,
            [ChoiceDisplay("Config - Page 2")]
            Config2,
            [ChoiceDisplay("AutoMod - Page 1")]
            AutoMod1,
            [ChoiceDisplay("AutoMod - Page 2")]
            AutoMod2,
        }

        [SlashCommand("help", "List of commands")]
        public async Task HelpAsync(HelpParam? param = null)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle(SecurityInfo.botName);

            List<EmbedFieldBuilder> fields = new();

            EmbedFieldBuilder field = new EmbedFieldBuilder()
                .WithIsInline(true);
            switch (param)
            {
            case HelpParam.Mod1:
                field.WithName("Moderator Commands - Page 1")
                    .WithValue(mod1);
                break;
            case HelpParam.Mod2:
                field.WithName("Moderator Commands - Page 2")
                    .WithValue(mod2);
                break;
            case HelpParam.Mod3:
                field.WithName("Moderator Commands - Page 3")
                    .WithValue(mod3);
                break;
            case HelpParam.Config1:
                field.WithName("Configuration Commands - Page 1")
                    .WithValue(config1);
                break;
            case HelpParam.Config2:
                field.WithName("Configuration Commands - Page 2")
                    .WithValue(config2);
                break;
            case HelpParam.AutoMod1:
                field.WithName("AutoMod Commands - Page 1")
                    .WithValue(automod1);
                break;
            case HelpParam.AutoMod2:
                field.WithName("Automod Commands - Page 2")
                    .WithValue(automod2);
                break;
            default:
                fields.Add(
                    new EmbedFieldBuilder()
                        .WithIsInline(false)
                        .WithName("General Commands")
                        .WithValue(general)
                );
                field.WithName("`help` Information")
                    .WithValue(help);
                break;
            }
            fields.Add(field);
            embed.WithFields(fields);

            await Context.Interaction.RespondAsync("Need a little democracy, freedom, and justice?", embed: embed.Build(), ephemeral: true);
        }
    }
}