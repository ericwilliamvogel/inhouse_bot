using System;
using System.Collections.Generic;
using System.Text;

namespace db
{
    public class PlayerData : DiscordEntity
    {
        public long _steamid { get; set; }
        //public int _ihlid { get; set; }

        public int _totalgames { get; set; }
        public int _gameswon { get; set; }

        public int _gameslost { get; set; }

        public int _ihlmmr { get; set; }

        public int _dotammr { get; set; }

        public int _adjmmr { get; set; }

        public int _status { get; set; }

        public int _gamestatus { get; set; }//enum

        public long _xp { get; set; }
        

        //public string _discorduser { get; set; }
    }

}
