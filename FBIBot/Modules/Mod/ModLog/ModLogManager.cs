using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class ModLogManager
    {
        public class StateInfo
        {
            public SocketGuild Guild { get; }
            public ulong LogId { get; }
            public string StateName { get; }
            public string StateValue { get; }

            public StateInfo(SocketGuild guild, ulong logId, string stateName, string stateValue)
            {
                Guild = guild;
                LogId = logId;
                StateName = stateName;
                StateValue = stateValue;
            }
        }

        public class ReasonInfo : StateInfo
        {
            public ReasonInfo(SocketGuild guild, ulong logId, string reason) : base(guild, logId, "Reason", reason ?? "(none given)") { }
        }

        public static async Task<bool> SetReasonAsync(ReasonInfo info)
            => await SetStateAsync(info);

        public static async Task<bool> SetStateAsync(StateInfo info)
        {
            IUserMessage msg = await modLogsDatabase.ModLogs.GetModLogAsync(info.Guild, info.LogId);
            if (msg == null || msg.Embeds.Count == 0)
            {
                return false;
            }
            EmbedBuilder e = msg.Embeds.ToList()[0].ToEmbedBuilder();
            List<EmbedFieldBuilder> fields = e.Fields.ToList();

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(e.Color ?? SecurityInfo.botColor)
                .WithTitle(e.Title)
                .WithCurrentTimestamp();

            EmbedFieldBuilder field = e.Fields.FirstOrDefault(x => x.Name.Contains(info.StateName));
            if (field == null)
            {
                return false;
            }
            int index = fields.IndexOf(field);
            fields.Remove(field);

            field.WithValue(info.StateValue);
            fields.Insert(index, field);
            embed.WithFields(fields);

            await msg.ModifyAsync(x => x.Embed = embed.Build());
            return true;
        }
    }
}