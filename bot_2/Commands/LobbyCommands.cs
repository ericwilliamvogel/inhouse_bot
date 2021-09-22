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
    public class LobbyCommands : BaseCommands
    {
        public LobbyCommands(Context context) : base(context)
        {

        }

        [Command("team1")]
        public async Task Team1Win(CommandContext context, long steamid)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                            Arg.IsRegistered
                },

                async () =>
                {

                    LobbySorter sorter = new LobbySorter(_context);
                    await sorter._report.ReportWinner(context, "team1", steamid);


                });
        }

        [Command("team2")]
        public async Task Team2Win(CommandContext context, long steamid)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                            Arg.IsRegistered
            },

            async () =>
            {



                LobbySorter sorter = new LobbySorter(_context);
                await sorter._report.ReportWinner(context, "team2", steamid);

            });

        }

        [Command("draw")]
        public async Task DrawGame(CommandContext context, long steamid)
        {
            Profile _profile = new Profile(context);


            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                            Arg.IsRegistered
            },

            async () =>
            {


                LobbySorter sorter = new LobbySorter(_context);
                await sorter._report.ReportWinner(context, "draw", steamid);

            });




        }

        [Command("invite")]
        public async Task Invite(CommandContext context, string doesntmatter)
        {
            Profile _profile = new Profile(context);


            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                            Arg.IsLobbyHost,
                            Arg.HasMention
            },

            async () =>
            {
                var user = context.Message.MentionedUsers.First().Id;

                if (context.Guild.Members.ContainsKey(user))
                {
                    LobbyUtilities _util = new LobbyUtilities(_context);
                    var host = await _context.discord_channel_info.FindAsync(_profile._id);
                    var number = host._number;

                    var member = context.Guild.Members[user];
                    var role = _util.GetRole(context, "spectatorLobby" + number);

                    if (role != null)
                    {
                        await _profile.SendDm("Role granted.");
                        await member.GrantRoleAsync(role);
                    }
                    else
                    {
                        await _profile.SendDm("Role not found? Send pip a message. This is new code, so its an easy fix");
                    }

                }
                else
                {
                    await _profile.SendDm("This user was not found in the server. Are they offline? Is their discord closed? Are they currently on another server?");
                }



            });
        }

        [Command("invite")]
        public async Task Invite(CommandContext context, string team, string doesntmatter)
        {
            Profile _profile = new Profile(context);


            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                            Arg.IsLobbyHost,
                            Arg.HasMention
            },

            async () =>
            {
                var user = context.Message.MentionedUsers.First().Id;

                if (context.Guild.Members.ContainsKey(user))
                {
                    if(team.ToLower() != "team1" && team.ToLower() != "team2")
                    {
                        await _profile.SendDm(team + " isn't a valid team. Please use team1 or team2 to assign the role to this player.");
                        return;
                    }

                    LobbyUtilities _util = new LobbyUtilities(_context);
                    var host = await _context.discord_channel_info.FindAsync(_profile._id);
                    var number = host._number;

                    var member = context.Guild.Members[user];
                    var role = _util.GetRole(context, team + "Lobby" + number);

                    if (role != null)
                    {
                        await _profile.SendDm("Role granted.");
                        await member.GrantRoleAsync(role);
                    }
                    else
                    {
                        await _profile.SendDm("Role not found? Send pip a message. This is new code, so its an easy fix");
                    }

                }
                else
                {
                    await _profile.SendDm("This user was not found in the server. Are they offline? Is their discord closed? Are they currently on another server?");
                }



            });
        }
        [Command("kick")]
        public async Task KickPlayer(CommandContext context, string input)
        {
            Profile _profile = new Profile(context);


            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                            Arg.IsLobbyHost,
                            Arg.HasMention
            },

            async () =>
            {
                await _profile.SendDm("You meant to use !draw 0, right? You listened to what pip said in trusted #general chat at 8:00pm PST on 6/4/21 right? You waited until picking phase ended to issue this command... right???? Pls ;-;");
                /*var lobbyInfo = await _context.discord_channel_info.FindAsync(_profile._id);


                var radiant = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == lobbyInfo._gameid && p._side == (int)Side.Radiant);
                var dire = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == lobbyInfo._gameid && p._side == (int)Side.Dire);

                ulong mentionedPlayerId = context.Message.MentionedUsers.First().Id;


                var priorityPlayers = GetPlayerIds(radiant, mentionedPlayerId);
                priorityPlayers.AddRange(GetPlayerIds(dire, mentionedPlayerId));

                List<Player> backlogPlayers = new List<Player>();

                var unorderedQueue = await _context.player_queue.ToListAsync();
                var orderedQueue = unorderedQueue.OrderBy(p => p._position).ToList();

                List<QueueData> toBeRemovedFromQueue = new List<QueueData>();

                foreach(var record in orderedQueue)
                {
                    backlogPlayers.Add(new Player(record._id, record._start));
                    toBeRemovedFromQueue.Add(record);
                }

                foreach(var record in toBeRemovedFromQueue)
                {
                    _context.player_queue.Remove(record);
                    //await _context.SaveChangesAsync();
                }

                                await DrawGame(context, 0);

                var myDateTime = DateTimeOffset.Now;
                foreach (var id in priorityPlayers)
                {
                    await _context.player_queue.AddAsync(new QueueData { _id = id, _start = myDateTime }).ConfigureAwait(false);
                    //await _context.SaveChangesAsync();
                }

                foreach(var record in backlogPlayers)
                {
                    await _context.player_queue.AddAsync(new QueueData { _id = record._id, _start = record._start }).ConfigureAwait(false);
                    //await _context.SaveChangesAsync();
                }


                //LobbySorter sorter = new LobbySorter(_context);
                //await sorter._report.WrapUp(context, lobbyInfo._number);
                //code
                */

            });




        }

        private List<ulong> GetPlayerIds(TeamRecord record, ulong id)
        {
            List<ulong> ids = new List<ulong>();

            if (record._p1 != id)
            {
                ids.Add(record._p1);
            }
            if (record._p2 != id)
            {
                ids.Add(record._p2);
            }
            if (record._p3 != id)
            {
                ids.Add(record._p3);
            }
            if (record._p4 != id)
            {
                ids.Add(record._p4);
            }
            if (record._p5 != id)
            {
                ids.Add(record._p5);
            }

            return ids;
        }

        [Command("p")]
        public async Task P(CommandContext context, string irrelevant)
        {
            await Pick(context, irrelevant);
        }
        [Command("pick")]
        public async Task Pick(CommandContext context, string irrelevant)
        {

            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsLobbyCaptain,
                    Arg.CanPick,
                    Arg.HasMention
                },

                async () =>
                {
                    var playerid = context.Message.MentionedUsers.FirstOrDefault().Id;

                    var gamerecordslist = await _context.game_record.ToListAsync();
                    var newlist = gamerecordslist.FindAll(p => p._p1 == _profile._id).OrderByDescending(p => p._gameid).ToList();

                    if(newlist == null || newlist.Count == 0)
                    {
                        await _profile.SendDm("No game records were found~~ Please open a ticket and ping Pip.");
                        return;
                    }


                    var record = newlist[0];

                    //var record = await _context.game_record.FirstOrDefaultAsync(p => p._p1 == _profile._id && p._p5 == 0);
                    var list = await _context.lobby_pool.ToListAsync();
                    var player = await _context.lobby_pool.FirstOrDefaultAsync(p => p._discordid == playerid && p._gameid == record._gameid);


                    int gameid = record._gameid;
                    var side = (Side)record._side;

                    var newRecord = await _context.game_record.FirstOrDefaultAsync(p => p._side != record._side && p._gameid == gameid);


                    if (player != null)
                    {
                        if (record != null)
                        {
                            if (record._p2 == 0)
                            {
                                record._p2 = playerid;
                                if(side == Side.Team1)
                                {
                                    record._canpick = 0;
                                    newRecord._canpick = 1;
                                    //await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    //nothing?
                                }
                            }
                            else if (record._p3 == 0)
                            {
                                record._p3 = playerid;
                                if (side == Side.Team1)
                                {
                                    //nothing?
                                }
                                else
                                {
                                    record._canpick = 0;
                                    newRecord._canpick = 1;
                                    //await _context.SaveChangesAsync();
                                }
                            }
                            else if (record._p4 == 0)
                            {
                                record._p4 = playerid;
                                if (side == Side.Team1)
                                {
                                    record._canpick = 0;
                                    newRecord._canpick = 1;
                                    //await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    //nothing?
                                }
                            }
                            else if (record._p5 == 0)
                            {
                                record._p5 = playerid;

                                record._canpick = 0;
                                newRecord._canpick = 1;
                                //await _context.SaveChangesAsync();
                            }

   
                            _context.lobby_pool.Remove(player);
                            //await _context.SaveChangesAsync();

                            if (record._p5 != 0 && newRecord._p5 != 0)
                            {
                                LobbySorter sorter = new LobbySorter(_context);
                                await sorter.FormLobby(context, _profile, gameid);

                            }
                            else
                            {
                                
                                LobbySorter sorter = new LobbySorter(_context);
                                await sorter.UpdateLobbyPool(context, _profile, gameid);



                            }

                        }

                    }
                    else
                    {
                        await _profile.SendDm("That player is not available to be drafted.");
                    }

                });
        }
    }
}
