using FBIBot.Databases;
using System.Threading.Tasks;

namespace FBIBot
{
    public static class DatabaseManager
    {
        public static readonly VerificationDatabase verificationDatabase = new();
        public static readonly ModRolesDatabase modRolesDatabase = new();
        public static readonly ModLogsDatabase modLogsDatabase = new();
        public static readonly ConfigDatabase configDatabase = new();
        public static readonly RaidModeDatabase raidModeDatabase = new();

        public static Task InitAsync() =>
            Task.WhenAll(
                verificationDatabase.InitAsync(),
                modRolesDatabase.InitAsync(),
                modLogsDatabase.InitAsync(),
                configDatabase.InitAsync(),
                raidModeDatabase.InitAsync()
            );

        public static Task CloseAsync() =>
            Task.WhenAll(
                verificationDatabase.CloseAsync(),
                modRolesDatabase.CloseAsync(),
                modLogsDatabase.CloseAsync(),
                configDatabase.CloseAsync(),
                raidModeDatabase.CloseAsync()
            );
    }
}