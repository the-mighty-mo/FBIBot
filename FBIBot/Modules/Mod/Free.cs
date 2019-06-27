using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Free : ModuleBase<SocketCommandContext>
    {
        [Command("free")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task FreeAsync(SocketGuildUser user)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsMod(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            SocketRole role = await Arrest.GetPrisonerRoleAsync(Context.Guild);
            List<SocketRole> roles = await Unmute.GetUserRolesAsync(user);
            if ((role == null || !user.Roles.Contains(role)) && roles.Count == 0)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is not held captive.");
                return;
            }

            if (roles.Count > 0)
            {
                await user.AddRolesAsync(roles);
                await Unmute.RemoveUserRolesAsync(user);
            }
            if (role != null)
            {
                await user.RemoveRoleAsync(role);
            }
            await RemovePrisonerAsync(user);

            await Context.Channel.SendMessageAsync($"{user.Mention} has been freed from Guantanamo Bay after a good amount of ~~torture~~ re-education.");
            await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Free, Context.User, user);

            if (!await HasPrisoners(Context.Guild))
            {
                SocketTextChannel channel = await Arrest.GetPrisonerChannelAsync(Context.Guild);

                await channel?.DeleteAsync();
                await role?.DeleteAsync();
                await RemovePrisonerChannelAsync(Context.Guild);
                await RemovePrisonerRoleAsync(Context.Guild);
            }
        }

        [Command("free")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
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

                SqliteDataReader reader = cmd.ExecuteReader();
                hasPrisoners = reader.Read();
                reader.Close();
            }

            return await Task.Run(() => hasPrisoners);
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
