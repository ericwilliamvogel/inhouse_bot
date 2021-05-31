using System;
using System.Collections.Generic;
using System.Text;

namespace db
{
    public class QueueData : DiscordEntity
    {
        //[Column(TypeName="datetimeoffset")]
        public DateTimeOffset _start { get; set; }

        public int _position { get; set; }
    }

    //this is for dbset, the conversion will likely return errors cuz it cant differentiate between different lists of queuedata.
    //so we have to use inheritance



}
