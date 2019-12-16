using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThothBotCore.Tournament
{
    public class TournamentUtilities
    {
        public static string GetTournamentFileName(string name)
        {
            string[] nz = Directory.GetFiles("Tourneys");
            var list = nz.ToList<string>();
            string result = list.FindLast(x => x.Contains(name));
            return list.FindLast(x => x.Contains(name));
        }
        public async Task CreateChallongeTournament()
        {

        }
    }
}
