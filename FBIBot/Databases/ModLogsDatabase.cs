using FBIBot.Databases.ModLogsDatabaseTables;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Databases
{
    public class ModLogsDatabase
    {
        private readonly SqliteConnection connection = new("Filename=ModLogs.db");
        private readonly Dictionary<System.Type, ITable> tables = new();

        public ModLogChannelTable ModLogChannel => tables[typeof(ModLogChannelTable)] as ModLogChannelTable;
        public CaptchaLogChannelTable CaptchaLogChannel => tables[typeof(CaptchaLogChannelTable)] as CaptchaLogChannelTable;
        public WelcomeChannelTable WelcomeChannel => tables[typeof(WelcomeChannelTable)] as WelcomeChannelTable;
        public ModLogsTable ModLogs => tables[typeof(ModLogsTable)] as ModLogsTable;
        public WarningsTable Warnings => tables[typeof(WarningsTable)] as WarningsTable;

        public ModLogsDatabase()
        {
            tables.Add(typeof(ModLogChannelTable), new ModLogChannelTable(connection));
            tables.Add(typeof(CaptchaLogChannelTable), new CaptchaLogChannelTable(connection));
            tables.Add(typeof(WelcomeChannelTable), new WelcomeChannelTable(connection));
            tables.Add(typeof(ModLogsTable), new ModLogsTable(connection));
            tables.Add(typeof(WarningsTable), new WarningsTable(connection));
        }

        public async Task InitAsync()
        {
            IEnumerable<Task> GetTableInits()
            {
                foreach (var table in tables.Values)
                {
                    yield return table.InitAsync();
                }
            }
            await connection.OpenAsync();
            await Task.WhenAll(GetTableInits());
        }

        public Task CloseAsync() => connection.CloseAsync();
    }
}