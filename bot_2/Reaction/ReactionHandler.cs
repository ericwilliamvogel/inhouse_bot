using bot_2.Commands;
using db;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace bot_2
{
    class EmojiHandler
    {
        Bot bot;

        private DiscordEmoji zero;

        private DiscordEmoji one;
        private DiscordEmoji two;
        private DiscordEmoji three;
        private DiscordEmoji four;
        private DiscordEmoji five;
        private DiscordEmoji six;
        private DiscordEmoji seven;
        private DiscordEmoji eight;
        private DiscordEmoji e;
        private DiscordEmoji w;

        private DiscordEmoji yes;
        private DiscordEmoji no;

        Context _context;

        private EmojiHandlerProfile _emojiProfile;
        public EmojiHandler(Bot bot, IServiceProvider services) 
        {
            _emojiProfile = new EmojiHandlerProfile();
            _context = (Context)services.GetService(typeof(Context));
            SetRankDictionary();
            this.bot = bot;
            SetEmojis();

            LogicDictionary();
        }

        private void SetEmojis()
        {
            zero = DiscordEmoji.FromName(bot.Client, ":zero:");
            one = DiscordEmoji.FromName(bot.Client, ":one:");
            two = DiscordEmoji.FromName(bot.Client, ":two:");
            three = DiscordEmoji.FromName(bot.Client, ":three:");
            four = DiscordEmoji.FromName(bot.Client, ":four:");
            five = DiscordEmoji.FromName(bot.Client, ":five:");
            six = DiscordEmoji.FromName(bot.Client, ":six:");
            seven = DiscordEmoji.FromName(bot.Client, ":seven:");
            eight = DiscordEmoji.FromName(bot.Client, ":eight:");

            yes = DiscordEmoji.FromName(bot.Client, ":white_check_mark:");
            no = DiscordEmoji.FromName(bot.Client, ":no_entry:");

            e = DiscordEmoji.FromName(bot.Client, ":regional_indicator_e:");
            w = DiscordEmoji.FromName(bot.Client, ":regional_indicator_w:");

        }

        public Dictionary<string, React> _reactLogic = new Dictionary<string, React>();
        public delegate Task React(MessageReactionAddEventArgs args);

        private Dictionary<string, int> _medalDictionary = new Dictionary<string, int>();
        private void SetRankDictionary()
        {
            _medalDictionary.Add("herald", 0);
            _medalDictionary.Add("guardian", 1);
            _medalDictionary.Add("crusader", 2);
            _medalDictionary.Add("archon", 3);
            _medalDictionary.Add("legend", 4);
            _medalDictionary.Add("ancient", 5);
            _medalDictionary.Add("divine", 6);
            _medalDictionary.Add("immortal", 7);
        }

        private void LogicDictionary()
        {

            _reactLogic.Add("Rank", async (MessageReactionAddEventArgs args) =>
            {
                var member = (DiscordMember)args.User;
                Dictionary<DiscordEmoji, DiscordRole> roles = new Dictionary<DiscordEmoji, DiscordRole>();

                roles.Add(one, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Herald").Value);
                roles.Add(two, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Guardian").Value);
                roles.Add(three, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Crusader").Value);
                roles.Add(four, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Archon").Value);
                roles.Add(five, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Legend").Value);
                roles.Add(six, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Ancient").Value);
                roles.Add(seven, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Divine").Value);
                roles.Add(eight, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Immortal").Value);

                await OnlyOne(roles, member, args.Emoji);

                await args.Message.DeleteReactionAsync(args.Emoji, args.User);

            });

            _reactLogic.Add("RankProgress", async (MessageReactionAddEventArgs args) =>
            {
                var member = (DiscordMember)args.User;
                Dictionary<DiscordEmoji, DiscordRole> roles = new Dictionary<DiscordEmoji, DiscordRole>();

                roles.Add(zero, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "0").Value);
                roles.Add(one, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "1").Value);
                roles.Add(two, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "2").Value);
                roles.Add(three, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "3").Value);
                roles.Add(four, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "4").Value);
                roles.Add(five, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "5").Value);

                await OnlyOne(roles, member, args.Emoji);

                var rank = member.Roles.FirstOrDefault(p => p.Name == "Herald" || p.Name == "Guardian" || p.Name == "Crusader" || p.Name == "Archon" || p.Name == "Legend" || p.Name == "Ancient" || p.Name == "Divine" || p.Name == "Immortal");

                if(roles.ContainsKey(args.Emoji) && rank != null)
                {
                    var val = roles[args.Emoji].Name;
                    var rankname = rank.Name;

                    var combined = rank.Name + " " + val;

                    if(_medalDictionary.ContainsKey(rank.Name.ToLower()))
                    {
                        Profile _profile = new Profile(args.Guild, args.User);

                        var start = _medalDictionary[rank.Name.ToLower()];
                        var pos = start * 6 * 130;

                        int progress = Int32.Parse(val);
                        var adj = progress * 130;
                        var final = pos + adj;

                        var record = await _context.player_data.FindAsync(_profile._id);
                        record._dotammr = final;

                        Console.WriteLine(record._dotammr);
                    }
                }
                
                await args.Message.DeleteReactionAsync(args.Emoji, args.User);

            });

            _reactLogic.Add("Region", async (MessageReactionAddEventArgs args) =>
            {
                var member = (DiscordMember)args.User;
                Dictionary<DiscordEmoji, DiscordRole> roles = new Dictionary<DiscordEmoji, DiscordRole>();

                roles.Add(e, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "US East").Value);
                roles.Add(w, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "US West").Value);

                await OnlyOne(roles, member, args.Emoji);

                await args.Message.DeleteReactionAsync(args.Emoji, args.User);

            });

            _reactLogic.Add("PrefPos", async (MessageReactionAddEventArgs args) =>
            {
                var member = (DiscordMember)args.User;
                Dictionary<DiscordEmoji, DiscordRole> roles = new Dictionary<DiscordEmoji, DiscordRole>();

                roles.Add(one, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == Bot._positions.Pos1 + "(Favorite)").Value);
                roles.Add(two, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == Bot._positions.Pos2 + "(Favorite)").Value);
                roles.Add(three, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == Bot._positions.Pos3 + "(Favorite)").Value);
                roles.Add(four, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == Bot._positions.Pos4 + "(Favorite)").Value);
                roles.Add(five, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == Bot._positions.Pos5 + "(Favorite)").Value);


                await OnlyOne(roles, member, args.Emoji);

                await args.Message.DeleteReactionAsync(args.Emoji, args.User);

            });

            _reactLogic.Add("Pos", async (MessageReactionAddEventArgs args) =>
            {
                var member = (DiscordMember)args.User;
                Dictionary<DiscordEmoji, DiscordRole> roles = new Dictionary<DiscordEmoji, DiscordRole>();

                roles.Add(one, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == Bot._positions.Pos1).Value);
                roles.Add(two, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == Bot._positions.Pos2).Value);
                roles.Add(three, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == Bot._positions.Pos3).Value);
                roles.Add(four, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == Bot._positions.Pos4).Value);
                roles.Add(five, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == Bot._positions.Pos5).Value);

                await AddRemove(roles, member, args.Emoji);

                await args.Message.DeleteReactionAsync(args.Emoji, args.User);

            });

            _reactLogic.Add("notifications", async (MessageReactionAddEventArgs args) =>
            {
                var member = (DiscordMember)args.User;
                Dictionary<DiscordEmoji, DiscordRole> roles = new Dictionary<DiscordEmoji, DiscordRole>();

                roles.Add(yes, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Member").Value);
                roles.Add(no, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Member").Value);

                if(args.Emoji == no)
                {
                    await member.RevokeRoleAsync(roles[args.Emoji]);
                }
                else if (args.Emoji == yes)
                {
                    await member.GrantRoleAsync(roles[args.Emoji]);
                }

                await args.Message.DeleteReactionAsync(args.Emoji, args.User);

            });

            _reactLogic.Add("profile", async (MessageReactionAddEventArgs args) =>
            {
                var member = (DiscordMember)args.User;


                if (args.Emoji == yes)
                {
                    await _emojiProfile.Profile(args, _context);
                }

                await args.Message.DeleteReactionAsync(args.Emoji, args.User);

            });

            _reactLogic.Add("extended", async (MessageReactionAddEventArgs args) =>
            {
                var member = (DiscordMember)args.User;

                Dictionary<DiscordEmoji, DiscordRole> roles = new Dictionary<DiscordEmoji, DiscordRole>();
                roles.Add(yes, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Extended").Value);
                roles.Add(no, args.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Extended").Value);

                if (args.Emoji == yes)
                {
                    await member.GrantRoleAsync(roles[args.Emoji]);
                }
                if (args.Emoji == no)
                {
                    await member.RevokeRoleAsync(roles[args.Emoji]);
                }

                await args.Message.DeleteReactionAsync(args.Emoji, args.User);

            });
        }

        public async Task AddRemove(Dictionary<DiscordEmoji, DiscordRole> roles, DiscordMember member, DiscordEmoji emoji)
        {

            if (member.Roles.Contains(roles[emoji]))
            {
                await member.RevokeRoleAsync(roles[emoji]);
            }
            else if (roles.ContainsKey(emoji))
            {
                await member.GrantRoleAsync(roles[emoji]);
            }
        }

        public async Task OnlyOne(Dictionary<DiscordEmoji, DiscordRole> roles, DiscordMember member, DiscordEmoji emoji)
        {
            if (roles.ContainsKey(emoji))
            {
                if(member.Roles.Contains(roles[emoji]))
                {
                    await member.RevokeRoleAsync(roles[emoji]);
                }
                else
                {
                    foreach (var role in roles)
                    {
                        if (member.Roles.Contains(role.Value))
                        {
                            await member.RevokeRoleAsync(role.Value);
                        }
                    }
                    await member.GrantRoleAsync(roles[emoji]);
                }

            }
        }


        
    }
}
