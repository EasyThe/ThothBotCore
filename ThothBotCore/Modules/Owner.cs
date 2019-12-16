using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Models;
using ThothBotCore.Storage;
using ThothBotCore.Storage.Models;
using ThothBotCore.Utilities;

namespace ThothBotCore.Modules
{
    public class Owner : ModuleBase<SocketCommandContext>
    {
        HiRezAPI hirezAPI = new HiRezAPI();
        readonly DominantColor domColor = new DominantColor();

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
                if (splitParams[i].Contains("pro"))
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
            }
        }

        [Command("setstreamer")]
        [RequireOwner]
        public async Task SetStreamerInDb([Remainder]string parameters)
        {
            string[] splitParams = parameters.Split(" ");
            List<PlayerIDbyName> playerID = JsonConvert.DeserializeObject<List<PlayerIDbyName>>(await hirezAPI.GetPlayerIdByName(splitParams[1]));
            if (splitParams[0].Contains("add") || splitParams[0].Contains("update"))
            {
                await Database.SetPlayerSpecials(playerID[0].player_id, splitParams[1], null, 1, splitParams[2]);
            }
            else
            {
                await Database.SetPlayerSpecials(playerID[0].player_id, splitParams[1], null, 0);
            }

            var playerspecs = await Database.GetPlayerSpecialsByPlayerID(playerID[0].player_id.ToString());
            var embed = new EmbedBuilder();
            bool b = Convert.ToBoolean(playerspecs[0].streamer_bool);
            embed.WithColor(Constants.DefaultBlueColor);
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Name";
                x.Value = playerspecs[0].Name;
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Streamer";
                x.Value = Text.ToTitleCase(b.ToString());
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Streamer Link";
                x.Value = playerspecs[0].streamer_link == "" || playerspecs[0].streamer_link == null ? "n/a" : playerspecs[0].streamer_link;
            });
            await ReplyAsync("", false, embed.Build());
        }

        [Command("setspec")]
        [RequireOwner]
        public async Task SetSpecial([Remainder] string parameters)
        {

        }

        [Command("insertallguilds")]
        [RequireOwner]
        public async Task DoGuilds()
        {
            foreach (var guild in Discord.Connection.Client.Guilds)
            {
                await Database.SetGuild(guild.Id, guild.Name);
            }
            await ReplyAsync("Done!");
        }

        [Command("deleteserverfromdb")]
        [RequireOwner]
        public async Task DeleteGuildFromDB(ulong id)
        {
            await Database.DeleteServerConfig(id);

            await ReplyAsync("Should be done :shrug:");
        }

        [Command("updatedb", true, RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task UpdateDBFromSmiteAPI()
        {
            List<Gods.God> gods = JsonConvert.DeserializeObject<List<Gods.God>>(await hirezAPI.GetGods());
            await Database.SaveGods(gods);
            string newjson = JsonConvert.SerializeObject(gods, Formatting.Indented);
            domColor.DoAllGodColors();
            await ReplyAsync($"{gods.Count} Gods were found and saved to the DB.");

            try
            {
                List<GetItems.Item> itemsList = JsonConvert.DeserializeObject<List<GetItems.Item>>(await hirezAPI.GetItems());
                await Database.InsertItems(itemsList);
                domColor.DoAllItemColors();

                await ReplyAsync($"{itemsList.Count} Items were found and saved to the DB.");
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("ae")]
        [RequireOwner]
        public async Task AddEmojiToGodCommand(string emoji, [Remainder]string godname)
        {
            await Database.InsertEmojiForGod(godname, emoji);
        }

        [Command("aei")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddEmojiToItemCommand(string emoji, [Remainder]string itemName)
        {
            if (itemName.Contains("'"))
            {
                itemName = itemName.Replace("'", "''");
            }
            await Database.InsertEmojiForItem(itemName, emoji);
        }

        [Command("lg")] // Leave Guild
        [RequireOwner]
        public async Task LeaveGuild(ulong id)
        {
            await Discord.Connection.Client.GetGuild(id).LeaveAsync();
        }

        [Command("getjson")]
        [RequireOwner]
        public async Task GetPlayerOwnerCommand(string username)
        {
            string result = "";
            try
            {
                result = await hirezAPI.GetPlayer(username);
                await ReplyAsync(result);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("2000"))
                {
                    await File.WriteAllTextAsync("testmethod.json", result);
                    await ReplyAsync("Saved as testmethod.json");
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

    }
}
