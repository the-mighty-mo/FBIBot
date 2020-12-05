using FBIBot.Databases;
using System.Threading.Tasks;

namespace FBIBot
{
    public static class DatabaseManager
    {
        public static readonly VerificationDatabase verificationDatabase = new VerificationDatabase();
        public static readonly ModRolesDatabase modRolesDatabase = new ModRolesDatabase();
        public static readonly ModLogsDatabase modLogsDatabase = new ModLogsDatabase();
        public static readonly ConfigDatabase configDatabase = new ConfigDatabase();
        public static readonly RaidModeDatabase raidModeDatabase = new RaidModeDatabase();

        public static async Task InitAsync()
        {
            await Task.WhenAll(
                verificationDatabase.InitAsync(),
                modRolesDatabase.InitAsync(),
                modLogsDatabase.InitAsync(),
                configDatabase.InitAsync(),
                raidModeDatabase.InitAsync()
            );
        }

        public static async Task CloseAsync()
        {
            await Task.WhenAll(
                verificationDatabase.CloseAsync(),
                modRolesDatabase.CloseAsync(),
                modLogsDatabase.CloseAsync(),
                configDatabase.CloseAsync(),
                raidModeDatabase.CloseAsync()
            );
        }
    }
}