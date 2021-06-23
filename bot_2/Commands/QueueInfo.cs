using db;
using DSharpPlus.CommandsNext;
using DSharpPlus.Exceptions;
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
    public class QueueInfo
    {
        Context _context;
        public QueueInfo(Context context)
        {
            this._context = context;
        }
        private string stringending = "----------";

        MmrCalculator calculator = new MmrCalculator();
        public async Task<string> GetPlayerQueueInfo(CommandContext context)
        {
            var playersInQueue = await _context.player_queue.ToListAsync();
            var count = playersInQueue.Count;
            string start = GetStringStart("Players", count);

            List<QueueData> recordsToBeRemoved = new List<QueueData>();

            var players = "";
            int counter = 0;
            foreach (var player in playersInQueue)
            {
                counter++;
                for (int i = 0; i < counter; i++)
                {
                    players += "-";
                }
                var startTime = player._start;
                players += await DisplayData(context, player._id, startTime,
                    (TimeSpan timespan) => {
                        if (timespan.Minutes >= 45)
                        {
                            recordsToBeRemoved.Add(player);
                        }
                    });


            }


            string queueInfo = start + players + stringending;

            await RemoveIdlePlayersFromQueue(context, recordsToBeRemoved);

            return queueInfo;

        }

        public async Task<string> GetSpectatorQueueInfo(CommandContext context)
        {

            var playersInQueue = await _context.spectator_queue.ToListAsync();
            var count = playersInQueue.Count;
            var players = "";

            string start = GetStringStart("Spectators", count);

            foreach (var player in playersInQueue)
            {
                var startTime = player._start;
                players += await DisplayData(context, player._id, startTime,
                    (TimeSpan timespan) => {
                        //spectators not removed from queue
                    });
            }


            string queueInfo = start + players + stringending;
            return queueInfo;

        }

        public async Task<string> GetCasterQueueInfo(CommandContext context)
        {
            var playersInQueue = await _context.caster_queue.ToListAsync();
            var count = playersInQueue.Count;

            string start = GetStringStart("Casters", count);

            var players = "";
            foreach (var player in playersInQueue)
            {
                var startTime = player._start;
                players += await DisplayData(context, player._id, startTime,
                    (TimeSpan timespan) => {
                        //casters not removed from queue
                    });
            }


            string queueInfo = start + players + stringending;
            return queueInfo;

        }

        public static TimeSpan StripMilliseconds(TimeSpan time)
        {
            return new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds);
        }
        public async Task<string> DisplayData(CommandContext context, ulong id, DateTimeOffset playerStart, Action<TimeSpan> RemoveIdleRecords) //we need to use an action here in case we want to have queue timeouts in the future for the different queue variants, not just players
        {
            string playerList = "";
            DateTimeOffset end = DateTimeOffset.Now;


            var playerMMR = await _context.player_data.FindAsync(id);

            string mmr = "<mmr_not_found>";



            string othermmr = mmr;

            if (playerMMR != null)
            {
                mmr = playerMMR._ihlmmr.ToString();
                othermmr = playerMMR._dotammr.ToString();
            }

            //if (context.Guild.Members.ContainsKey(id))
                //othermmr = calculator.GetMMR(context, context.Guild.Members[id]).ToString();

            if (playerStart != null)
            {
                DateTimeOffset start = playerStart;
                TimeSpan timespan = end - start;
                timespan = QueueInfo.StripMilliseconds(timespan);

                playerList += "<@" + id + ">" + " : " + timespan + " -- " + mmr + " inhouse mmr / " + othermmr + " dota mmr.\n";

                RemoveIdleRecords(timespan);

            }
            else
            {
                playerList += "<@" + id + ">" + " :  -- " + mmr + " inhouse mmr / " + othermmr + " dota mmr.\n";
            }

            return playerList;
        }

        public async Task<string> GetLeaderboard()
        {

            string leaderboard = "---Leaderboard---\n";
            var list = await _context.player_data.ToListAsync();
            var orderedList = list.OrderByDescending(p => p._ihlmmr).ToList();
            if (orderedList.Count >= 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    int rank = i + 1;
                    leaderboard += "#" + rank + "- <@" + orderedList[i]._id + ">, " + orderedList[i]._ihlmmr + " inhouse mmr\n";
                }
            }
            leaderboard += "---------------------\n\n";

            return leaderboard;
        }

        public async Task<string> GetParticipationAward()
        {
            string participationAward = "---Participation Award---\n";
            var list = await _context.player_data.ToListAsync();
            var orderedList = list.OrderBy(p => p._ihlmmr).ToList();

            int rank = orderedList.Count;
            participationAward += "#" + rank + "- <@" + orderedList[0]._id + ">, " + orderedList[0]._ihlmmr + " inhouse mmr\n";

            participationAward += "---------------------\n\n";

            return participationAward;
        }
        private string GetStringStart(string input, int count)
        {
            string start = "----------\n**" + input + " queueing: " + count + "**\n";
            return start;
        }

        private async Task RemoveIdlePlayersFromQueue(CommandContext context, List<QueueData> recordsToBeRemoved)
        {
            foreach (var player in recordsToBeRemoved)
            {
                _context.player_queue.Remove(player);
                await _context.SaveChangesAsync();

                Profile profile = new Profile(context, player._id);
                var member = await _context.player_data.FindAsync(player._id);
                if (member != null)
                {
                    member._gamestatus = 0;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    await profile.SendDm("For some reason we couldn't locate your profile using your discord id. Your status still hasn't been reset. You may need to #create-a-ticket so an admin can fix your status.");
                }
                await profile.SendDm("You have been removed from queue because you've been idle for 45 minutes. Type !q or !queue if you'd like to return to queue.");

            }
        }
    }
}
