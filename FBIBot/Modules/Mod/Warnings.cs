using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    [Group("warnings", "Manages warnings on the server")]
    [RequireMod]
    public class Warnings : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("get", "Gets the number of warnings and mod logs for the warnings for the given user")]
        public Task GetWarnsAsync(SocketUser user) =>
            GetWarnsAsync((user as SocketGuildUser)!);

        private async Task GetWarnsAsync(SocketGuildUser user)
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
                IUserMessage? msg = await modLogsDatabase.ModLogs.GetModLogAsync(Context.Guild, id);
                if (msg == null)
                {
                    continue;
                }

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

        [Group("remove", "Removes warnings from a user")]
        public class Remove : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("id", "Removes the given warning from the user")]
            public Task RemoveWarnAsync([RequireInvokerHierarchy("remove warnings from")] SocketUser user, ulong id) =>
                RemoveWarnAsync((user as SocketGuildUser)!, id);

            private async Task RemoveWarnAsync(SocketGuildUser user, ulong id)
            {
                if (!await modLogsDatabase.Warnings.GetWarningAsync(user, id))
                {
                    await Context.Interaction.RespondAsync($"Our security team has informed us that the given warning does not exist.");
                    return;
                }

                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(new Color(12, 156, 24))
                    .WithDescription($"The given warning has been pardoned.");

                await Task.WhenAll
                (
                    Context.Interaction.RespondAsync(embed: embed.Build()),
                    RemoveWarningModLog.SendToModLogAsync((Context.User as SocketGuildUser)!, user, id),
                    modLogsDatabase.Warnings.RemoveWarningAsync(user, id)
                );
            }

            [SlashCommand("count", "Removes a number of warnings from the user. Removes the oldest first")]
            public Task RemoveWarnsAsync([RequireInvokerHierarchy("remove warnings from")] SocketUser user, [Summary(description: "Default: all")] int? count = null) =>
                RemoveWarnsAsync((user as SocketGuildUser)!, count);

            private async Task RemoveWarnsAsync(SocketGuildUser user, int? count = null)
            {
                if (count == 0)
                {
                    await Context.Interaction.RespondAsync("Why are you trying to remove ***0*** warnings? I have more important things to do.");
                    return;
                }
                if ((await modLogsDatabase.Warnings.GetWarningsAsync(user)).Count == 0)
                {
                    await Context.Interaction.RespondAsync("Our security team has informed us that the given user does not have any warnings.");
                    return;
                }

                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(new Color(12, 156, 24))
                    .WithDescription($"We have pardoned {count?.ToString() ?? "all"} warnings for {user.Mention}.");

                await Task.WhenAll
                (
                    Context.Interaction.RespondAsync(embed: embed.Build()),
                    RemoveWarningsModLog.SendToModLogAsync((Context.User as SocketGuildUser)!, user, count),
                    modLogsDatabase.Warnings.RemoveWarningsAsync(user, count)
                );
            }
        }
    }
}