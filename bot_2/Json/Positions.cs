using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace bot_2
{

    public class GameChoice
    {
        [JsonProperty("Desc")]
        public string Desc { get; set; }

        [JsonProperty("Game")]
        public int Game { get; set; }
    }
    public class Admins
    {
        [JsonProperty("Admin")]
        public ulong Admin { get; set; }
    }
    public class Positions
    {
        [JsonProperty("Pos1")]
        public string Pos1 { get; set; }

        [JsonProperty("Pos2")]
        public string Pos2 { get; set; }

        [JsonProperty("Pos3")]
        public string Pos3 { get; set; }

        [JsonProperty("Pos4")]
        public string Pos4 { get; set; }

        [JsonProperty("Pos5")]
        public string Pos5 { get; set; }
    }
}
