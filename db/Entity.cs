using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace db
{
    public abstract class Entity //autoincrement id, int cannot be used w/ steamid primary keys
    {
        [Key]
        public int _id { get; set; } //eh
    }

    public abstract class SteamEntity
    {
        [Key]
        public ulong _id { get; set; } //steam id
    }

    public abstract class DiscordEntity
    {
        [Key]
        public ulong _id { get; set; } //discord username
    }
}
