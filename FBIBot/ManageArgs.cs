using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBIBot
{
    public class ManageArgs
    {
        public static void RemoveBotID(ref string[] args)
        {
            List<string> largs = args.ToList();
            largs.RemoveAll(x => x == SecurityInfo.botID);
            args = largs.ToArray();
        }
    }
}
