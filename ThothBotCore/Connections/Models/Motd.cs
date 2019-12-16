using System;
using System.Collections.Generic;

namespace ThothBotCore.Connections.Models
{
    public class Motd
    {
        public string description { get; set; }
        public string gameMode { get; set; }
        public string maxPlayers { get; set; }
        public string name { get; set; }
        public object ret_msg { get; set; }
        public DateTime startDateTime { get; set; }
        public string team1GodsCSV { get; set; }
        public string team2GodsCSV { get; set; }
        public string title { get; set; }
    }
}
