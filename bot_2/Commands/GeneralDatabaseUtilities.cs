using db;
using DSharpPlus.CommandsNext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_2.Commands
{
    public class GeneralDatabaseInfo
    {
        private Context _context;

        public GeneralDatabaseInfo(Context context)
        {
            this._context = context;
        }
        public async Task<string> GetTeamMention(CommandContext context, TeamRecord record)
        {
            string fullString = "";

            fullString += await GetIndividualMention(context, record._p1);
            fullString += await GetIndividualMention(context, record._p2);
            fullString += await GetIndividualMention(context, record._p3);
            fullString += await GetIndividualMention(context, record._p4);
            fullString += await GetIndividualMention(context, record._p5);

            return fullString;
        }
        public async Task<string> GetIndividualMention(CommandContext context, ulong player)
        {
            string mention = "---" + "<@" + player + ">";

            var record = await _context.player_data.FindAsync(player);
            if (record != null)
            {
                mention += " / " + record._ihlmmr.ToString() + " GrinhouseMMR / " + record._dotammr.ToString() + " DotaMMR.";
            }
            mention += "\n";

            if (player == 0)
            {
                mention = "";
            }


            return mention;
        }
        public async Task<string> CreateGameProfile(CommandContext context, List<ChannelInfo> records)
        {
            string completeString = "";
            foreach (ChannelInfo record in records)
            {
                var gameid = record._gameid;

                var hostMention = "<not_found>";
                hostMention = "<@" + record._id + ">";



                string starter = "---Lobby" + gameid + "---\n";


                var gamerecord = await _context.game_data.FindAsync(gameid);
                if (gamerecord != null)
                {
                    if(gamerecord._start != null)
                    {
                        DateTimeOffset now = DateTimeOffset.Now;
                        TimeSpan span = now - gamerecord._start;
                        span = QueueInfo.StripMilliseconds(span);
                        starter += "--" + span + "--\n";
                    }

                }
                var radiant = await _context.game_record.FirstOrDefaultAsync(e => e._gameid == gameid && e._side == 0);
                var dire = await _context.game_record.FirstOrDefaultAsync(e => e._gameid == gameid && e._side == 1);

                var radiantGain = radiant._onwin;
                var radiantLoss = radiant._onlose;
                var direGain = radiant._onlose;
                var direLoss = radiant._onwin;


                string dMen = await GetTeamMention(context, dire);
                string rMen = await GetTeamMention(context, radiant);
                string cMen = await GetTeamMention(context, radiant);
                string sMen = await GetTeamMention(context, dire);

                var largeBetList = await _context.game_bets.ToListAsync();
                var betList = largeBetList.FindAll(p => p._gameid == gameid);

                var radiantBets = betList.FindAll(p => p._side == (int)Side.Radiant);
                int totalRadiantBets = 0;
                foreach (var bet in radiantBets)
                {
                    totalRadiantBets += bet._amount;
                }
                var direBets = betList.FindAll(p => p._side == (int)Side.Dire);
                int totalDireBets = 0;
                foreach(var bet in direBets)
                {
                    totalDireBets += bet._amount;
                }

                string radiantMention = "Radiant = \n" +
                    "Win: " + radiantGain + " mmr /// Lose: " + radiantLoss + " mmr /// Coins bet: " + totalRadiantBets +  " \n" + rMen;

                string direMention = "Dire = \n" +
                    "Win: " + direGain + " mmr /// Lose: " + direLoss + " mmr /// Coins bet: " + totalDireBets + " \n" + dMen;

                string finalString = "" + starter + "Lobby host = " + hostMention + "\n\n" + radiantMention + direMention + "\n\n\n";


                completeString += finalString;
            }
            return completeString;

        }
    }

}
