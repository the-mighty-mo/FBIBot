using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    public class GetWarnings : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("get-warnings", "Gets the number of warnings and mod logs for the warnings for the given user")]
        [RequireMod]
        public async Task GetWarnsAsync(SocketGuildUser user)
        {
            List<ulong> ids = await modLogsDatabase.Warnings.GetWarningsAsync(user);
            if (ids.Count == 0)
            {
                await Context.Interaction.RespondAsync("Our security team has informed us that the given user does not have any warnings.");
                return;
            }

            List<ulong> idsPastDay = new();
            List<ulong> idsPastHour = new();
            foreach (ulong id in ids)
            {
                IUserMessage msg = await modLogsDatabase.ModLogs.GetModLogAsync(Context.Guild, id);
                TimeSpan timeSinceLog = Context.Interaction.CreatedAt - msg.Timestamp;
                if (timeSinceLog <= TimeSpan.FromDays(1))
                {
                    idsPastDay.Add(id);
                    if (timeSinceLog <= TimeSpan.FromHours(1))
                    {
                        idsPastHour.Add(id);
                    }
                }
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(255, 213, 31))
                .WithTitle($"Federal Bureau of Investigation - Warnings for {user.Nickname ?? user.Username}")
                .WithCurrentTimestamp();

            EmbedFieldBuilder warnings = new EmbedFieldBuilder()
                    .WithIsInline(true)
                    .WithName($"Total Warnings: {ids.Count}")
                    .WithValue($"- Past 24 hours: {idsPastDay.Count}\n" +
                               $"- Past hour: {idsPastHour.Count}");
            embed.AddField(warnings);

            EmbedFieldBuilder modLogs = new EmbedFieldBuilder()
                    .WithIsInline(true)
                    .WithName($"See mod logs: {string.Join(", ", ids)}")
                    .WithValue($"- Past 24 hours: {string.Join(", ", idsPastDay)}\n" +
                               $"- Past hour: {string.Join(", ", idsPastHour)}");
            embed.AddField(modLogs);

            await Context.Interaction.RespondAsync(embed: embed.Build());
        }
    }
}