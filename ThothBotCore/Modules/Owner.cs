using Discord.Commands;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Storage;

namespace ThothBotCore.Modules
{
    public class Owner : ModuleBase<SocketCommandContext>
    {
        HiRezAPI hirezAPI = new HiRezAPI();

        [Command("setplayersspec")]
        [Alias("sps")]
        [RequireOwner]
        public async Task SetPlayersSpecial(string username, [Remainder]string parameters)
        {
            List<PlayerIDbyName> playerID = JsonConvert.DeserializeObject<List<PlayerIDbyName>>(await hirezAPI.GetPlayerIdByName(username));
            string[] splitParams = parameters.Split(" ");
            for (int i = 0; i < splitParams.Length; i++)
            {
                if (splitParams[i].Contains("discord")) //discord
                {
                    if (splitParams[i + 1].Contains("link"))
                    {
                        await Database.SetPlayerSpecials(playerID[0].player_id, username, Context.User.Id);
                    }
                    else
                    {
                        await Database.SetPlayerSpecials(playerID[0].player_id, username, 0);
                    }
                }
                if (splitParams[i].Contains("streamer"))// streamer
                {
                    if (splitParams[i + 1].Contains("yes"))
                    {
                        await Database.SetPlayerSpecials(playerID[0].player_id, username, null, 1);
                    }
                    else
                    {
                        await Database.SetPlayerSpecials(playerID[0].player_id, username, null, 0);
                    }
                }
                if (splitParams[i].Contains("pro"))
                {
                    if (splitParams[i + 1].Contains("yes"))
                    {
                        await Database.SetPlayerSpecials(playerID[0].player_id, username, null, null, 1);
                    }
                    else
                    {
                        await Database.SetPlayerSpecials(playerID[0].player_id, username, null, null, 0);
                    }
                }
                if (splitParams[i].Contains("spec"))
                {
                    if (splitParams[i + 1].Contains("add"))
                    {
                        await Database.SetPlayerSpecials(playerID[0].player_id, username, null, null, null, splitParams[i + 2]);
                    }
                }
            }
        }

        public class PlayerIDbyName
        {
            public int player_id { get; set; }
            public string portal { get; set; }
            public int portal_id { get; set; }
            public object ret_msg { get; set; }
        }
    }
}
