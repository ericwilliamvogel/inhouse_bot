using db;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenDotaDotNet;

namespace bot_2.Commands
{


    public class StandardCommands : BaseCommands
    {

        private Dictionary<string, int> _medalDictionary;


        public StandardCommands(Context context) : base(context)
        {
            _medalDictionary = new Dictionary<string, int>();
            SetRankDictionary();

        }
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






        /*[Command("generatemessage")]
        public async Task GenMessage(CommandContext context)
        {
            Profile _profile = new Profile(context);

            var verified = await _conditions.AreMet(context, _profile,
                new List<Argument> {

                });

            if (!verified)
            {
                await context.Message.DeleteAsync();
                return;
            }
            await context.Channel.SendMessageAsync("manuallygeneratedmessage");
            await context.Message.DeleteAsync();
        
        }*/

        [Command("leave")]
        public async Task LeaveQueue(CommandContext context)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument> {
                            _conditions.IsRegistered,
                            _conditions.IsInCommandChannel,
                            _conditions.IsQueued
                },

                async () =>
                {


                    var record = _context.player_queue.First(p => p._id == _profile._id);
                    if (record == null)
                    {
                        await _profile.SendDm("Error, I have no idea how this could happen. DM an admin or something to get it fixed.");
                    }

                    _context.player_queue.Remove(record);
                    await _context.SaveChangesAsync();
                    await _profile.SendDm("You've been removed from queue.");


                });
        }



        [Command("radiant")]
        public async Task RadiantWin(CommandContext context, long steamid)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument> {
                            _conditions.IsRegistered,
                            _conditions.IsInCommandChannel
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

            new List<Argument> {
                            _conditions.IsRegistered,
                            _conditions.IsInCommandChannel
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

            new List<Argument> {
                            _conditions.IsRegistered,
                            _conditions.IsInCommandChannel
            },

            async () =>
            {


                LobbySorter sorter = new LobbySorter(_context);
                await sorter._utilities.ReportWinner(context, "draw", steamid);

            });




        }

        [Command("q")]
        public async Task q(CommandContext context)
        {
            await EnterQueue(context);
        }


        [Command("mmr")]
        public async Task ShowMMR(CommandContext context)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

            new List<Argument> {
                            _conditions.IsRegistered,
                            _conditions.IsInCommandChannel
            },

            async () =>
            {

                var player = await _context.player_data.FindAsync(_profile._id);

                await _profile.SendDm("Your GHL mmr is " + player._ihlmmr);
                await _profile.SendDm("Your Dota mmr is " + player._dotammr);
                await _profile.SendDm("You've won " + player._gameswon + " games.");
                await _profile.SendDm("You've lost " + player._gameslost + " games.");

            });

        }
        [Command("queue")]
        public async Task EnterQueue(CommandContext context)
        {
            Profile _profile = new Profile(context);
            var verified = await _conditions.AreMet(context, _profile,
                new List<Argument> {
                    _conditions.IsRegistered,
                    _conditions.IsInCommandChannel
                });

            if (!verified)
            {
                await context.Message.DeleteAsync();
                return;
            }

            //we need to seperate the two bundles of arguments because we'll return a null error if the user doesn't have a record under player_data/player_record
            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument> {
                       _conditions.IsReady,
                       _conditions.IsntQueued
                },

                async () =>
                {

                    string dt = DateTime.Now.ToString();
                    await _context.player_queue.AddAsync(new QueueData { _id = _profile._id, _start = dt }).ConfigureAwait(false);
                    await _context.SaveChangesAsync().ConfigureAwait(false);


                    var list = await _context.player_queue.ToListAsync();
                    int count = list.Count();

                    await _updatedQueue.StartThread(context);

                    if (count >= 10)
                    {
                        LobbySorter sorter = new LobbySorter(_context);
                        await sorter.Setup(context);
                    }
                    else
                    {
                        await _profile.SendDm("You've been placed in queue. You will be notified via DM when it pops. In the server's command channel type !leave if you would like to leave the queue.");

                    }



                });





        }

        [Command("register")]
        public async Task RegisterPlayer(CommandContext context, long steamid)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument> {
                    _conditions.IsntRegistered,
                    _conditions.IsInCommandChannel
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
                        await _context.player_data.AddAsync(new PlayerData { _id = _profile._id, _steamid = steamid, _status = 1, _ihlmmr = 400, _dotammr = (int)playerDetails.MmrEstimate.Estimate }).ConfigureAwait(false);
                        await _context.SaveChangesAsync().ConfigureAwait(false);
                        await _profile.SendDm("You are now registered. Type !queue to enter the queue and !leave to leave the queue. Your GHL mmr starts at 400 and has a base increment of 15. Your initial mmr was recognized as " + (int)playerDetails.MmrEstimate.Estimate + ". If this is off by 300mmr or above,  create a ticket and screenshot your mmr from the DotA client and send it via the ticket.").ConfigureAwait(false);
                    }


                });

        }



        [Command("updaterank")]
        public async Task UpdateRank(CommandContext context, string medal, int rank)
        {

            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument> {
                    _conditions.IsRegistered,
                    _conditions.IsInCommandChannel
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

                new List<Argument> {
                    _conditions.IsRegistered,
                    _conditions.IsInCommandChannel
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
                        record._dotammr = (int)playerDetails.MmrEstimate.Estimate;
                        await _context.SaveChangesAsync();

                        await _profile.SendDm("Your player id was updated and your mmr was recognized as " + (int)playerDetails.MmrEstimate.Estimate + ". If this is off by 300mmr or above,  create a ticket and screenshot your mmr from the DotA client and send it via the ticket.");
                    }

                });
        }



    }
}
