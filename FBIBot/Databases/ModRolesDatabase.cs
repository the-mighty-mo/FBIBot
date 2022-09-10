using FBIBot.Databases.ModRolesDatabaseTables;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Databases
{
    public class ModRolesDatabase
    {
        private readonly SqliteConnection connection = new("Filename=ModRoles.db");
        private readonly Dictionary<System.Type, ITable> tables = new();

        public MutedTable Muted => (tables[typeof(MutedTable)] as MutedTable)!;
        public PrisonerRoleTable PrisonerRole => (tables[typeof(PrisonerRoleTable)] as PrisonerRoleTable)!;
        public PrisonerChannelTable PrisonerChannel => (tables[typeof(PrisonerChannelTable)] as PrisonerChannelTable)!;
        public PrisonersTable Prisoners => (tables[typeof(PrisonersTable)] as PrisonersTable)!;
        public UserRolesTable UserRoles => (tables[typeof(UserRolesTable)] as UserRolesTable)!;
        public ModsTable Mods => (tables[typeof(ModsTable)] as ModsTable)!;
        public AdminsTable Admins => (tables[typeof(AdminsTable)] as AdminsTable)!;

        public ModRolesDatabase()
        {
            tables.Add(typeof(MutedTable), new MutedTable(connection));
            tables.Add(typeof(PrisonerRoleTable), new PrisonerRoleTable(connection));
            tables.Add(typeof(PrisonerChannelTable), new PrisonerChannelTable(connection));
            tables.Add(typeof(PrisonersTable), new PrisonersTable(connection));
            tables.Add(typeof(UserRolesTable), new UserRolesTable(connection));
            tables.Add(typeof(ModsTable), new ModsTable(connection));
            tables.Add(typeof(AdminsTable), new AdminsTable(connection));
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