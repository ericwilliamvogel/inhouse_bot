using db;
using DSharpPlus.CommandsNext;
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
    public class UpdatedQueue
    {
        Context _context;
        GeneralDatabaseUtilities _utilities;
        static bool started = false;
        public ulong PreLoadedChannel { get; private set; }
        public ulong PreLoadedMessage { get; private set; }
        public UpdatedQueue(Context context)
        {
            this._context = context;
            _utilities = new GeneralDatabaseUtilities(context);
            ReadJsonFile();
        }

        private TimeSpan StripMilliseconds(TimeSpan time)
        {
            return new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds);
        }

        public static async Task ResetVariables()
        {
            started = false;
            await Task.CompletedTask;
        }
        private void ReadJsonFile()
        {
            var json = string.Empty;

            using (var fs = File.OpenRead("channelConfig.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();

            var channelConfigJson = JsonConvert.DeserializeObject<ChannelConfigJson>(json);
            PreLoadedChannel = channelConfigJson.Channel;
            PreLoadedMessage = channelConfigJson.Message;
        }

        public async Task StartThread(CommandContext context)
        {
            if (!started)
            {
                started = true;
                Task task = await Task.Factory.StartNew(async () =>
                {
                   await UpdateMessage(context);

                }, TaskCreationOptions.LongRunning);


            }
            /*else
            {
                Profile profile = new Profile(context);
                profile.SendDm("Queue thread has already been started. If it's still not moving for you, create a ticket and an admin can manually reset it.");
            }*/
        }
        public async Task UpdateMessage(CommandContext context)
        {


            try
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

                var message = await context.Guild.Channels[PreLoadedChannel].GetMessageAsync(PreLoadedMessage);
                if (message == null)
                {
                    Console.WriteLine("was null");
                    return;
                }

                var playersInQueue = await _context.player_queue.ToListAsync();
                var castersInQueue = await _context.caster_queue.ToListAsync();
                var spectatorsInQueue = await _context.spectator_queue.ToListAsync();
                var gamesBeingPlayed = await _context.discord_channel_info.ToListAsync();

                string players = "----------\nPlayers queueing: \n";

                string casters = "----------\nCasters queueing: \n";

                string spectators = "----------\nSpectators queueing: \n";

                string stringend = "----------";
                //string availableplayers = "----------\nPlayers afk/online: \n";
                /*var playersNotInQueue = await _context.player_data.ToListAsync();
                var x = playersNotInQueue.FindAll(p => p._gamestatus == 0);
                foreach(var player in playersNotInQueue)
                {
                    var playerMMR = await _context.player_data.FindAsync(player._id);
                    string mmr = "<mmr_not_found>";
                    string othermmr = mmr;
                    if (playerMMR != null)
                    {
                        mmr = playerMMR._ihlmmr.ToString();
                        othermmr = playerMMR._dotammr.ToString();
                    }
                    availableplayers += "<@" + player._id + "> -- " + mmr + " inhouse mmr / " + othermmr + " dota mmr.\n";
                }*/
                DateTime end = DateTime.Now;
                foreach (var player in playersInQueue)
                {
                    var playerMMR = await _context.player_data.FindAsync(player._id);
                    string mmr = "<mmr_not_found>";
                    string othermmr = mmr;
                    if (playerMMR != null)
                    {
                        mmr = playerMMR._ihlmmr.ToString();
                        othermmr = playerMMR._dotammr.ToString();
                    }
                    DateTime start = Convert.ToDateTime(player._start);
                    TimeSpan timespan = end - start;
                    timespan = StripMilliseconds(timespan);

                    players += "<@" + player._id + ">" + " : " + timespan + " -- " + mmr + " inhouse mmr / " + othermmr + " dota mmr.\n";

                }

                foreach (var caster in castersInQueue)
                {
                    var playerMMR = await _context.player_data.FindAsync(caster._id);
                    string mmr = "<mmr_not_found>";
                    string othermmr = mmr;
                    if (playerMMR != null)
                    {
                        mmr = playerMMR._ihlmmr.ToString();
                        othermmr = playerMMR._dotammr.ToString();
                    }
                    DateTime start = Convert.ToDateTime(caster._start);
                    TimeSpan timespan = end - start; 
                    timespan = StripMilliseconds(timespan);

                    casters += "<@" + caster._id + ">" + " : " + timespan + "\n";

                }


                foreach (var spectator in spectatorsInQueue)
                {
                    var playerMMR = await _context.player_data.FindAsync(spectator._id);
                    string mmr = "<mmr_not_found>";
                    string othermmr = mmr;
                    if (playerMMR != null)
                    {
                        mmr = playerMMR._ihlmmr.ToString();
                        othermmr = playerMMR._dotammr.ToString();
                    }
                    DateTime start = Convert.ToDateTime(spectator._start);
                    TimeSpan timespan = end - start;
                    timespan = StripMilliseconds(timespan);

                    spectators += "<@" + spectator._id + ">" + " : " + timespan + " -- " + mmr + " inhouse mmr / " + othermmr + " dota mmr.\n";

                }

                players += stringend;
                casters += stringend;
                spectators += stringend;

                string currentGames = await _utilities.CreateGameProfile(context, gamesBeingPlayed);


                string leaderboard = "---Leaderboard---\n";
                var list = await _context.player_data.ToListAsync();
                var orderedlist = list.OrderByDescending(p => p._ihlmmr).ToList();
                if (orderedlist.Count >= 3)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        int rank = i + 1;
                        leaderboard += "#" + rank + "- <@" + orderedlist[i]._id + ">, " + orderedlist[i]._ihlmmr + " inhouse mmr\n";
                    }
                }
                leaderboard += "---------------------\n\n";


                string finalString = DateTime.Now.ToString() + "\nWelcome to GrinHouseLeague. There are **" + playersInQueue.Count + " players** in queue and **" + gamesBeingPlayed.Count + " games** currently being played.\n\n" + leaderboard +
                    players +
                    "\n\n" +
                    casters +
                    "\n\n" +
                    spectators +
                    "\n\n" +
                    //availableplayers +
                    //"\n\n" +
                    currentGames;



                await message.ModifyAsync(finalString);

                await Task.Delay(2000);

                await UpdateMessage(context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

    }
}
