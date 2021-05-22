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

        [Command("radiant")]
        public async Task RadiantWin(CommandContext context, long steamid)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                            Arg.IsRegistered,
                            Arg.IsInCommandChannel
                },

                async () =>
                {

                    LobbySorter sorter = new LobbySorter(_context);
                    await sorter._utilities.ReportWinner(context, "radiant", steamid);


                });
        }

        [Command("dire")]
        public async Task DireWin(CommandContext context, long steamid)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                            Arg.IsRegistered,
                            Arg.IsInCommandChannel
            },

            async () =>
            {



                LobbySorter sorter = new LobbySorter(_context);
                await sorter._utilities.ReportWinner(context, "dire", steamid);

            });

        }

        [Command("draw")]
        public async Task DrawGame(CommandContext context, long steamid)
        {
            Profile _profile = new Profile(context);


            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                            Arg.IsRegistered,
                            Arg.IsInCommandChannel
            },

            async () =>
            {


                LobbySorter sorter = new LobbySorter(_context);
                await sorter._utilities.ReportWinner(context, "draw", steamid);

            });




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


                    var record = await _context.game_record.FirstOrDefaultAsync(p => p._p1 == _profile._id && p._p5 == 0);
                    var list = await _context.lobby_pool.ToListAsync();
                    var player = list.FirstOrDefault(p => p._discordid == playerid && p._gameid == record._gameid);

                    if (player != null)
                    {
                        if (record != null)
                        {
                            if (record._p2 == 0)
                            {
                                record._p2 = playerid;
                            }
                            else if (record._p3 == 0)
                            {
                                record._p3 = playerid;
                            }
                            else if (record._p4 == 0)
                            {
                                record._p4 = playerid;
                            }
                            else if (record._p5 == 0)
                            {
                                record._p5 = playerid;
                            }

                            var gameid = record._gameid;
                            record._canpick = 0;
                            _context.lobby_pool.Remove(player);
                            await _context.SaveChangesAsync();
                            int side = record._side;


                            //update

                            var newRecord = await _context.game_record.FirstOrDefaultAsync(p => p._side != side && p._gameid == gameid);
                            if (record._p5 != 0 && newRecord._p5 != 0)
                            {
                                LobbySorter sorter = new LobbySorter(_context);
                                await sorter.FormLobby(context, _profile, gameid);

                            }
                            else
                            {
                                newRecord._canpick = 1;
                                await _context.SaveChangesAsync();

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
