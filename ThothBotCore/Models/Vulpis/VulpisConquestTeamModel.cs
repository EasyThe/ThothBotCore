﻿using System.Collections.Generic;

namespace ThothBotCore.Models.Vulpis
{
    public class VulpisConquestTeamModel
    {
        public bool HasPro { get; set; } = false;
        public string Solo { get; set; }
        public string Jungle { get; set; }
        public string Mid { get; set; }
        public string Support { get; set; }
        public string ADC { get; set; }
    }
}
