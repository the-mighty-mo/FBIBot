﻿using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class SetPrefix : ModuleBase<SocketCommandContext>
    {
        [Command("setprefix")]
        [Alias("set-prefix")]
        [RequireAdmin]
        public async Task PrefixAsync(string prefix = CommandHandler.prefix)
        {
            if (await GetPrefixAsync(Context.Guild) == prefix)
            {
                await Context.Channel.SendMessageAsync($"The FBI's prefix is already `{prefix}`.");
                return;
            }

            await SetPrefixAsync(Context.Guild, prefix);
            await Context.Channel.SendMessageAsync($"The FBI's prefix has been set to `{prefix}`.");
        }

        public static async Task<string> GetPrefixAsync(SocketGuild g)
        {
            string prefix = CommandHandler.prefix;

            string getPrefix = "SELECT prefix FROM Prefixes WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getPrefix, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    prefix = (string)reader["prefix"];
                }
                reader.Close();
            }

            return await Task.Run(() => prefix);
        }

        public static async Task SetPrefixAsync(SocketGuild g, string prefix)
        {
            string update = "UPDATE Prefixes SET prefix = @prefix WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO Prefixes (guild_id, prefix) SELECT @guild_id, @prefix WHERE (SELECT Changes() = 0);";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                cmd.Parameters.AddWithValue("@prefix", prefix);

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}