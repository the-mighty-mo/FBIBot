using System.Threading.Tasks;

namespace FBIBot.Databases
{
    interface ITable
    {
        public Task InitAsync();
    }
}
