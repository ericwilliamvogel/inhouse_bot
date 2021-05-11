using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace bot_2
{


 
        public struct ConfigJson
        {
            [JsonProperty("token")]
            public string Token { get; private set; }

            [JsonProperty("prefix")]
            public string Prefix { get; private set; }
        }
    
        public struct ChannelConfigJson
    {
        [JsonProperty("channel")]
        public ulong Channel { get; private set; }

        [JsonProperty("message")]
        public ulong Message { get; private set; }
    }
}
