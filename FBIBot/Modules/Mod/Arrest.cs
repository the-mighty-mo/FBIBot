using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Arrest : ModuleBase<SocketCommandContext>
    {
        [Command("arrest")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ArrestAsync(SocketGuildUser user, string timeout = null)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsMod(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            SocketRole role = await GetPrisonerRoleAsync(Context.Guild) ?? await CreatePrisonerRoleAsync();
            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is already held captive.");
            }

            SocketTextChannel channel = await GetPrisonerChannelAsync(Context.Guild) ?? await CreateGuantanamoAsync(role);

            List<SocketRole> roles = user.Roles.ToList();
            roles.Remove(Context.Guild.EveryoneRole);
            await Mute.SaveUserRolesAsync(roles, user);
            try
            {
                await user.RemoveRolesAsync(roles);
            }
            catch
            {
                try
                {
                    await user.AddRolesAsync(roles);
                }
                catch { };
                await Context.Channel.SendMessageAsync("We cannot arrest members with higher authority than ourselves.");
                await Unmute.RemoveUserRolesAsync(user);

                if (!await Free.HasPrisoners(Context.Guild))
                {
                    await channel?.DeleteAsync();
                    await role?.DeleteAsync();
                    await Free.RemovePrisonerChannelAsync(Context.Guild);
                    await Free.RemovePrisonerRoleAsync(Context.Guild);
                }
                return;
            }
            await user.AddRoleAsync(role);

            await RecordPrisonerAsync(user);

            bool isTimeout = double.TryParse(timeout, out double minutes);

            await Context.Channel.SendMessageAsync($"{user.Mention} has been sent to Guantanamo Bay{(timeout != null && isTimeout ? $" for {timeout} {(minutes == 1 ? "minute" : "minutes")}" : "")}.");
            await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Arrest, Context.User, user, timeout);

            if (timeout != null && isTimeout)
            {
                await Task.Delay((int)(minutes * 60 * 1000));

                if (!user.Roles.Contains(role))
                {
                    return;
                }

                await user.AddRolesAsync(roles);
                await user.RemoveRoleAsync(role);
                await Unmute.RemoveUserRolesAsync(user);
                await Free.RemovePrisonerAsync(user);

                if (!await Free.HasPrisoners(Context.Guild))
                {
                    await channel?.DeleteAsync();
                    await role?.DeleteAsync();
                    await Free.RemovePrisonerChannelAsync(Context.Guild);
                    await Free.RemovePrisonerRoleAsync(Context.Guild);
                }

                await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Free, Context.Client.CurrentUser, user);
            }
        }

        [Command("arrest")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ArrestAsync(string user, string timeout = null)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await ArrestAsync(u, timeout);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }

        async Task<SocketRole> CreatePrisonerRoleAsync()
        {
            SocketRole role;
            GuildPermissions perms = new GuildPermissions(viewChannel: false);
            Color color = new Color(127, 127, 127);
            ulong roleID = (await Context.Guild.CreateRoleAsync("Prisoner", perms, color)).Id;
            role = Context.Guild.GetRole(roleID);

            await SetPrisonerRoleAsync(role);
            return await Task.Run(() => role);
        }

        async Task<SocketTextChannel> CreateGuantanamoAsync(SocketRole role)
        {
            SocketTextChannel textChannel = null;
            RestTextChannel channel = await Context.Guild.CreateTextChannelAsync("guantanamo");

            OverwritePermissions perms = new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, readMessageHistory: PermValue.Deny, addReactions: PermValue.Allow);
            await channel.AddPermissionOverwriteAsync(role, perms);

            await channel.AddPermissionOverwriteAsync(Context.Client.CurrentUser, OverwritePermissions.AllowAll(channel));
            await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, OverwritePermissions.DenyAll(channel));
            await channel.UpdateAsync();

            textChannel = Context.Guild.GetTextChannel(channel.Id);

            await SetPrisonerChannelAsync(textChannel);

            return await Task.Run(() => textChannel);
        }

        public static async Task<SocketRole> GetPrisonerRoleAsync(SocketGuild g)
        {
            SocketRole role = null;

            string getRole = "SELECT role_id FROM PrisonerRole WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getRole, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ulong roleID = ulong.Parse(reader["role_id"].ToString());
                    role = g.GetRole(roleID);
                }
                reader.Close();
            }

            return await Task.Run(() => role);
        }

        public static async Task SetPrisonerRoleAsync(SocketRole role)
        {
            string update = "UPDATE PrisonerRole SET role_id = @role_id WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO PrisonerRole (guild_id, role_id) SELECT @guild_id, @role_id WHERE (Select Changes() = 0);";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<SocketTextChannel> GetPrisonerChannelAsync(SocketGuild g)
        {
            SocketTextChannel channel = null;

            string getChannel = "SELECT channel_id FROM PrisonerChannel WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getChannel, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ulong channelID = ulong.Parse(reader["channel_id"].ToString());
                    channel = g.GetTextChannel(channelID);
                }
                reader.Close();
            }

            return await Task.Run(() => channel);
        }

        public static async Task SetPrisonerChannelAsync(SocketTextChannel channel)
        {
            string update = "UPDATE PrisonerChannel SET channel_id = @channel_id WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO PrisonerChannel (guild_id, channel_id) SELECT @guild_id, @channel_id WHERE (Select Changes() = 0);";
            using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", channel.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@channel_id", channel.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RecordPrisonerAsync(SocketGuildUser user)
        {
            string insert = "INSERT INTO Prisoners (guild_id, user_id) SELECT @guild_id, @user_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM Prisoners WHERE guild_id = @guild_id AND user_id = @user_id);";
            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
