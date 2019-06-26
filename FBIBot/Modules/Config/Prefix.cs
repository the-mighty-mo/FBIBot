using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class Prefix : ModuleBase<SocketCommandContext>
    {
        [Command("setprefix")]
        [RequireOwner()]
        public async Task SetPrefixAsync(string prefix = CommandHandler.prefix)
        {
            if (await GetPrefixAsync(Context.Guild) == prefix)
            {
                await Context.Channel.SendMessageAsync($"The FBI's prefix is already {(prefix == @"\" ? @"\\" : prefix)}.");
                return;
            }

            await SavePrefixAsync(prefix);
            await Context.Channel.SendMessageAsync($"The FBI's prefix has been set to {(prefix == @"\" ? @"\\" : prefix)}.");
        }

        async Task SavePrefixAsync(string prefix)
        {
            string createView = "CREATE VIEW IF NOT EXISTS prefixupdate AS SELECT guild_id, prefix FROM Prefixes;";
            string createTrigger = "CREATE TRIGGER IF NOT EXISTS updateprefix INSTEAD OF INSERT ON prefixupdate\n" +
                "BEGIN\n" +
                "UPDATE Prefixes SET prefix = NEW.prefix WHERE guild_id = NEW.guild_id;\n" +
                "INSERT INTO Prefixes (guild_id, prefix) SELECT NEW.guild_id, NEW.prefix WHERE (Select Changes() = 0);\n" +
                "END;";
            string insert = "INSERT INTO prefixupdate (guild_id, prefix) VALUES (@guild_id, @prefix);";
            string drop = "DROP TRIGGER updateprefix; DROP VIEW prefixupdate;";

            using (SqliteCommand cmd = new SqliteCommand(createView + createTrigger + insert + drop, Program.cnPrefix))
            {
                cmd.Parameters.AddWithValue("@guild_id", Context.Guild.Id);
                cmd.Parameters.AddWithValue("@prefix", prefix);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<string> GetPrefixAsync(SocketGuild g)
        {
            string prefix = CommandHandler.prefix;

            string getPrefix = "SELECT prefix FROM Prefixes WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getPrefix, Program.cnPrefix))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id);

                SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    prefix = (string)reader["prefix"];
                }
                reader.Close();
            }

            return await Task.Run(() => prefix);
        }
    }
}
