using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Free : ModuleBase<SocketCommandContext>
    {
        [Command("free")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task FreeAsync([RequireInvokerHierarchy("free")] SocketGuildUser user)
        {
            SocketRole role = await Arrest.GetPrisonerRoleAsync(Context.Guild);
            List<SocketRole> roles = await Unmute.GetUserRolesAsync(user);
            if ((role == null || !user.Roles.Contains(role)) && roles.Count == 0)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is not held captive.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(12, 156, 24))
                .WithDescription($"{user.Mention} has been freed from Guantanamo Bay after a good amount of ~~torture~~ re-education.");

            List<Task> cmds = new List<Task>()
            {
                RemovePrisonerAsync(user),
                Context.Channel.SendMessageAsync("", false, embed.Build()),
                FreeModLog.SendToModLogAsync(Context.User as SocketGuildUser, user)
            };
            if (roles.Count > 0)
            {
                cmds.AddRange(new List<Task>()
                {
                    user.AddRolesAsync(roles),
                    Unmute.RemoveUserRolesAsync(user)
                });
            }
            if (role != null)
            {
                cmds.Add(user.RemoveRoleAsync(role));
            }
            await Task.WhenAll(cmds);

            if (!await HasPrisoners(Context.Guild))
            {
                SocketTextChannel channel = await Arrest.GetPrisonerChannelAsync(Context.Guild);

                await Task.WhenAll
                (
                    channel?.DeleteAsync(),
                    role?.DeleteAsync(),
                    RemovePrisonerChannelAsync(Context.Guild),
                    RemovePrisonerRoleAsync(Context.Guild)
                );
            }
        }

        [Command("free")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task FreeAsync(string user)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await FreeAsync(u);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }

        public static async Task RemovePrisonerRoleAsync(SocketGuild g)
        {
            string delete = "DELETE FROM PrisonerRole WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemovePrisonerChannelAsync(SocketGuild g)
        {
            string delete = "DELETE FROM PrisonerChannel WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<bool> HasPrisoners(SocketGuild g)
        {
            bool hasPrisoners = false;

            string getUsers = "SELECT * FROM Prisoners WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getUsers, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                hasPrisoners = await reader.ReadAsync();
                reader.Close();
            }

            return hasPrisoners;
        }

        public static async Task RemovePrisonerAsync(SocketGuildUser user)
        {
            string delete = "DELETE FROM Prisoners WHERE guild_id = @guild_id AND user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
