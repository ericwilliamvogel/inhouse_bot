using db;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_2.Commands
{
    public class CasterCommands : BaseCommands
    {
        public CasterCommands(Context context) : base(context)
        {


        }

        [Command("vod")]
        public async Task EnterQueue(CommandContext context, ulong messageid, string vod)
        {
            Profile _profile = new Profile(context);
            //we need to seperate the two bundles of arguments because we'll return a null error if the user doesn't have a record under player_data/player_record
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsInCommandChannel,
                    Arg.HasCasterRole
                },

                async () =>
                {
                    if (context.Guild.Channels.ContainsKey(842870150994591764))
                    {
                        var channel = context.Guild.Channels[842870150994591764];
                        var msg = await channel.GetMessageAsync(messageid);
                        if(msg != null)
                        {
                            await msg.ModifyAsync(msg.Content + "\n" + vod);
                            await _profile.SendDm("Vod was added to this game.");
                        }
                        else
                        {
                            await _profile.SendDm("This message ID was not found. Please try again. If the problem persists, consider opening a ticket to report this.");
                        }
                    }
                    else
                    {
                        await _profile.SendDm("The #game-history channel wasn't recognized under the backend ID. Please open a ticket, screenshot this, and place it inside the ticket.");
                    }



                });





        }


        [Command("casterqueue")]
        public async Task EnterQueue(CommandContext context)
        {
            Profile _profile = new Profile(context);

            var verified = await _conditions.AreMet(context, _profile,
                new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel
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
                       Arg.IsntCasterQueued,
                       Arg.HasCasterRole
                },

                async () =>
                {

                    var myDateTime = DateTimeOffset.Now;
                    await _context.caster_queue.AddAsync(new CasterQueueData { _id = _profile._id, _start = myDateTime }).ConfigureAwait(false);
                    var record = await _context.player_data.FindAsync(_profile._id);
                    record._gamestatus = 1;
                    await _context.SaveChangesAsync().ConfigureAwait(false);


                    await _updatedQueue.StartThread(context);
                    await _profile.SendDm("You've been placed in CASTER queue. You will be notified via DM when it pops. In the server's command channel type !casterleave if you would like to leave the queue.");
 



                });





        }

        [Command("casterleave")]
        public async Task LeaveQueue(CommandContext context)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                            Arg.IsRegistered,
                            Arg.IsInCommandChannel,
                            Arg.IsCasterQueued,
                            Arg.HasCasterRole
                },

                async () =>
                {


                    var record = await _context.caster_queue.FirstOrDefaultAsync(p => p._id == _profile._id);
                    if (record == null)
                    {
                        await _profile.SendDm("Error, I have no idea how this could happen. Caster queue record not recognized on leave. Create a ticket and report this message please.");
                    }



                    _context.caster_queue.Remove(record);


                    var c_record = await _context.player_data.FindAsync(_profile._id);
                    c_record._gamestatus = 0;
                    await _context.SaveChangesAsync();


                    await _profile.SendDm("You've been removed from queue.");


                });
        }

    }
}
