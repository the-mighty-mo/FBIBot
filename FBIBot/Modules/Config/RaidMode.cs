using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class RaidMode : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("raid-mode", "Toggle enable; Sets the server verification level to High (Tableflip) and kicks any joining members")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageGuild)]
        public async Task RaidModeAsync()
        {
            VerificationLevel? level = await raidModeDatabase.RaidMode.GetVerificationLevelAsync(Context.Guild);
            bool isDisabled = level == null;

            if (isDisabled)
            {
                await raidModeDatabase.RaidMode.SaveVerificationLevelAsync(Context.Guild);

                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(new Color(206, 15, 65))
                    .WithDescription(":rotating_light: :rotating_light: WE HERE AT THE FBI ARE IN RAID MODE! :rotating_light: :rotating_light:");

                await Task.WhenAll
                (
                    Context.Guild.ModifyAsync(x => x.VerificationLevel = VerificationLevel.High),
                    Context.Interaction.RespondAsync(embed: emb.Build()),
                    RaidModeModLog.SendToModLogAsync(Context.User as SocketGuildUser, true)
                );
                return;
            }

            Task[] cmds =
            {
                Context.Guild.ModifyAsync(x => x.VerificationLevel = (VerificationLevel)level!),
                raidModeDatabase.RaidMode.RemoveVerificationLevelAsync(Context.Guild)
            };

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(12, 156, 24))
                .WithDescription("The FBI is now out of raid mode. Surveillance results will be posted in the Mod Logs.");

            await Task.WhenAll
            (
                Context.Interaction.RespondAsync(embed: embed.Build()),
                RaidModeModLog.SendToModLogAsync(Context.User as SocketGuildUser, false)
            );
            await Task.WhenAll
            (
                cmds[0],
                cmds[1],
                SendUsersAsync()
            );
        }

        private async Task SendUsersAsync()
        {
            List<string> blockedUsers = await raidModeDatabase.UsersBlocked.GetBlockedUsersAsync(Context.Guild);
            SocketTextChannel channel = await modLogsDatabase.ModLogChannel.GetModLogChannelAsync(Context.Guild);
            List<EmbedFieldBuilder> fields = new();
            int messages = 1;

            if (blockedUsers.Count == 0)
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(new Color(12, 156, 24))
                    .WithTitle($"FBI Raid Mode Surveillance Results{(messages > 1 ? $" ({messages})" : "")}")
                    .WithCurrentTimestamp();

                EmbedFieldBuilder field = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("No users blocked")
                    .WithValue("No users attempted to join the server during Raid Mode.");
                emb.AddField(field);

                await channel.SendMessageAsync(embed: emb.Build());
            }
            else
            {
                int i = 1;
                int j = 1;
                string blocked = "";
                foreach (string userID in blockedUsers)
                {
                    if (j > 3)
                    {
                        EmbedBuilder emb = new EmbedBuilder()
                            .WithColor(new Color(206, 15, 65))
                            .WithTitle($"FBI Raid Mode Surveillance Results ({messages})")
                            .WithCurrentTimestamp()
                            .WithFields(fields);
                        await channel.SendMessageAsync(embed: emb.Build());
                        j = 1;
                        messages++;
                        fields.Clear();
                    }

                    blocked += $"{(i > 1 ? "\n" : "")}<@{userID}> (ID: {userID})";

                    if (++i > 20)
                    {
                        EmbedFieldBuilder field = new EmbedFieldBuilder()
                            .WithIsInline(false)
                            .WithName($"Blocked Users ({3 * (messages - 1) + j})")
                            .WithValue(blocked);
                        fields.Add(field);
                        i = 1;
                        j++;
                        blocked = "";
                    }
                }
                if (i is <= 20 and > 1)
                {
                    EmbedFieldBuilder field = new EmbedFieldBuilder()
                        .WithIsInline(false)
                        .WithName($"Blocked Users{(j > 1 || messages > 1 ? $" ({3 * (messages - 1) + j})" : "")}")
                        .WithValue(blocked);
                    fields.Add(field);
                }

                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(new Color(206, 15, 65))
                    .WithTitle($"FBI Raid Mode Surveillance Results{(messages > 1 ? $" ({messages})" : "")}")
                    .WithCurrentTimestamp()
                    .WithFields(fields);

                await channel.SendMessageAsync(embed: embed.Build());
            }

            await raidModeDatabase.UsersBlocked.RemoveBlockedUsersAsync(Context.Guild);
        }
    }
}