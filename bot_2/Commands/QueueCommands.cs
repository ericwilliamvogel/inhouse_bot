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
    public class QueueCommands : BaseCommands
    {

        public QueueCommands(Context context) : base(context)
        {

        }
        [Command("spectatorqueue")]
        public async Task SpectatorQueue(CommandContext context)
        {
            Profile _profile = new Profile(context);

            var verified = await _conditions.AreMet(context, _profile,
                new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel,
                    Arg.InhouseIsOpen
                });

            if (!verified)
            {
                await context.Message.DeleteAsync();
                return;
            }

            //we need to seperate the two bundles of arguments because we'll return a null error if the user doesn't have a record under player_data/player_record
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                       Arg.IsReady,
                       Arg.IsntSpectatorQueued
                },

                async () =>
                {

                    var myDateTime = DateTimeOffset.Now;
                    await _context.spectator_queue.AddAsync(new SpectatorQueueData { _id = _profile._id, _start = myDateTime }).ConfigureAwait(false);
                    var record = await _context.player_data.FindAsync(_profile._id);
                    record._gamestatus = 1;
                    await _context.SaveChangesAsync().ConfigureAwait(false);


                    await _updatedQueue.StartThread(context);
                    await _profile.SendDm("You've been placed in SPECTATOR queue. You will be notified via DM when it pops. In the server's command channel type !spectatorleave if you would like to leave the queue.");




                });





        }

        [Command("spectatorleave")]
        public async Task SpectatorLeaveQueue(CommandContext context)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                            Arg.IsRegistered,
                            Arg.IsInCommandChannel,
                            Arg.IsSpectatorQueued,
                            Arg.InhouseIsOpen
                },

                async () =>
                {


                    var record = _context.spectator_queue.FirstOrDefault(p => p._id == _profile._id);
                    if (record == null)
                    {
                        await _profile.SendDm("Error, I have no idea how this could happen. Spectator queue record not recognized on leave. Create a ticket and report this message please.");
                    }



                    _context.spectator_queue.Remove(record);


                    var s_record = await _context.player_data.FindAsync(_profile._id);
                    s_record._gamestatus = 0;
                    await _context.SaveChangesAsync();


                    await _profile.SendDm("You've been removed from queue.");


                });
        }

        [Command("refresh")]
        public async Task RefreshQueue(CommandContext context)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                            Arg.IsRegistered,
                            Arg.IsInCommandChannel
                },

                async () =>
                {

                    await _updatedQueue.StartThread(context);

                });
        }
        [Command("l")]
        public async Task LQueue(CommandContext context)
        {
            await LeaveQueue(context);
        }

        [Command("leave")]
        public async Task LeaveQueue(CommandContext context)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                            Arg.IsRegistered,
                            Arg.IsInCommandChannel,
                            Arg.IsQueued,
                            Arg.InhouseIsOpen
                },

                async () =>
                {


                    var record = _context.player_queue.FirstOrDefault(p => p._id == _profile._id);
                    if (record == null)
                    {
                        await _profile.SendDm("Error, I have no idea how this could happen. DM an admin or something to get it fixed.");
                    }

                    _context.player_queue.Remove(record);

                    var p_record = await _context.player_data.FindAsync(_profile._id);
                    p_record._gamestatus = 0;
                    await _context.SaveChangesAsync();
                    await _profile.SendDm("You've been removed from queue.");


                });
        }

        [Command("q")]
        public async Task q(CommandContext context)
        {
            await EnterQueue(context);
        }

        [Command("queue")]
        public async Task EnterQueue(CommandContext context)
        {
            Profile _profile = new Profile(context);
            var verified = await _conditions.AreMet(context, _profile,
                new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel,
                    Arg.InhouseIsOpen
                });

            if (!verified)
            {
                await context.Message.DeleteAsync();
                return;
            }

            //we need to seperate the two bundles of arguments because we'll return a null error if the user doesn't have a record under player_data/player_record
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                       Arg.IsReady,
                       Arg.IsntQueued,
                       Arg.ProfileComplete
                },

                async () =>
                {

                    var myDateTime = DateTimeOffset.Now;
                    //string sqlFormattedDate = myDateTime.ToString("HH:mm:ss");
                    await _context.player_queue.AddAsync(new QueueData { _id = _profile._id, _start = myDateTime }).ConfigureAwait(false);
                    
                    await _context.SaveChangesAsync();
                    var record = await _context.player_data.FindAsync(_profile._id);
                    record._gamestatus = 1;

                    await _context.SaveChangesAsync();


                    var list = await _context.player_queue.ToListAsync();
                    int count = list.Count();

                     await _updatedQueue.StartThread(context);

                    if (count >= 10)
                    {
                        LobbySorter sorter = new LobbySorter(_context);
                        await sorter.Setup(context, _profile);
                    }
                    else
                    {
                        await _profile.SendDm("You've been placed in queue. You will be notified via DM when it pops. In the server's command channel type !leave if you would like to leave the queue.");

                    }



                });





        }




    }


}
