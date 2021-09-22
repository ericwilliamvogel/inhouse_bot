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
using static DSharpPlus.Entities.DiscordEmbedBuilder;

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
        //await _context.SaveChangesAsync();
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
                                //await _context.SaveChangesAsync();

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
                        //await _context.SaveChangesAsync();
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
                    //await _context.SaveChangesAsync();
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
                        //await _context.SaveChangesAsync();
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
                            var channel = await Bot._validator.Get(context, "game-history");
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
                    //await _context.SaveChangesAsync();

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

        [Command("testnewget")]
        public async Task Channelasdasdlear(CommandContext context, string channel)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole
                },

                async () =>
                {
                    var tempchannel = await Bot._validator.Get(context, channel);
                    await tempchannel.SendMessageAsync("Test");
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
                        //await _context.SaveChangesAsync();
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
                            //await _context.SaveChangesAsync();
                        }
                        else if (change.ToLower() == "subtract" || change.ToLower() == "decrease")
                        {
                            record._xp -= number;
                            //await _context.SaveChangesAsync();
                        }
                        await _profile.SendDm("Player's coins changed.");

                    }
                });


        }

        [Command("validatefiles")]
        public async Task SetMMr(CommandContext context)
        {

            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole
                },

                async () =>
                {
                    await Bot._validator.Validate(context);
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
                            //await _context.SaveChangesAsync();
                        }
                        else if(change.ToLower() == "subtract" || change.ToLower() == "decrease")
                        {
                            record._ihlmmr -= number;
                            //await _context.SaveChangesAsync();
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

        [Command("error")]
        public async Task reporterror(CommandContext context)
        {

            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsInAdminCommandChannel
                },

                async () =>
                {
                    await _profile.ReportError(context, "aoe");

                });


        }

        [Command("removerolefromall")]
        public async Task Geadfasasdasd12asdasd3dfasdfStu123ff(CommandContext context, string input)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole
                },

                async () =>
                {

                    var list = await _context.player_data.ToListAsync();

                    var role = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == input).Value;
                    foreach (var profile in list)
                    {
                        try
                        {
                            if (context.Guild.Members.ContainsKey(profile._id))
                            {
                                await context.Guild.Members[profile._id].RevokeRoleAsync(role);
                            }

                        }
                        catch
                        {

                        }
                    }

                });
        }

        [Command("grantroletoall")]
        public async Task Geadfasasdasd123dfasdasdfStu123ff(CommandContext context, string input)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole
                },

                async () =>
                {

                    var list = await _context.player_data.ToListAsync();

                    var role = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == input).Value;
                    foreach (var profile in list)
                    {
                        try
                        {
                            if (context.Guild.Members.ContainsKey(profile._id))
                            {
                                await context.Guild.Members[profile._id].GrantRoleAsync(role);
                            }

                        }
                        catch
                        {

                        }
                    }

                });
        }


        [Command("assignregistered")]
        public async Task Geadfasasdasd123dfasdfStu123ff(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole
                },

                async () =>
                {

                    var list = await _context.player_data.ToListAsync();

                    var role = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Registered").Value;
                    foreach (var profile in list)
                    {
                        try
                        {
                            if (context.Guild.Members.ContainsKey(profile._id))
                            {
                                await context.Guild.Members[profile._id].GrantRoleAsync(role);
                            }

                        }
                        catch
                        {

                        }
                    }

                });
        }


        [Command("vote")]
        public async Task Geadfas123dfasdfStu123ff(CommandContext context, params string[] strings)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole
                },

                async () =>
                {

                    string full = context.Message.Content.Substring(5);

                    DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                    .WithTitle("Vote")
                    .AddField("Topic", full, false)
                    .WithColor(DiscordColor.Gold);


                    builder.Footer = new EmbedFooter() { Text = "" };

                    DiscordEmbed embed = builder.Build();

                    var message = await context.Channel.SendMessageAsync("", false, embed);

                    await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));
                    await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":no_entry:"));


                });
        }

        [Command("genSetup")]
        public async Task GeadfasdfasdfStu123ff(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole
                },

                async () =>
                {
                    DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                    .WithTitle("Example")
                    .WithImageUrl("https://cdn.discordapp.com/attachments/889649751354671154/889650133740969994/unknown.png")
                    .AddField("Command", "!register 182944770", false)
                    .WithColor(DiscordColor.Gold);


                    builder.Footer = new EmbedFooter() { Text = "" };

                    DiscordEmbed embed = builder.Build();

                    await context.Channel.SendMessageAsync("", false, embed);

                    builder = new DiscordEmbedBuilder()
                    .WithTitle("Registration")
                    .AddField("Setup", "Find your *DOTA FRIEND ID*, then below use **!register youridhere** to register for this inhouse league.\n\nNote: This is required to play in the league. Do not use a fake ID.", false)
                    .WithColor(DiscordColor.Gold);

                    embed = builder.Build();

                    await context.Channel.SendMessageAsync("", false, embed);


                });
        }

        [Command("leaderboard")]
        public async Task GeadfasdfasdfStasdu123ff(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole
                },

                async () =>
                {
                    await _updatedQueue.UpdateLeaderboard(context);


                });
        }

        [Command("settingsSetup")]
        public async Task GeadfaasdsdfasdfStu123ff(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole
                },

                async () =>
                {
                    DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                        .WithTitle("Send Profile")
                        .WithImageUrl("http://cdn.shopify.com/s/files/1/0554/2386/0930/collections/Wraith_king_arcana.png?v=1616383799")
                        .AddField("React", ":white_check_mark: : Send me my profile.\n", false)
                        .WithColor(DiscordColor.Gold);


                    builder.Footer = new EmbedFooter() { Text = "" };

                    DiscordEmbed embed = builder.Build();

                    var message = await context.Channel.SendMessageAsync("", false, embed);

                    await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));


                    builder = new DiscordEmbedBuilder()
                    .WithTitle("Notifications")
                    .WithImageUrl("https://i.pinimg.com/originals/88/5d/c0/885dc039da5d39ff4b6fdd1c28a18bd2.gif")
                    .AddField("Settings", ":no_entry: : Turn off all inhouse notifications.\n" +
                    ":white_check_mark: Enable all inhouse notifications.", false)
                    .WithColor(DiscordColor.Gold);


                    builder.Footer = new EmbedFooter() { Text = "" };

                    embed = builder.Build();

                    message = await context.Channel.SendMessageAsync("", false, embed);

                    await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":white_check_mark:"));
                    await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":no_entry:"));

                });
        }


        [Command("embed")]
        public async Task GeadfasdfasdfStuff(CommandContext context)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.HasAdminRole
                },

                async () =>
                {
                    EmbedDriver driver = new EmbedDriver();
                    await driver.GeneratePositions(context);
                    await driver.GenerateFavPositions(context);
                    await driver.GenerateRegion(context);
                    await driver.GenerateRank(context);
                    await driver.GenerateRankProgress(context);



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
