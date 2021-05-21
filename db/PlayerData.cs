using System;
using System.Collections.Generic;
using System.Text;

namespace db
{
    public enum Region
    {
        NONE = 0,
        USEAST = 1,
        USWEST = 2
    }
    public class PlayerData : DiscordEntity
    {
        public long _steamid { get; set; }
        //public int _ihlid { get; set; }

        public int _gameswon { get; set; }

        public int _gameslost { get; set; }

        public int _ihlmmr { get; set; }

        public int _dotammr { get; set; }

        public int _adjmmr { get; set; }

        public int _status { get; set; }

        public int _gamestatus { get; set; }//enum

        public int _role1 { get; set; }

        public int _role2 { get; set; }

        public int _region { get; set; }
        //public string _discorduser { get; set; }
    }

}
