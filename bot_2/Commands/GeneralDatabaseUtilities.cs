using db;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_2.Commands
{
    public class GeneralDatabaseUtilities
    {
        private Context _context;

        public GeneralDatabaseUtilities(Context context)
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

                var radiant = _context.game_record.FirstOrDefault(e => e._gameid == gameid && e._side == 0);
                var dire = _context.game_record.FirstOrDefault(e => e._gameid == gameid && e._side == 1);

                var radiantGain = radiant._onwin;
                var radiantLoss = radiant._onlose;
                var direGain = radiant._onlose;
                var direLoss = radiant._onwin;


                string dMen = await GetTeamMention(context, dire);
                string rMen = await GetTeamMention(context, radiant);
                string cMen = await GetTeamMention(context, radiant);
                string sMen = await GetTeamMention(context, dire);

                string radiantMention = "Radiant = \n" +
                    "Win: " + radiantGain + " mmr /// Lose: " + radiantLoss + " mmr \n" + rMen;

                string direMention = "Dire = \n" +
                    "Win: " + direGain + " mmr /// Lose: " + direLoss + " mmr \n" + dMen;

                string finalString = "||" + starter + "Lobby host = " + hostMention + "\n\n" + radiantMention + direMention + "||\n\n\n";

                completeString += finalString;
            }
            return completeString;

        }
    }

}
