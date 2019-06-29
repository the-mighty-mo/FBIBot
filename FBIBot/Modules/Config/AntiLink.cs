using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class AntiLink : ModuleBase<SocketCommandContext>
    {
        [Command("antilink")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiLinkAsync(string enable)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsAdmin(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await GetAntiLinkAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-link is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            if (isEnable)
            {
                await SetAntiLinkAsync(Context.Guild);
            }
            else
            {
                await RemoveAntiLinkAsync(Context.Guild);
            }

            await Context.Channel.SendMessageAsync($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} messages linking external, communist media.");
        }

        public static async Task<bool> GetAntiLinkAsync(SocketGuild g)
        {
            bool isAntiLink = false;

            string getSpam = "SELECT guild_id FROM AntiLink WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getSpam, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                isAntiLink = reader.Read();
                reader.Close();
            }

            return await Task.Run(() => isAntiLink);
        }

        public static async Task SetAntiLinkAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiLink (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiLink WHERE guild_id = @guild_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveAntiLinkAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiLink WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
