using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace db 
{
    public class EmoteUnlockedData : Entity
    {
        public ulong _playerid { get; set; }
        public int _emoteid { get; set; }


    }
}
