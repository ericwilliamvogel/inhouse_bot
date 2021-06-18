using db;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using OpenDotaDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bot_2.Commands
{


    public class StandardCommands : BaseCommands
    {

        private Dictionary<string, int> _medalDictionary;
        Dictionary<int, string> emojiLib = new Dictionary<int, string>();

        public StandardCommands(Context context) : base(context)
        {
            _medalDictionary = new Dictionary<string, int>();
            SetRankDictionary();
            SetEmojiLib();

        }

        private void SetEmojiLib()
        {
            emojiLib.Add(1, "(• ε •)");
            emojiLib.Add(2, "( ͡° ͜ʖ ͡°)");
            emojiLib.Add(3, "[̲̅$̲̅(̲̅ ͡° ͜ʖ ͡°̲̅)̲̅$̲̅]");
            emojiLib.Add(4, "(◕‿◕✿)");
            emojiLib.Add(5, "ᕙ(⇀‸↼‶)ᕗ");
            emojiLib.Add(6, "⚆ _ ⚆");
            emojiLib.Add(7, "༼ つ ◕_◕ ༽つ");
            emojiLib.Add(8, "ಠ_ಠ");
            emojiLib.Add(9, "(ง'̀-'́)ง");
            emojiLib.Add(10, "¯|_(ツ)_/¯");
        }
        public string GetEmoji(int input)
        {
            if (emojiLib.ContainsKey(input))
            {
                return emojiLib[input];
            }
            else
            {
                return "error";
            }

        }
        private void SetRankDictionary()
        {
            _medalDictionary.Add("herald", 0);
            _medalDictionary.Add("guardian ", 1);
            _medalDictionary.Add("crusader", 2);
            _medalDictionary.Add("archon", 3);
            _medalDictionary.Add("legend", 4);
            _medalDictionary.Add("ancient", 5);
            _medalDictionary.Add("divine", 6);
            _medalDictionary.Add("immortal", 7);
        }






        [Command("generatemessage")]
        public async Task GenMessage(CommandContext context)
        {
            Profile _profile = new Profile(context);

            var verified = await _conditions.AreMet(context, _profile,
                new List<Arg> {
                    Arg.HasAdminRole
                });

            if (!verified)
            {
                await context.Message.DeleteAsync();
                return;
            }
            await context.Channel.SendMessageAsync("manuallygeneratedmessage");
            await context.Message.DeleteAsync();

        }

        /*[Command("deletemessage")]
        public async Task RefreshQueue(CommandContext context, ulong id)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument> {
                            _conditions.IsRegistered,
                            _conditions.IsInCommandChannel
                },

                async () =>
                {

                    var msg = await context.Channel.GetMessageAsync(id);
                    await msg.DeleteAsync();
                });
        }
        */










        [Command("mmr")]
        public async Task ShowMMR(CommandContext context)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                            Arg.IsRegistered,
                            Arg.IsInCommandChannel
            },

            async () =>
            {

                await _profile.SendDm("!mmr is now deprecated. Use !profile instead!");

            });

        }

        [Command("updateregion")]
        public async Task UpdateRegion(CommandContext context, string input)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                            Arg.IsRegistered,
                            Arg.IsInCommandChannel
            },

            async () =>
            {

                var player = await _context.player_data.FindAsync(_profile._id);
                string msg = input.ToLower();
                if (msg == "east" || msg == "useast" || msg == "use")
                {
                    player._region = (int)Region.USEAST;
                    await _context.SaveChangesAsync();
                    await _profile.SendDm("Updated your region to " + (Region)player._region);
                }
                else if (msg == "west" || msg == "uswest" || msg == "usw")
                {
                    player._region = (int)Region.USWEST;
                    await _context.SaveChangesAsync();
                    await _profile.SendDm("Updated your region to " + (Region)player._region);
                }
                else
                {
                    await _profile.SendDm("Input " + input + " was not recognized as valid. Just type west or east :)");
                }

            });

        }

        [Command("updatepositions")]
        public async Task UpdateRoleOne(CommandContext context, int input, int input2)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel
            },

            async () =>
            {
                var player = await _context.player_data.FindAsync(_profile._id);
                if (input <= 0 || input >= 6 || input2 <= 0 || input2 >= 6)
                {
                    await _profile.SendDm("Input " + input + " was not recognized as valid. Use a number between 1 and 5.");
                }
                else
                {
                    player._role1 = input;
                    player._role2 = input2;
                    await _context.SaveChangesAsync();
                    await _profile.SendDm("Updated your primary role to " + player._role1 + ". Your second role has been set as " + player._role2 + ".");
                }


            });

        }

        [Command("updateposition")]
        public async Task URO(CommandContext context, int input, int input2)
        {
            await UpdateRoleOne(context, input, input2);
        }



        [Command("pingme")]
        public async Task PingMe(CommandContext context)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel
            },

            async () =>
            {

                var role = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Pingable").Value;
                if (role != null)
                {

                        await context.Member.GrantRoleAsync(role);
                        await _profile.SendDm("You will now be notified if games are in need of players.");
                    
                }
                else
                {
                    await _profile.SendDm("Role not found! Try again. If the error persists, create a ticket.");
                }

            });
        }

        [Command("dontpingme")]
        public async Task DontPingMe(CommandContext context)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel
            },

            async () =>
            {

                var role = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Pingable").Value;
                if (role != null)
                {

                        await context.Member.RevokeRoleAsync(role);
                        await _profile.SendDm("You will no longer be pinged if games are in need of players.");
                }
                else
                {
                    await _profile.SendDm("Role not found! Try again. If the error persists, create a ticket.");
                }

            });
        }
        [Command("preferences")]
        public async Task Preferences(CommandContext context)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel
            },

            async () =>
            {
                var player = await _context.player_data.FindAsync(_profile._id);

                string error = "";
                string info = "Your profile:";
                info += "\nDotaFriendID = " + player._steamid;
                info += "\nRegion = " + (Region)player._region;
                info += "\nRole1 = " + player._role1;
                info += "\nRole2 = " + player._role2;

                if (player._steamid == 0)
                {
                    error += "\n\nYou can update your friendid by using !updateid your_dota_friend_id in the general #commands channel. Example: !updateid 199304122";
                }
                if (player._dotammr == 0)
                {
                    error += "\n\nYou can update your mmr by using !updaterank in the general #commands channel. Example: !updaterank ancient 5";
                }
                if (player._region == (int)Region.NONE)
                {
                    error += "\n\nYou can update your region by using !updateregion your_region in the general #commands channel. Examples: !updateregion useast, !updateregion east, !updateregion uswest, !updateregion west";
                }
                if (player._role1 <= 0 || player._role1 > 5)
                {
                    error += "\n\nYou can update your positions by using !updatepositions most_comfortable second_most_comfortable in the general #commands channel. 2 positions must be entered. Examples: !updatepositions 5 4, !updatepositions 1 3, etc. ";
                }
                await _profile.SendDm(info + error);
                if (error == "")
                {
                    await _profile.SendDm("\n\nYour profile is up to date, nothing is missing.");
                }
    



            });

        }




        

        [Command("register")]
        public async Task RegisterPlayer(CommandContext context, long steamid)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsntRegistered,
                    Arg.IsInCommandChannel
                },

                async () =>
                {

                    var openDota = new OpenDotaApi();
                    var playerDetails = await openDota.Players.GetPlayerByIdAsync(steamid);

                    if (playerDetails == null)
                    {
                        await _profile.SendDm("Your dota friend id did not return a record. " +
                            "Please refer back to our welcome page for instructions on how to get your dota friend id and try again." +
                            " If you're sure everything is correct feel free to contact a mod / admin to get you set up.");
                    }
                    else
                    {
                        await _context.player_data.AddAsync(new PlayerData { _id = _profile._id, _steamid = steamid, _status = 1, _ihlmmr = 400, _dotammr = 0, _region = 0, _role1 = 0, _role2 = 0 });

                        await _context.SaveChangesAsync();
                        await _profile.SendDm("You are now registered. There are a few more steps to complete your registration so that you can queue! " +
                            "See below for a list of commands to get you started! When you finish your setup, type !queue or !q to jump into the queue for a game.\n" +
                            "Important: All commands are executed in the #commands channel, not in this DM channel.\n\n" +
                            "You can update your friendid by using !updateid your_dota_friend_id in the general #commands channel. Example: !updateid 199304122\n" +
                            "You can update your mmr by using !updaterank in the general #commands channel. Example: !updaterank ancient 5\n" +
                            "You can update your region by using !updateregion your_region in the general #commands channel. Examples: !updateregion useast, !updateregion east, !updateregion uswest, !updateregion west\n" +
                            "You can update your positions by using !updatepositions most_comfortable second_most_comfortable in the general #commands channel. 2 positions must be entered(no more, no less). Examples: !updatepositions 5 4, !updatepositions 1 3, etc. ");
                        /*if (playerDetails.MmrEstimate.Estimate == null)
                        {

                           
                            .ConfigureAwait(false);

                        }
                        else
                        {
                            await _context.player_data.AddAsync(new PlayerData { _id = _profile._id, _steamid = steamid, _status = 1, _ihlmmr = 400, _dotammr = (int)playerDetails.MmrEstimate.Estimate, _region = 0, _role1 = 0, _role2 = 0 }).ConfigureAwait(false);
                            await _profile.SendDm("You are now registered. Type !queue to enter the queue and !leave to leave the queue. Your GHL mmr starts at 400 and has a base increment of 15. Your initial mmr was recognized as " + (int)playerDetails.MmrEstimate.Estimate + ". If this is off by 300mmr or above, use the !updaterank command to update your mmr. Example: !updaterank ancient 5").ConfigureAwait(false);


                            await _context.SaveChangesAsync();
                        }*/


                    }


                });

        }


        [Command("updaterank")]
        public async Task UpdateRank(CommandContext context, string medal, int rank)
        {

            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel
                },

                async () =>
                {

                    var updatedmedal = medal.ToLower();
                    if (_medalDictionary.ContainsKey(updatedmedal))
                    {
                        var start = _medalDictionary[updatedmedal];
                        var pos = start * 6 * 130;
                        var adj = rank * 130;
                        var final = pos + adj;

                        var record = await _context.player_data.FindAsync(_profile._id);
                        record._dotammr = final;
                        await _context.SaveChangesAsync();

                        await _profile.SendDm("Updated mmr to " + final + ".");
                    }
                    else
                    {

                        await _profile.SendDm("The rank you entered wasn't recognized. It was received as " + medal + " " + rank + " Please enter a valid medal ie. Ancient, Legend, Archon, then a valid number between 1-5. Example: !updaterank ancient 3");
                    }
                });
        }

        [Command("updateid")]
        public async Task UpdatePlayer(CommandContext context, long steamid)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel
                },

                async () =>
                {

                    var openDota = new OpenDotaApi();
                    var playerDetails = await openDota.Players.GetPlayerByIdAsync(steamid);


                    if (playerDetails == null)
                    {
                        await _profile.SendDm("Your dota friend id did not return a record. " +
                            "Please refer back to our welcome page for instructions on how to get your dota friend id and try again." +
                            " If you're sure everything is correct feel free to contact a mod / admin to get you set up.");
                    }
                    else
                    {
                        var record = await _context.player_data.FindAsync(_profile._id);
                        record._steamid = steamid;
                        if (playerDetails.MmrEstimate.Estimate != null)
                        {
                            record._dotammr = (int)playerDetails.MmrEstimate.Estimate;
                        }
                        else
                        {
                            record._dotammr = 0;
                        }
                        await _context.SaveChangesAsync();

                        await _profile.SendDm("Your player id was updated.");
                    }

                });
        }

        [Command("emote")]
        public async Task Emoji(CommandContext context, int emoteType)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsRegistered
                },

                async () =>
                {
                    var record = await _context.emote_unlocked.FirstOrDefaultAsync(p => p._emoteid == emoteType && p._playerid == _profile._id);

                    if(record == null)
                    {
                        await _profile.SendDm("You do not have permission to use that emote.");
                    }
                    else
                    {
                        var mention = context.Message.Author.Mention;

                        string emoji = "```" + GetEmoji(emoteType) + "```";
                        string msg = mention + " emoted: " + emoji;

                        await context.Channel.SendMessageAsync(msg);
                    }

                });
        }

        [Command("emote")]
        public async Task Emoji(CommandContext context, int emoteType, string doesntmatter)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsRegistered,
                    Arg.HasMention
                },

                async () =>
                {
                
                    var record = await _context.emote_unlocked.FirstOrDefaultAsync(p => p._emoteid == emoteType && p._playerid == _profile._id);

                    if (record == null)
                    {
                        await _profile.SendDm("You do not have permission to use that emote.");
                    }
                    else
                    {
                            var mention = context.Message.MentionedUsers.First().Mention;

                            string emoji = "```" + GetEmoji(emoteType) + "```";
                            string msg = mention + " - " + emoji;

                            await context.Channel.SendMessageAsync(msg);
                    }



                });
        }

        [Command("profile")]
        public async Task Profile(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel
                },

                async () =>
                {
                    var playerrecord = await _context.player_data.FindAsync(_profile._id);


                    string profile = "---Player Profile---\n";
                    profile += "GHL mmr: " + playerrecord._ihlmmr;
                    profile += "\nDota mmr: " + playerrecord._dotammr;
                    profile += "\nW/L: " + playerrecord._gameswon + "/" + playerrecord._gameslost;
                    profile += "\nTotal games: " + playerrecord._totalgames;


                    profile += "\nCoins: " + playerrecord._xp;

                    var largeEmojiList = await _context.emote_unlocked.ToListAsync();
                    var emojiList = largeEmojiList.FindAll(p => p._playerid == _profile._id);
                    profile += "\n\n---Emotes owned---\n";
                    foreach (KeyValuePair<int, string> pair in emojiLib)
                    {
                        string ifOwned = "Not owned";
                        if(emojiList.FirstOrDefault(p => p._emoteid == pair.Key) != null)
                        {
                            ifOwned = "Owned";
                        }
                        profile += pair.Value + " (" + pair.Key + ") : " + ifOwned + "\n";
                    }
                    await _profile.SendDm(profile);
                });
        }

        [Command("open")]
        public async Task Open(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel,
                    Arg.HasTrustedRole
                },

                async () =>
                {
                    TaskScheduler._inhouseOpen = true;
                    if (context.Guild.Channels.ContainsKey(839331576554455050))
                    {
                        var channel = context.Guild.Channels[839331576554455050];
                        await channel.SendMessageAsync("<@&852359190453288981>, Queueing has been opened early by <@" + _profile._id + ">. Queueing will still close at 1amEST. \n\nReminder to use !pingme to be given the <@&852359190453288981> role. If you change your mind use !dontpingme.");
                    }
                    await _profile.SendDm("Inhouse has been opened early. Doors will close at 1amEST as scheduled.");
                });
        }

        [Command("bet")]
        public async Task Bet(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel
                },

                async () =>
                {
                    await _profile.SendDm("'!bet bet_amount team1_or_team2 inhouse_game_id' : To bet on a live game.\nUsage: To place a bet of 100 on side team1, GameID 4033 = '!bet 100 team1 4033'");
                });
        }
        [Command("bet")]
        public async Task Bet(CommandContext context, int amount, string side, int gameid)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel
                },

                async () =>
                {
                    int sideToInt = (int)Side.Draw;
                    if (side.ToLower() == "team1")
                    {
                        sideToInt = (int)Side.Team1;
                    }
                    else if (side.ToLower() == "team2")
                    {
                        sideToInt = (int)Side.Team2;
                    }
                    else
                    {
                        await _profile.SendDm("'" + side + "' is not a valid side. Use 'team1' or 'team2'.");
                        return;
                    }

                    //doublecheck
                    if(sideToInt == (int)Side.Draw)
                    {
                        await _profile.SendDm("Error occurred for some reason. Side returned as a draw instead of " + side + ".");
                        return;
                    }

                    var playerrecord = await _context.player_data.FindAsync(_profile._id);
                    if(amount > playerrecord._xp)
                    {
                        await _profile.SendDm("You only have " + playerrecord._xp + " coins on your account. You cannot make this bet for " + amount + " coins.");
                        return;
                    }

                    var discordrecord = await _context.discord_channel_info.FirstOrDefaultAsync(p => p._gameid == gameid);
                    if(discordrecord == null)
                    {
                        await _profile.SendDm("No internal discord record was found under that game id. A game id should have 4 digits. The game must be active in order to bet on it.");
                        return;
                    }

                    var game = await _context.game_data.FindAsync(gameid);
                    if(game == null)
                    {
                        await _profile.SendDm("No game record was found under that game id. A game id should have 4 digits. The game must be active in order to bet on it.");
                        return;
                    }

                    DateTimeOffset now = DateTimeOffset.Now;
                    TimeSpan span = now - game._start;
                    if(span.Minutes >= 20)
                    {
                        await _profile.SendDm("You cannot bet on a game after 20 minutes has passed.");
                        return;
                    }

                    var findgame = await _context.game_bets.FirstOrDefaultAsync(p => p._discordid == _profile._id && p._gameid == gameid);
                    if(findgame != null)
                    {
                        await _profile.SendDm("You already have a bet active for this game.");
                        return;
                    }


                    await _context.game_bets.AddAsync(new BetData { _discordid = _profile._id, _amount = amount, _side = sideToInt, _gameid = gameid });
                    await _context.SaveChangesAsync();

                    playerrecord._xp -= amount;
                    await _context.SaveChangesAsync();

                    await _profile.SendDm("Bet submitted. Good luck!\nGameId: " + gameid + "\nSide: " + side + "\nAmount: " + amount + " coins\nPotential winnings: " + amount*2 + " coins\nCoins remaining: " + playerrecord._xp + " coins");


                    
                    if(context.Guild.Channels.ContainsKey(852929890780840007))
                    {
                        var channel = context.Guild.Channels[852929890780840007];
                        await channel.SendMessageAsync("-----------------\n<@" + _profile._id + ">\nGameId: " + gameid + "\nSide: " + side + "\nAmount: " + amount + " coins\nPotential winnings: " + amount*2 + " coins\nCoins remaining: " + playerrecord._xp + " coins\n-----------------");
                    }
                });
        }




    }
}
