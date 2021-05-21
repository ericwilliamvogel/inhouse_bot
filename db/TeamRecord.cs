using System;
using System.Collections.Generic;
using System.Text;

namespace db
{
    public enum Side
    {
        Radiant = 0,
        Dire = 1,
        Draw = 2
    }
    public class TeamRecord : Entity
    {
        public int _side { get; set; }

        public int _gameid { get; set; }

        public int _onwin { get; set; }

        public int _canpick { get; set; }
        public int _onlose { get; set; }

        public ulong _p1 { get; set; }

        public ulong _p2 { get; set; }

        public ulong _p3 { get; set; }

        public ulong _p4 { get; set; }

        public ulong _p5 { get; set; }
    }
}
