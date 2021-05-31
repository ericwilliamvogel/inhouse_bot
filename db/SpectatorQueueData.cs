using System;
using System.Collections.Generic;
using System.Text;

namespace db
{
    public class SpectatorQueueData : DiscordEntity
    {
        public DateTimeOffset _start { get; set; }

        public int _position { get; set; }
    }

}
