﻿using FBIBot.Databases.ConfigDatabaseTables;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Databases
{
    public class ConfigDatabase
    {
        private readonly SqliteConnection connection = new SqliteConnection("Filename=Config.db");
        private readonly Dictionary<System.Type, ITable> tables = new Dictionary<System.Type, ITable>();

        public PrefixesTable Prefixes => tables[typeof(PrefixesTable)] as PrefixesTable;
        public ModifyMutedTable ModifyMuted => tables[typeof(ModifyMutedTable)] as ModifyMutedTable;
        public AutoSurveillanceTable AutoSurveillance => tables[typeof(AutoSurveillanceTable)] as AutoSurveillanceTable;
        public AntiZalgoTable AntiZalgo => tables[typeof(AntiZalgoTable)] as AntiZalgoTable;
        public AntiSpamTable AntiSpam => tables[typeof(AntiSpamTable)] as AntiSpamTable;
        public AntiSingleSpamTable AntiSingleSpam => tables[typeof(AntiSingleSpamTable)] as AntiSingleSpamTable;
        public AntiMassMentionTable AntiMassMention => tables[typeof(AntiMassMentionTable)] as AntiMassMentionTable;
        public AntiCapsTable AntiCaps => tables[typeof(AntiCapsTable)] as AntiCapsTable;
        public AntiInviteTable AntiInvite => tables[typeof(AntiInviteTable)] as AntiInviteTable;
        public AntiLinkTable AntiLink => tables[typeof(AntiLinkTable)] as AntiLinkTable;

        public ConfigDatabase()
        {
            tables.Add(typeof(PrefixesTable), new PrefixesTable(connection));
            tables.Add(typeof(ModifyMutedTable), new ModifyMutedTable(connection));
            tables.Add(typeof(AutoSurveillanceTable), new AutoSurveillanceTable(connection));
            tables.Add(typeof(AntiZalgoTable), new AntiZalgoTable(connection));
            tables.Add(typeof(AntiSpamTable), new AntiSpamTable(connection));
            tables.Add(typeof(AntiSingleSpamTable), new AntiSingleSpamTable(connection));
            tables.Add(typeof(AntiMassMentionTable), new AntiMassMentionTable(connection));
            tables.Add(typeof(AntiCapsTable), new AntiCapsTable(connection));
            tables.Add(typeof(AntiInviteTable), new AntiInviteTable(connection));
            tables.Add(typeof(AntiLinkTable), new AntiLinkTable(connection));
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