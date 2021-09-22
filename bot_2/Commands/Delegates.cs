using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace bot_2.Commands
{
    public delegate Task<bool> Argument(CommandContext context, Profile profile);


    public enum Region
    {
        NONE = 0,
        USEAST = 1,
        USWEST = 2
    }

    public enum Side
    {
        Team1 = 0,
        Team2 = 1,
        Draw = 2
    }
}
