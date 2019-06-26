﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class ModifyMutedRoles : ModuleBase<SocketCommandContext>
    {
        [Command("modifymutedroles")]
        [RequireOwner()]
        public async Task ModifyMutedRolesAsync(string modify)
        {
            bool isModify = modify == "true" || modify == "enable";
            string state = isModify ? "permitted to" : "prohibited from";

            if (isModify && !await GetModifyMutedAsync(Context.Guild))
            {
                await AddModifyMutedAsync(Context.Guild);
            }
            else if (!isModify && await GetModifyMutedAsync(Context.Guild))
            {
                await RemoveModifyMutedAsync(Context.Guild);
            }
            else
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that we are already {state} modifying muted member's roles.");
            }

            await Context.Channel.SendMessageAsync($"We are now {state} modifying muted member's roles.");
        }

        public static async Task AddModifyMutedAsync(SocketGuild g)
        {
            string add = "INSERT INTO ModifyMuted (guild_id) SELECT @guild_id WHERE NOT EXISTS (SELECT 1 FROM ModifyMuted WHERE guild_id = @guild_id);";
            using (SqliteCommand cmd = new SqliteCommand(add, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveModifyMutedAsync(SocketGuild g)
        {
            string add = "DELETE FROM ModifyMuted WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(add, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<bool> GetModifyMutedAsync(SocketGuild g)
        {
            bool modify = false;

            string add = "SELECT * FROM ModifyMuted WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(add, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                modify = reader.Read();
                reader.Close();
            }

            return await Task.Run(() => modify);
        }
    }
}