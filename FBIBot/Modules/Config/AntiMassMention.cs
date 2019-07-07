using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class AntiMassMention : ModuleBase<SocketCommandContext>
    {
        [Command("anti-massmention")]
        [Alias("antimassmention")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiMassMentionAsync(string enable)
        {
            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await GetAntiMassMentionAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-mass-mention is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            if (isEnable)
            {
                await SetAntiMassMentionAsync(Context.Guild);
            }
            else
            {
                await RemoveAntiMassMentionAsync(Context.Guild);
            }

            await Context.Channel.SendMessageAsync($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} messages mentioning everyone they don't like.");
        }

        public static async Task<bool> GetAntiMassMentionAsync(SocketGuild g)
        {
            bool isAntiMassMention = false;

            string getSpam = "SELECT guild_id FROM AntiMassMention WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getSpam, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                isAntiMassMention = reader.Read();
                reader.Close();
            }

            return await Task.Run(() => isAntiMassMention);
        }

        public static async Task SetAntiMassMentionAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiMassMention (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiMassMention WHERE guild_id = @guild_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveAntiMassMentionAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiMassMention WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
