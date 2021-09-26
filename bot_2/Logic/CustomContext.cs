using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;

namespace bot_2
{
    public class CustomContext
    {
        public CustomContext(CommandContext context)
        {
            this.Guild = context.Guild;
            this.Message = context.Message;
            this.Channel = context.Channel;
            this.Member = context.Member;
        }
        public CustomContext(MessageReactionAddEventArgs context)
        {
            this.Guild = context.Guild;
            this.Message = context.Message;
            this.Channel = context.Channel;
            this.Member = (DiscordMember)context.User;
        }

        public DiscordMember Member;
        public DiscordGuild Guild;
        public DiscordMessage Message;
        public DiscordChannel Channel;
    }
}
