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

namespace bot_2.Commands
{

    public delegate Task<bool> Argument(CommandContext context, Profile profile);

    
    public class StandardCommands : BaseCommandModule
    {
        private Context _context;
        private Conditions _conditions;
        bool started = false;
        public ulong PreLoadedChannel { get; private set; }
        public ulong PreLoadedMessage { get; private set; }
        public StandardCommands(Context context)
        {
            _context = context;
            _conditions = new Conditions(context);

            var json = string.Empty;

            using (var fs = File.OpenRead("channelConfig.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();

            var channelConfigJson = JsonConvert.DeserializeObject<ChannelConfigJson>(json);

            PreLoadedChannel = channelConfigJson.Channel;
            PreLoadedMessage = channelConfigJson.Message;
        }

        private TimeSpan StripMilliseconds(TimeSpan time)
        {
            return new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds);
        }

        private async Task<string> GetTeamMention(CommandContext context, TeamRecord record)
        {
            string fullString = "";

            fullString += await GetIndividualMention(context, record._p1);
            fullString += await GetIndividualMention(context, record._p2);
            fullString += await GetIndividualMention(context, record._p3);
            fullString += await GetIndividualMention(context, record._p4);
            fullString += await GetIndividualMention(context, record._p5);

            return fullString;
        }
        private async Task<string> GetIndividualMention(CommandContext context, ulong player)
        {
            string mention = "---" + context.Guild.Members[player].Mention;

            var record = await _context.player_data.FindAsync(player);
            if (record != null)
            {
                mention += " / " + record._ihlmmr.ToString() + " GrinhouseMMR";
            }
            mention += "\n";
            return mention;
        }
        private async Task<string> CreateGameProfile(CommandContext context, List<ChannelInfo> records)
        {
            string completeString = "";
            foreach (ChannelInfo record in records)
            {
                var gameid = record._gameid;
                var hostMention = context.Guild.Members[record._id].Mention;

                string starter = "---Lobby" + gameid + "---\n";

                var radiant = _context.game_record.FirstOrDefault(e => e._gameid == gameid && e._side == 0);
                var dire = _context.game_record.FirstOrDefault(e => e._gameid == gameid && e._side == 1);

                var radiantGain = radiant._onwin;
                var radiantLoss = radiant._onlose;
                var direGain = radiant._onlose;
                var direLoss = radiant._onwin;


                string dMen = await GetTeamMention(context, dire);
                string rMen = await GetTeamMention(context, radiant);

                string radiantMention = "Radiant = \n" +
                    "Win: " + radiantGain + " mmr /// Lose: " + radiantLoss + " mmr \n" + rMen;

                string direMention = "Dire = \n" +
                    "Win: " + direGain + " mmr /// Lose: " + direLoss + " mmr \n" + dMen;

                string finalString = "||" + starter + "Lobby host = " + hostMention + "\n\n" + radiantMention + direMention + "||\n\n\n";

                completeString += finalString;
            }
            return completeString;

        }
        private async Task UpdateMessage(CommandContext context)
        {

            if (context.Guild.Channels.ContainsKey(PreLoadedChannel))
            {
                Console.WriteLine("channel loaded");
            }
            else
            {
                Console.WriteLine("badkey channel");
                return;
            }

            var channel = context.Guild.Channels[PreLoadedChannel];
            try
            {
                var message = await context.Guild.Channels[PreLoadedChannel].GetMessageAsync(PreLoadedMessage);
                if (message == null)
                {
                    Console.WriteLine("was null");
                    return;
                }

                var playersInQueue = await _context.player_queue.ToListAsync();
                var gamesBeingPlayed = await _context.discord_channel_info.ToListAsync();
                string players = "----------\nPlayers queueing: \n";
                DateTime end = DateTime.Now;
                foreach (var player in playersInQueue)
                {
                    var playerMMR = await _context.player_data.FindAsync(player._id);
                    string mmr = "<mmr_not_found>";
                    if(playerMMR != null)
                    {
                        mmr = playerMMR._ihlmmr.ToString();
                    }
                    DateTime start = Convert.ToDateTime(player._start);
                    TimeSpan timespan = end - start;
                    timespan = StripMilliseconds(timespan);
                    if(context.Guild.Members.ContainsKey(player._id))
                    {
                        players += context.Guild.Members[player._id].Mention + " : " + timespan + " -- " + mmr +  " inhouse mmr.\n";
                    }
                    else
                    {
                        players += "Unknown player" + " : " + timespan + " -- " + " inhouse mmr.\n";
                    }

                }

                players += "----------";

                string currentGames = await CreateGameProfile(context, gamesBeingPlayed);
                string finalString = DateTime.Now.ToString() + "\nWelcome to GrinHouseLeague. There are **" + playersInQueue.Count + " players** in queue and **" + gamesBeingPlayed.Count + " games** currently being played.\n\n" + players + "\n\n" + currentGames;



                await message.ModifyAsync(finalString);

                await Task.Delay(1000);

                await UpdateMessage(context);

                Console.WriteLine("trying");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
        [Command("getid")]
        public async Task GetID(CommandContext context)
        {
            await context.Channel.SendMessageAsync(context.Channel.Id.ToString());
            await context.Channel.SendMessageAsync(context.Channel.GuildId.ToString());
        }
        [Command("close")]
        public async Task Close(CommandContext context, int number)
        {
            Sorter sorter = new Sorter(_context);
            await sorter.CloseLobby(context, number);
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

            var verified = await _conditions.AreMet(context, _profile,
                new List<Argument> {
                                _conditions.IsRegistered,
                                _conditions.IsInCommandChannel,
                                _conditions.IsQueued
                });

            if (!verified)
            {
                await context.Message.DeleteAsync();
                return;
            }


            var record = _context.player_queue.First(p => p._id == _profile._id);
            if (record == null)
            {
                await _profile.SendDm("Error, I have no idea how this could happen. DM an admin or something to get it fixed.");
            }

            _context.player_queue.Remove(record);
            await _context.SaveChangesAsync();
            await _profile.SendDm("You've been removed from queue.");
            await context.Message.DeleteAsync();
        }
        


        [Command("radiant")]
        public async Task RadiantWin(CommandContext context, long steamid)
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

            Sorter sorter = new Sorter(_context);
            await sorter.ReportWinner(context, "radiant", steamid);
            await context.Message.DeleteAsync();
        }

        [Command("dire")]
        public async Task DireWin(CommandContext context, long steamid)
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

            Sorter sorter = new Sorter(_context);
            await sorter.ReportWinner(context, "dire", steamid);
            await context.Message.DeleteAsync();
        }

        [Command("draw")]
        public async Task DrawGame(CommandContext context, long steamid)
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

            Sorter sorter = new Sorter(_context);
            await sorter.ReportWinner(context, "draw", steamid);
            await context.Message.DeleteAsync();
        }
        [Command("role")]
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

        [Command("q")]
        public async Task q(CommandContext context)
        {
            await EnterQueue(context);
        }


        [Command("mmr")]
        public async Task ShowMMR(CommandContext context)
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


            var player = await _context.player_data.FindAsync(_profile._id);

            await _profile.SendDm("Your GHL mmr is " + player._ihlmmr);
            await _profile.SendDm("You've won " + player._gameswon + " games.");
            await _profile.SendDm("You've lost " + player._gameslost + " games.");
            await context.Message.DeleteAsync();
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

            var verifiedagain = await _conditions.AreMet(context, _profile,
                new List<Argument> {
                        _conditions.IsReady,
                       _conditions.IsntQueued
                });

            if (!verifiedagain)
            {
                await context.Message.DeleteAsync();
                return;
            }


            try
            {

                string dt = DateTime.Now.ToString();
                await _context.player_queue.AddAsync(new QueueData { _id = _profile._id, _start = dt }).ConfigureAwait(false);
                await _context.SaveChangesAsync().ConfigureAwait(false);


                var list = await _context.player_queue.ToListAsync();
                var count = list.Count();


                //if doesnt work just put it after the setup
                if (!started)
                {
                    started = true;
                    Task task = await Task.Factory.StartNew(async () =>
                    {
                        await UpdateMessage(context);

                    }, TaskCreationOptions.LongRunning);

                }

                await context.Message.DeleteAsync();
                if (count >= 10)
                {
                    Sorter sorter = new Sorter(_context);
                    await sorter.Setup(context);
                }
                else
                {
                    await _profile.SendDm("You've been placed in queue. You will be notified via DM when it pops. In the server's command channel type !leave if you would like to leave the queue.");

                }

            }
            catch (Exception e)
            {
                await _profile.ReportError(e);
            }




        }

        [Command("register")]
        public async Task RegisterPlayer(CommandContext context, string steamid)
        {
            Profile _profile = new Profile(context);
            var verified = await _conditions.AreMet(context, _profile,
                new List<Argument> {
                    _conditions.IsntRegistered,
                    _conditions.IsInCommandChannel
                });

            if (!verified)
            {
                await context.Message.DeleteAsync();
                return;
            }

            await _context.player_data.AddAsync(new PlayerData { _id = _profile._id, _steamid = Convert.ToInt64(steamid), _status = 1, _ihlmmr = 400 }).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            await _profile.SendDm("You are now registered. Type !queue to enter the queue and !leave to leave the queue. Your GHL mmr starts at 400 and has a base increment of 15.").ConfigureAwait(false);
            await context.Message.DeleteAsync();
        }


        [Command("clear")]
        public async Task Clear(CommandContext context)
        {
            Profile _profile = new Profile(context);
            var verified = await _conditions.AreMet(context, _profile,
                new List<Argument> {
                    _conditions.IsInAdminCommandChannel
                });

            if (!verified)
            {
                await context.Message.DeleteAsync();
                return;
            }

            var queue = await _context.player_queue.ToListAsync();
            foreach(QueueData record in queue)
            {
                _context.player_queue.Remove(record);
            }
            await _context.SaveChangesAsync();
            await _profile.SendDm("The queue has been cleared.").ConfigureAwait(false);
            await context.Message.DeleteAsync();
        }

        [Command("clear_channel")]
        public async Task Clear(CommandContext context, ulong input)
        {
            Profile _profile = new Profile(context);
            var verified = await _conditions.AreMet(context, _profile,
                new List<Argument> {
                    _conditions.IsInAdminCommandChannel
                });

            if (!verified)
            {
                await context.Message.DeleteAsync();
                return;
            }

            if(context.Guild.Channels.ContainsKey(input))
            {
                /*var channel = context.Guild.Channels[input];
                foreach(DiscordMessage message in channel.Messages)
                {
                    await message.DeleteAsync();
                }*/
            }
            else
            {
                await context.Channel.SendMessageAsync("Channel not recognized under the id you typed.").ConfigureAwait(false);
            }




            await context.Message.DeleteAsync();
        }
    }
}
