using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Arrest : ModuleBase<SocketCommandContext>
    {
        [Command("arrest")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireOwner()]
        public async Task ArrestAsync(SocketGuildUser user, string timeout = "")
        {
            SocketRole role = await GetPrisonerRoleAsync(Context.Guild) ?? await CreatePrisonerRoleAsync();
            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is already held captive.");
            }

            SocketTextChannel channel = Context.Guild.TextChannels.FirstOrDefault(x => x.Name == "guantanamo") ?? await CreateGuantanamoAsync(role);

            List<SocketRole> roles = user.Roles.ToList();
            roles.Remove(Context.Guild.EveryoneRole);
            await Mute.SaveUserRolesAsync(roles, user);
            await user.RemoveRolesAsync(roles);
            await user.AddRoleAsync(role);

            await Context.Channel.SendMessageAsync($"{user.Mention} has been sent to Guantanamo Bay{(timeout.Length > 0 ? $" for {timeout} minutes" : "")}.");

            if (timeout != null && double.TryParse(timeout, out double minutes))
            {
                await Task.Delay((int)(minutes * 60 * 1000));

                if (!user.Roles.Contains(role))
                {
                    return;
                }

                await user.AddRolesAsync(roles);
                await user.RemoveRoleAsync(role);
                await Unmute.RemoveUserRolesAsync(user);

                await Context.Channel.SendMessageAsync($"{user.Mention} has been freed from Guantanamo Bay after a good amount of ~~torture~~ re-education.");
            }
        }

        [Command("arrest")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync(string user, string timeout = "")
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
            SocketTextChannel channel;
            ulong channelID = (await Context.Guild.CreateTextChannelAsync("guantanamo")).Id;
            channel = Context.Guild.GetTextChannel(channelID);

            OverwritePermissions perms = new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, readMessageHistory: PermValue.Deny, addReactions: PermValue.Allow);
            await channel.AddPermissionOverwriteAsync(role, perms);

            await channel.AddPermissionOverwriteAsync(Context.Client.CurrentUser, OverwritePermissions.AllowAll(channel));
            await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, OverwritePermissions.DenyAll(channel));

            return await Task.Run(() => channel);
        }

        public static async Task<SocketRole> GetPrisonerRoleAsync(SocketGuild g)
        {
            SocketRole role = null;

            string getRole = "SELECT role_id FROM Prisoner WHERE guild_id = @guild_id;";
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

        async Task SetPrisonerRoleAsync(SocketRole role)
        {
            string insert = "INSERT INTO Prisoner (guild_id, role_id) SELECT @guild_id, @role_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM Prisoner WHERE guild_id = @guild_id AND role_id = @role_id);";
            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", Context.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
