using System;
using System.Collections.Generic;
using System.Text;

namespace db
{
    public class GameData : Entity //autoincrement id
    {
        public ulong _host { get; set; }

        public long _steamid { get; set; }
        public int _winner { get; set; }

        public DateTimeOffset _start { get; set; }

    }

}
