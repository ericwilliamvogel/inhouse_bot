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

        public async Task SendMessage(CustomContext context, string channelname, string message)
        {
            if (await Bot._validator.Exists(context.Guild, channelname))
            {
                var channel = await Bot._validator.Get(context.Guild, channelname);
                await channel.SendMessageAsync(message);
            }


        }

        public async Task SendMessage(CommandContext context, string channelname, string message)
        {
            if (await Bot._validator.Exists(context, channelname))
            {
                var channel = await Bot._validator.Get(context, channelname);
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
