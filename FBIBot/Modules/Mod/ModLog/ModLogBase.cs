using Discord;
using Discord.WebSocket;
using FBIBot.Modules.Config;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class ModLogBase
    {
        public struct ModLogInfo
        {
            public struct RequiredInfo
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

            public struct OptionalInfo
            {
                public readonly string reason;
                public readonly string timeout;

                public OptionalInfo(string reason = null, string timeout = null)
                {
                    this.reason = reason;
                    this.timeout = timeout;
                }
            }

            public readonly SocketGuildUser Invoker { get; }
            public readonly Color Color { get; }
            public readonly string CommandName { get; }
            public readonly string CommandValue { get; }

            public readonly bool HasReasonField { get; }

            public readonly string Reason { get; }
            public readonly string Timeout { get; }

            public ModLogInfo(RequiredInfo info, bool hasReasonField = false, OptionalInfo optional = default)
            {
                Invoker = info.invoker;
                Color = info.color;
                CommandName = info.commandName;
                CommandValue = info.commandValue;

                HasReasonField = hasReasonField;

                Reason = optional.reason;
                Timeout = optional.timeout;
            }
        }

        public static async Task SendToModLogAsync(ModLogInfo info)
        {
            ulong id = await ModLogManager.GetNextModLogID(info.Invoker.Guild);
            SocketTextChannel channel = await SetModLog.GetModLogChannelAsync(info.Invoker.Guild);

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

            var msg = await channel.SendMessageAsync("", false, embed.Build());
            if (msg != null)
            {
                await ModLogManager.SaveModLogAsync(msg, info.Invoker.Guild, id);
            }
        }
    }
}
