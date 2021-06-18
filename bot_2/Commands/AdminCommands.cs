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
                    Arg.HasAdminRole
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

        [Command("sneakyopen")]
        public async Task SneakyOpen(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsInAdminCommandChannel,
                    Arg.HasAdminRole
                },

                async () =>
                {
                    TaskScheduler._inhouseOpen = true;
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
                        /*
                        await RemoveDiscordLobbyRecord(context, gameid);
                        await Clear(context);
                        await ResetAllStatus(context);
                        */
                        await _profile.SendDm("Only semifunctional. You can use other commands! :)");
                    }
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
                            var channel = context.Guild.Channels[Bot.Channels.GameHistoryChannel];
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
        public async Task QueueClear(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg>
                {
                     Arg.HasAdminRole
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
        public async Task ChannelClear(CommandContext context)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole
                },

                async () =>
                {
                    await DeleteLastMessage(context.Channel);
                    await _profile.SendDm("Channel cleared.");
                });

        }

        [Command("channelclear")]
        public async Task ChannelClear(CommandContext context, int number)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole
                },

                async () =>
                {
                    await DeleteLastMessage(context.Channel, number);
                    await _profile.SendDm("Channel cleared.");
                });

        }

        public async Task DeleteLastMessage(DiscordChannel channel)
        {
            var list = await channel.GetMessagesAsync(100);
            await channel.DeleteMessagesAsync(list);
        }


        public async Task DeleteLastMessage(DiscordChannel channel, int number)
        {
            var list = await channel.GetMessagesAsync(number);
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

        [Command("getprofile")]
        public async Task GetProfile (CommandContext context, string doesntmatter)
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

                    var record = await _context.player_data.FindAsync(ment);

                    if (record == null)
                    {
                        await _profile.SendDm("Player mentioned in command wasn't found in database.");
                    }
                    else
                    {
                        string playerinfo = "Player info : " + context.Message.MentionedUsers.First().Mention;
                        playerinfo += "\nFriendId : " + record._steamid;
                        playerinfo += "\nTotalGames : " + record._totalgames;
                        playerinfo += "\nW / L : " + record._gameswon + "/" + record._gameslost;
                        playerinfo += "\nDotaMmr : " + record._dotammr;
                        playerinfo += "\nGrinMmr : " + record._ihlmmr;
                        playerinfo += "\nRoles : " + record._role1 + "," + record._role2;
                        playerinfo += "\nRegion : " + record._region;
                        playerinfo += "\nCoins : " + record._xp;

                        await _profile.SendDm(playerinfo);
                    }
            });


        }

        [Command("playercoins")]
        public async Task SetCoins(CommandContext context, string change, int number, string doesntmatter)
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

                    var record = await _context.player_data.FindAsync(ment);

                    if (record == null)
                    {
                        await _profile.SendDm("Player mentioned in command wasn't found in database.");
                    }
                    else
                    {
                        if (change.ToLower() == "add" || change.ToLower() == "increase")
                        {
                            record._xp += number;
                            await _context.SaveChangesAsync();
                        }
                        else if (change.ToLower() == "subtract" || change.ToLower() == "decrease")
                        {
                            record._xp -= number;
                            await _context.SaveChangesAsync();
                        }
                        await _profile.SendDm("Player's coins changed.");

                    }
                });


        }


        [Command("playermmr")]
        public async Task SetMMr(CommandContext context, string change, int number, string doesntmatter)
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

                    var record = await _context.player_data.FindAsync(ment);

                    if (record == null)
                    {
                        await _profile.SendDm("Player mentioned in command wasn't found in database.");
                    }
                    else
                    {
                        if(change.ToLower() == "add" || change.ToLower() == "increase")
                        {
                            record._ihlmmr += number;
                            await _context.SaveChangesAsync();
                        }
                        else if(change.ToLower() == "subtract" || change.ToLower() == "decrease")
                        {
                            record._ihlmmr -= number;
                            await _context.SaveChangesAsync();
                        }
                        await _profile.SendDm("Player's mmr changed.");

                    }
                });


        }

        [Command("unlockqueue")]
        public async Task UnlockQueue(CommandContext context)
        {

            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsInAdminCommandChannel
                },

                async () =>
                {
                    await Task.Delay(1);
                    Conditions._locked = false;

                });


        }

        [Command("testlargeload")]
        public async Task tll(CommandContext context)
        {

            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsInAdminCommandChannel
                },

                async () =>
                {
                    Conditions._locked = true;
                    var players = await _context.player_queue.ToListAsync();
                    players = await _context.player_queue.ToListAsync();
                    players = await _context.player_queue.ToListAsync();
                    players = await _context.player_queue.ToListAsync();
                    players = await _context.player_queue.ToListAsync();
                    players = await _context.player_queue.ToListAsync();
                    players = await _context.player_queue.ToListAsync();
                    players = await _context.player_queue.ToListAsync();
                    players = await _context.player_queue.ToListAsync();
                    players = await _context.player_queue.ToListAsync();
                    players = await _context.player_queue.ToListAsync();
                    players = await _context.player_queue.ToListAsync();
                    await Task.Delay(10000);
                    Conditions._locked = false;

                });


        }


    }
}
