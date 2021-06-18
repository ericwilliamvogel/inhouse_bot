using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace bot_2.Commands
{
    public class QOL
    {

        public async Task SendMessage(CommandContext context, ulong id, string message)
        {
            if (ChannelExists(context, id))
            {
                var channel = context.Guild.Channels[id];
                await channel.SendMessageAsync(message);
            }
        }

        public bool ChannelExists(CommandContext context, ulong id)
        {
            if (context.Guild.Channels.ContainsKey(id))
            {
                return true;
            }
            return false;
        }

        public bool ChannelExists(MessageCreateEventArgs e, ulong id)
        {
            if (e.Guild.Channels.ContainsKey(id))
            {
                return true;
            }
            return false;
        }

        public bool ChannelsExist(MessageCreateEventArgs e, List<ulong> ids)
        {
            foreach (ulong id in ids)
            {
                if (!ChannelExists(e, id))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
