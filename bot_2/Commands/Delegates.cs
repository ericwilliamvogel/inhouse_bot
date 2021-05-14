using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace bot_2.Commands
{
    public delegate Task<bool> Argument(CommandContext context, Profile profile);
}
