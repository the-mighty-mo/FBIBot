﻿using FBIBot.Databases.RaidModeDatabaseTables;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Databases
{
    public class RaidModeDatabase
    {
        private readonly SqliteConnection connection = new SqliteConnection("Filename=Raid-Mode.db");
        private readonly Dictionary<System.Type, ITable> tables = new Dictionary<System.Type, ITable>();

        public RaidModeTable RaidMode => tables[typeof(RaidModeTable)] as RaidModeTable;
        public UsersBlockedTable UsersBlocked => tables[typeof(UsersBlockedTable)] as UsersBlockedTable;

        public RaidModeDatabase()
        {
            tables.Add(typeof(RaidModeTable), new RaidModeTable(connection));
            tables.Add(typeof(UsersBlockedTable), new UsersBlockedTable(connection));
        }

        public async Task InitAsync()
        {
            await connection.OpenAsync();
            IEnumerable<Task> GetTableInits()
            {
                foreach (var table in tables.Values)
                {
                    yield return table.InitAsync();
                }
            }
            await Task.WhenAll(GetTableInits());
        }

        public async Task CloseAsync() => await connection.CloseAsync();
    }
}