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
        }
        public async Task UpdateMessage(CommandContext context)
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

                    /*
                    if (context.Guild.Members.ContainsKey(player._id))
                    {
                        
                    }
                    else
                    {
                        players += "Unknown player" + " : " + timespan + " -- " + " inhouse mmr.\n";
                    }*/

                }

                players += "----------";

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

                string finalString = DateTime.Now.ToString() + "\nWelcome to GrinHouseLeague. There are **" + playersInQueue.Count + " players** in queue and **" + gamesBeingPlayed.Count + " games** currently being played.\n\n" + leaderboard + players + "\n\n" + currentGames;



                await message.ModifyAsync(finalString);

                await Task.Delay(2000);

                await UpdateMessage(context);

                Console.WriteLine("trying");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

    }
}
