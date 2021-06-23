using bot_2.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_2
{
    class EmojiHandler
    {
        Bot bot;

        private DiscordEmoji one;
        private DiscordEmoji two;
        private DiscordEmoji three;
        private DiscordEmoji four;
        private DiscordEmoji five;
        private DiscordEmoji yes;
        private DiscordEmoji no;
        public EmojiHandler(Bot bot)
        {
            this.bot = bot;
            SetEmojis();
            LogicDictionary();
        }

        private void SetEmojis()
        {
            one = DiscordEmoji.FromName(bot.Client, ":one:");
            two = DiscordEmoji.FromName(bot.Client, ":two:");
            three = DiscordEmoji.FromName(bot.Client, ":three:");
            four = DiscordEmoji.FromName(bot.Client, ":four:");
            five = DiscordEmoji.FromName(bot.Client, ":five:");
            yes = DiscordEmoji.FromName(bot.Client, ":thumbsup:");
            no = DiscordEmoji.FromName(bot.Client, ":thumbsdown:");


        }

        public Dictionary<ulong, React> _reactLogic = new Dictionary<ulong, React>();
        public delegate Task React(MessageReactionAddEventArgs args);
        private void LogicDictionary()
        {


            _reactLogic.Add(bot.ReactMessages.Positions, async (MessageReactionAddEventArgs args) =>
            {
                var member = (DiscordMember)args.User;
                Dictionary<DiscordEmoji, DiscordRole> roles = new Dictionary<DiscordEmoji, DiscordRole>();

                roles.Add(one, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Pos1").Value);
                roles.Add(two, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Pos2").Value);
                roles.Add(three, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Pos3").Value);
                roles.Add(four, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Pos4").Value);
                roles.Add(five, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Pos5").Value);

                if (roles.ContainsKey(args.Emoji))
                {
                    await member.GrantRoleAsync(roles[args.Emoji]);
                }
                else
                {
                    await args.Message.DeleteReactionAsync(args.Emoji, args.User);
                }
            });


            _reactLogic.Add(bot.ReactMessages.FavoritePosition, async (MessageReactionAddEventArgs args) =>
            {
                Profile _profile = new Profile(args.Guild, args.User);
                var allreactions = args.Message.Reactions.ToList();
                var reac = allreactions.FindAll(p => p.IsMe == true).ToList();
                
                if(reac.Count > 1)
                {
                    await args.Message.DeleteReactionAsync(args.Emoji, args.User);
                    return;
                }
                if (args.Message.Reactions.FirstOrDefault(p => p.IsMe == true) == null)
                {
                    var member = (DiscordMember)args.User;
                    Dictionary<DiscordEmoji, DiscordRole> roles = new Dictionary<DiscordEmoji, DiscordRole>();

                    roles.Add(one, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Pos1(Favorite)").Value);
                    roles.Add(two, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Pos2(Favorite)").Value);
                    roles.Add(three, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Pos3(Favorite)").Value);
                    roles.Add(four, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Pos4(Favorite)").Value);
                    roles.Add(five, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Pos5(Favorite)").Value);

                    if (roles.ContainsKey(args.Emoji))
                    {
                        await member.GrantRoleAsync(roles[args.Emoji]);
                    }
                    else
                    {
                        await args.Message.DeleteReactionAsync(args.Emoji, args.User);
                    }
                }
                


            });
        }

    }
}
