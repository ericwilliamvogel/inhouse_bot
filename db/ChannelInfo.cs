using System;
using System.Collections.Generic;
using System.Text;

namespace db
{
    public class ChannelInfo : DiscordEntity
    {
        public int _number { get; set; }

        public int _gameid { get; set; }

        public ulong _messageid { get; set; }
    }
}
