using System;
using System.Collections.Generic;
using System.Text;

namespace db
{
    public class BetData : Entity
    {
        public int _gameid { get; set; }
        public int _side { get; set; }
        public ulong _discordid { get; set; }
        public int _amount { get; set; }
    }
}
