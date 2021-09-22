using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace bot_2
{

    public class ChannelRecord
    {
        [JsonProperty("Id")]
        public ulong Id { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
    }

    public class JsonRecord
    {
        [JsonProperty("Channels")]
        public List<ChannelRecord> Channels { get; set; }
    }
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
    }

    public struct ScheduleJson
    {
        [JsonProperty("schedule")]
        public List<DayScheduleJson> Schedule { get; set; }
    }

    public struct DayScheduleJson
    {
        [JsonProperty("day")]
        public int Day { get; set; }

        [JsonProperty("starthour")]
        public int StartHour { get; set; }

        [JsonProperty("endhour")]
        public int EndHour { get; set; }

        [JsonProperty("startminute")]
        public int StartMinute { get; set; }


        [JsonProperty("endminute")]
        public int EndMinute { get; set; }
    }
}
