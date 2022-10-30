using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class ModLogBase
    {
        public class ModLogInfo
        {
            public class RequiredInfo
            {
                public readonly SocketGuildUser invoker;
                public readonly Color color;
                public readonly string commandName;
                public readonly string commandValue;

                public RequiredInfo(SocketGuildUser invoker, Color color, string commandName, string commandValue)
                {
                    this.invoker = invoker;
                    this.color = color;
                    this.commandName = commandName;
                    this.commandValue = commandValue;
                }
            }

            public class ReasonInfo
            {
                public readonly string? reason;

                public ReasonInfo(string? reason = null) =>
                    this.reason = reason;
            }

            public SocketGuildUser Invoker { get; }
            public Color Color { get; }
            public string CommandName { get; }
            public string CommandValue { get; }

            public bool HasReasonField { get; }

            public string? Reason { get; }

            public ModLogInfo(RequiredInfo info, ReasonInfo? reasonInfo = null)
            {
                Invoker = info.invoker;
                Color = info.color;
                CommandName = info.commandName;
                CommandValue = info.commandValue;

                HasReasonField = reasonInfo != null;
                Reason = reasonInfo?.reason;
            }
        }

        public static async Task SendToModLogAsync(ModLogInfo info)
        {
            ulong id = await modLogsDatabase.ModLogs.GetNextModLogID(info.Invoker.Guild).ConfigureAwait(false);
            SocketTextChannel? channel = await modLogsDatabase.ModLogChannel.GetModLogChannelAsync(info.Invoker.Guild).ConfigureAwait(false);

            if (channel == null)
            {
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(info.Color)
                .WithTitle($"Federal Bureau of Investigation - Log {id}")
                .WithCurrentTimestamp();

            EmbedFieldBuilder command = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName(info.CommandName)
                .WithValue(info.CommandValue);
            embed.AddField(command);

            EmbedFieldBuilder invoked = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Invoked by")
                .WithValue(info.Invoker.Mention);
            embed.AddField(invoked);

            if (info.HasReasonField)
            {
                EmbedFieldBuilder field = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue(info.Reason ?? "(none given)");
                embed.AddField(field);
            }

            var msg = await channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            if (msg != null)
            {
                await modLogsDatabase.ModLogs.SaveModLogAsync(msg, info.Invoker.Guild, id).ConfigureAwait(false);
            }
        }
    }
}