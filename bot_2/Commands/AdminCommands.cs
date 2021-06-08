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

        [Command("createreact")]
        public async Task CreateReact(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole
                },

                async () =>
                {
                    var message = await context.Channel.SendMessageAsync("<@&775151515442741258>\n---React if you're down to play today---\n\n");
                    await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":thumbsup:"));
                    /*var message = await context.Channel.SendMessageAsync("2PM EST - 5PM EST\n");
                    await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":thumbsup:"));
                    //await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":thumbsdown:"));

                    var message1 = await context.Channel.SendMessageAsync("5PM EST - 8PM EST\n");
                    await message1.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":thumbsup:"));
                    //await message1.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":thumbsdown:"));

                    var message2 = await context.Channel.SendMessageAsync("8PM EST - 11PM EST\n");
                    await message2.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":thumbsup:"));
                    //await message2.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":thumbsdown:"));

                    var message3 = await context.Channel.SendMessageAsync("11PM EST - 2AM EST\n");
                    await message3.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":thumbsup:"));
                    //await message3.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":thumbsdown:"));*/

                });
        }

        [Command("grantemotetoallmembers")]
        public async Task GrantEmotePower(CommandContext context, int emoteType)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole,
                    Arg.HasMention
                },

                async () =>
                {
                    var records = await _context.player_data.ToListAsync();
                    foreach(var player in records)
                    {
                        var record = await _context.emote_unlocked.FirstOrDefaultAsync(p => p._playerid == player._id && p._emoteid == emoteType);
                        if (record == null)
                        {
                            await _context.emote_unlocked.AddAsync(new EmoteUnlockedData { _playerid = player._id, _emoteid = emoteType });
                            await _context.SaveChangesAsync();
                        }
                    }

                    await _profile.SendDm("Emote powers granted to all users in the database.");

                });
        }

        [Command("grantemote")]
        public async Task GrantEmotePower(CommandContext context, int emoteType, string doesntmatter)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole,
                    Arg.HasMention
                },

                async () =>
                {
                    var ment = context.Message.MentionedUsers.First().Id;

                    var record = await _context.emote_unlocked.FirstOrDefaultAsync(p => p._playerid == ment && p._emoteid == emoteType);
                    if (record == null)
                    {

                        await _profile.SendDm("Emote powers granted.");
                        await _context.emote_unlocked.AddAsync(new EmoteUnlockedData { _playerid = ment, _emoteid = emoteType });
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        await _profile.SendDm("Player already has that emote.");
                    }

                });
        }

        [Command("manuallyresetallstatus")]
        public async Task ResetAllStatus(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsInAdminCommandChannel,
                    Arg.HasAdminRole
                },

                async () =>
                {
                    var table = await _context.player_data.ToListAsync();
                    foreach (var record in table)
                    {
                        record._gamestatus = 0;
                    }
                    await _context.SaveChangesAsync();
                });
        }

        [Command("removelobbyrecord")]
        public async Task RemoveDiscordLobbyRecord(CommandContext context, int gameid)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsInAdminCommandChannel,
                    Arg.HasAdminRole
                },

                async () =>
                {
                    var record = await _context.discord_channel_info.FirstOrDefaultAsync(p => p._gameid == gameid);
                    if(record == null)
                    {
                        await _profile.SendDm("Record wasn't found under that game id.");
                    }
                    else
                    {
                        _context.discord_channel_info.Remove(record);
                        await _context.SaveChangesAsync();
                    }
                });
        }

        [Command("lobbykill")]
        public async Task HardReset(CommandContext context, int gameid)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsInAdminCommandChannel,
                    Arg.HasAdminRole
                },

                async () =>
                {
                    var record = await _context.discord_channel_info.FindAsync(gameid);
                    if (record == null)
                    {
                        await _profile.SendDm("Record wasn't found under that game id.");
                    }
                    else
                    {
                        await RemoveDiscordLobbyRecord(context, gameid);
                        await Clear(context);
                        await ResetAllStatus(context);

                        await _profile.SendDm("Full reset complete.");
                    }
                });
        }

        [Command("manuallyrestartthread")]
        public async Task RestartThread(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsInAdminCommandChannel,
                    Arg.HasAdminRole
                },

                async () =>
                {
                    await UpdatedQueue.ResetVariables();
                    await _updatedQueue.StartThread(context);
                });
        }

        [Command("grabgame")]
        public async Task RestartThread(CommandContext context, long id)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsInAdminCommandChannel,
                    Arg.HasAdminRole
                },

                async () =>
                {
                    var openDota = new OpenDotaApi();
                    try
                    {
                        var gameDetails = await openDota.Matches.GetMatchByIdAsync(id);
                        if (gameDetails != null)
                        {
                            LobbySorter sorter = new LobbySorter(_context);
                            string gHistory = await sorter._info.GetGameHistory(openDota, gameDetails);
                            var channel = context.Guild.Channels[842870150994591764];
                            await channel.SendMessageAsync(gHistory);
                        }
                        else
                        {
                            await _profile.SendDm("That game key was not recognized, the game was not stored in our history. Open a ticket and include the GrinHouse lobby # and the true game id. If you don't know where to find it feel free to take a screenshot of the match endscreen in the dota client.");
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
        }
 
        [Command("testid")]
        public async Task TestID(CommandContext context, long steamid)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsInAdminCommandChannel
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

                new List<Arg> {
                    Arg.HasAdminRole
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

                new List<Arg>
                {
                    Arg.IsInAdminCommandChannel
                },

                async () =>
                {
                    LobbySorter sorter = new LobbySorter(_context);
                    await sorter._report.CloseLobby(context, number);

                });



        }

        [Command("queueclear")]
        public async Task Clear(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg>
                {
                     Arg.IsInAdminCommandChannel
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

                new List<Arg> {
                    Arg.IsInAdminCommandChannel
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

                new List<Arg> {
                    Arg.IsInAdminCommandChannel
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

                new List<Arg> {
                    Arg.IsInAdminCommandChannel
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
