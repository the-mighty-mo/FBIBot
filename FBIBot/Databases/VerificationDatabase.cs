using FBIBot.Databases.VerificationDatabaseTables;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Databases
{
    public class VerificationDatabase
    {
        private readonly SqliteConnection connection = new("Filename=Verification.db");
        private readonly Dictionary<System.Type, ITable> tables = new();

        public CaptchaTable Captcha => tables[typeof(CaptchaTable)] as CaptchaTable;
        public VerifiedTable Verified => tables[typeof(VerifiedTable)] as VerifiedTable;
        public AttemptsTable Attempts => tables[typeof(AttemptsTable)] as AttemptsTable;
        public RolesTable Roles => tables[typeof(RolesTable)] as RolesTable;

        public VerificationDatabase()
        {
            tables.Add(typeof(CaptchaTable), new CaptchaTable(connection));
            tables.Add(typeof(VerifiedTable), new VerifiedTable(connection));
            tables.Add(typeof(AttemptsTable), new AttemptsTable(connection));
            tables.Add(typeof(RolesTable), new RolesTable(connection));
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