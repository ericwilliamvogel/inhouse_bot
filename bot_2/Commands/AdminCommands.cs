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

    public class AdminCommands : BaseCommands
    {
        /*[Command("role")]
public async Task EnterQueue(CommandContext context, int number)
{
    Profile _profile = new Profile(context);

    var verified = await _conditions.AreMet(context, _profile,
        new List<Argument> {
            _conditions.IsRegistered,
            _conditions.IsInAdminCommandChannel
        });

    if (!verified)
    {
        await context.Message.DeleteAsync();
        return;
    }

    var relativeData = await _context.player_data.FindAsync(_profile._id);
    if (relativeData != null)
    {
        relativeData._status = number;
        await _context.SaveChangesAsync();
    }
    await context.Message.DeleteAsync();

}
*/
        public AdminCommands(Context context) : base(context)
        {


        }

        [Command("manuallyrestartthread")]
        public async Task RestartThread(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument> {
                    _conditions.IsInAdminCommandChannel,
                    _conditions.HasAtLeastAdminRole
                },

                async () =>
                {
                    await UpdatedQueue.ResetVariables();
                    await _updatedQueue.StartThread(context);
                });
        }

        [Command("testid")]
        public async Task TestID(CommandContext context, long steamid)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument> {
                    _conditions.IsInAdminCommandChannel
                },

                async () =>
                {
                    var openDota = new OpenDotaApi();
                    var playerDetails = await openDota.Players.GetPlayerByIdAsync(steamid);

                    if (playerDetails != null)
                        await _profile.SendDm("MmrEstimate : " + playerDetails.MmrEstimate.Estimate);

                });
        }

        [Command("getid")]
        public async Task GetID(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument> {
                    //isadmin - coming soon
                },

                async () =>
                {
                    await _profile.SendDm(context.Channel.Id.ToString());
                    await _profile.SendDm(context.Channel.GuildId.ToString());

                });

        }

        [Command("close")]
        public async Task Close(CommandContext context, int number)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument>
                {
                    _conditions.IsInAdminCommandChannel
                },

                async () =>
                {
                    LobbySorter sorter = new LobbySorter(_context);
                    await sorter._utilities.CloseLobby(context, number);

                });



        }

        [Command("queueclear")]
        public async Task Clear(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument>
                {
                     _conditions.IsInAdminCommandChannel
                },

                async () =>
                {
                    var queue = await _context.player_queue.ToListAsync();
                    foreach (QueueData record in queue)
                    {
                        _context.player_queue.Remove(record);
                    }
                    await _context.SaveChangesAsync();

                    await _profile.SendDm("The queue has been cleared.").ConfigureAwait(false);
                });


        }

        [Command("channelclear")]
        public async Task Clear(CommandContext context, ulong input)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument> {
                    _conditions.IsInAdminCommandChannel
                },

                async () =>
                {
                    if (context.Guild.Channels.ContainsKey(input))
                    {
                        var channel = context.Guild.Channels[input];
                        await DeleteLastMessage(channel);
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync("Channel not recognized under the id you typed.").ConfigureAwait(false);
                    }

                });

        }

        public async Task DeleteLastMessage(DiscordChannel channel)
        {
            var list = await channel.GetMessagesAsync(100);
            await channel.DeleteMessagesAsync(list);
        }

        [Command("resetstatus")]
        public async Task UpdateRole(CommandContext context, string user)
        {

            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument> {
                    _conditions.IsInAdminCommandChannel
                },

                async () =>
                {
                    var ment = context.Message.MentionedUsers.First().Id;
                    var record = await _context.player_data.FindAsync(ment);
                    if (record == null)
                    {
                        await _profile.SendDm("Player mentioned in command wasn't found in database.");
                    }
                    else
                    {
                        await _profile.SendDm("Player's queue status was reset.");
                        record._gamestatus = 0;
                        await _context.SaveChangesAsync();
                    }

                });


        }

        [Command("updaterole")]
        public async Task UpdateRole(CommandContext context, string user, int role)
        {

            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument> {
                    _conditions.IsInAdminCommandChannel
                },

                async () =>
                {

                    var ment = context.Message.MentionedUsers.First().Id;
                    var record = await _context.player_data.FindAsync(ment);
                    if (record == null)
                    {
                        await _profile.SendDm("Player mentioned in command wasn't found in database.");
                    }
                    else
                    {
                        await _profile.SendDm("Player's role was changed to " + role + ".");
                        record._status = role;
                        await _context.SaveChangesAsync();
                    }

                });
        }




    }
}
