using System;
using System.Collections.Generic;
using System.Text;


namespace db
{

    public enum Status
    {
        Unregistered = 0, //after role assigned from emoticon
        Registered = 1, //access after initial advanced registration
        Trusted = 2, //after x amount of games played, will have ability to ticket games, create lobbies, report games
        Mod = 3, //can assign mmr adjustments, can assign trusted players
        Admin = 4, //everything
        Banned = 7
    }

    public enum Faction
    {
        WSS = 0,
        Grin = 1
            //etc
    }
    public class DataSet:Entity //testing or game
    {
        public string _name { get; set; }
        public string _desc { get; set; }
    }


    public enum Day
    {
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday
    }


    public class PlayerAvailability : DiscordEntity
    {
        public int _day { get; set; }//enum
        public int _start { get; set; }
        public int _finish { get; set; }
    }


    public class RoomAvailability : Entity //made in sql
    {
        public bool _available { get; set; }
    }
}
