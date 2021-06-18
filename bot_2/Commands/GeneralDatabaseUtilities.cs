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
                var team1 = await _context.game_record.FirstOrDefaultAsync(e => e._gameid == gameid && e._side == (int)Side.Team1);
                var team2 = await _context.game_record.FirstOrDefaultAsync(e => e._gameid == gameid && e._side == (int)Side.Team2);

                var team1Gain = team1._onwin;
                var team1Loss = team1._onlose;
                var team2Gain = team1._onlose;
                var team2Loss = team1._onwin;


                string dMen = await GetTeamMention(context, team2);
                string rMen = await GetTeamMention(context, team1);
                string cMen = await GetTeamMention(context, team1);
                string sMen = await GetTeamMention(context, team2);

                var largeBetList = await _context.game_bets.ToListAsync();
                var betList = largeBetList.FindAll(p => p._gameid == gameid);

                var team1Bets = betList.FindAll(p => p._side == (int)Side.Team1);
                int totalTeam1Bets = 0;
                foreach (var bet in team1Bets)
                {
                    totalTeam1Bets += bet._amount;
                }
                var team2Bets = betList.FindAll(p => p._side == (int)Side.Team2);
                int totalTeam2Bets = 0;
                foreach(var bet in team2Bets)
                {
                    totalTeam2Bets += bet._amount;
                }

                string team1Mention = "Team1 = \n" +
                    "Win: " + team1Gain + " mmr /// Lose: " + team1Loss + " mmr /// Coins bet: " + totalTeam1Bets +  " \n" + rMen;

                string team2Mention = "Team2 = \n" +
                    "Win: " + team2Gain + " mmr /// Lose: " + team2Loss + " mmr /// Coins bet: " + totalTeam2Bets + " \n" + dMen;

                string finalString = "" + starter + "Lobby host = " + hostMention + "\n\n" + team1Mention + team2Mention + "\n\n\n";


                completeString += finalString;
            }
            return completeString;

        }
    }

}
