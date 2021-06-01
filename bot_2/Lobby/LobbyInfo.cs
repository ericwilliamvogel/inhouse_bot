using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using db;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OpenDotaDotNet;

namespace bot_2.Commands
{
    public class LobbyInfo
    {
        private LobbyUtilities _utilities;
        private Context _context;
        public LobbyInfo(Context context, LobbyUtilities utilities)
        {
            _context = context;
            _utilities = utilities;
        }

        public async Task<string> GetGameHistory(OpenDotaApi openDota, OpenDotaDotNet.Models.Matches.Match gameDetails)
        {
            string direscore = "Dire Score: " + gameDetails.DireScore + " / ";
            string radiantscore = "Radiant Score: " + gameDetails.RadiantScore + "\n";

            string starttime = "Game Started: " + gameDetails.StartTime + " / ";
            string duration = "Game Length: " + gameDetails.Duration + "\n";


            string region = "Region: " + gameDetails.Region + "\n";
            string winner = "<not_found>";

            if (gameDetails.RadiantWin)
            {
                winner = "Winner: Radiant\n";
            }
            else
            {
                winner = "Winner: Dire\n";

            }


            string dire = "Dire: \n";
            string radiant = "Radiant: \n";

            var heroes = await openDota.Heroes.GetHeroesAsync();

            foreach (var player in gameDetails.Players)
            {
                string info = string.Empty;
                var id = player.AccountId;
                var record = await _context.player_data.FirstOrDefaultAsync(p => p._steamid == id);


                long kills = player.Kills;
                int deaths = player.Deaths;
                long assists = player.Assists;
                int gpm = player.GoldPerMin;
                int xpm = player.XpPerMin;

                int lasthits = player.LastHits;
                int denies = player.Denies;

                long heroid = player.HeroId;

                var hero = heroes.FirstOrDefault(p => p.Id == heroid).LocalizedName;

                long dmg = player.HeroDamage;

                if (record != null)
                {
                    info += "<@" + record._id + ">, Hero: " + hero + "\n - K/D/A: " + kills + "/" + deaths + "/" + assists;
                }
                else
                {
                    info += "<unkownplayer>, Hero: " + hero + "\n - K/D/A: " + kills + "/" + deaths + "/" + assists;
                }

                info += " - GPM/XPM: " + gpm + "/" + xpm + " - \n - LH/D: " + lasthits + "/" + denies + " - Dmg: " + dmg + "-\n";
                if (player.IsRadiant)
                {
                    radiant += info;
                }
                else
                {
                    dire += info;
                }
            }

            string final = "\n\n" + winner + direscore + radiantscore + starttime + duration +
    region + radiant + dire + "\n\n";

            return final;
        }


        public async Task<string> GetGameHistory(OpenDotaApi openDota, int gameid, OpenDotaDotNet.Models.Matches.Match gameDetails)
        {
            var game = await _context.game_data.FindAsync(gameid);
            var steamid = game._steamid;
            var ihlid = game._id;


            string gametitle = "--GrinHouseIHL Lobby " + ihlid + "--\n";
            string replaytitle = "--ReplayID :" + steamid + "--\n";
            string direscore = "Dire Score: " + gameDetails.DireScore + " / ";
            string radiantscore = "Radiant Score: " + gameDetails.RadiantScore + "\n";

            string starttime = "Game Started: " + gameDetails.StartTime + " / ";
            string duration = "Game Length: " + gameDetails.Duration + "\n";


            string region = "Region: " + gameDetails.Region + "\n";
            string winner = "<not_found>";

            if (gameDetails.RadiantWin)
            {
                winner = "Winner: Radiant\n";
            }
            else
            {
                winner = "Winner: Dire\n";

            }


            string dire = "Dire: \n";
            string radiant = "Radiant: \n";

            var heroes = await openDota.Heroes.GetHeroesAsync();

            foreach (var player in gameDetails.Players)
            {
                string info = string.Empty;
                var id = player.AccountId;
                var record = await _context.player_data.FirstOrDefaultAsync(p => p._steamid == id);


                long kills = player.Kills;
                int deaths = player.Deaths;
                long assists = player.Assists;
                int gpm = player.GoldPerMin;
                int xpm = player.XpPerMin;

                int lasthits = player.LastHits;
                int denies = player.Denies;

                long heroid = player.HeroId;

                var hero = heroes.FirstOrDefault(p => p.Id == heroid).LocalizedName;

                long dmg = player.HeroDamage;

                if (record != null)
                {
                    info += "<@" + record._id + ">, Hero: " + hero + "\n - K/D/A: " + kills + "/" + deaths + "/" + assists;
                }
                else
                {
                    info += "<unkownplayer>, Hero: " + hero + "\n - K/D/A: " + kills + "/" + deaths + "/" + assists;
                }

                info += " - GPM/XPM: " + gpm + "/" + xpm + " - \n - LH/D: " + lasthits + "/" + denies + " - Dmg: " + dmg + "-\n";
                if (player.IsRadiant)
                {
                    radiant += info;
                }
                else
                {
                    dire += info;
                }
            }

            string final = "\n\n" + gametitle + replaytitle + "\n\n" + winner + direscore + radiantscore + starttime + duration +
    region + radiant + dire + "\n\n";

            return final;
        }

        public string GetFullLobbyInfo(CommandContext context, List<Player> team1, List<Player> team2, int gameid)
        {
            var game = _context.discord_channel_info.FirstOrDefault(p => p._gameid == gameid);
            ulong leaderid = game._id;
            int basemmr = 15;

            //PLEASE JUST READ THIS FROM DATABASE LATER!!!

            int team1mmr = _utilities.GetTeamTrueMmr(team1);
            int team2mmr = _utilities.GetTeamTrueMmr(team2);

            int team1gain = basemmr * team2mmr / team1mmr;

            int team1loss = basemmr * team1mmr / team2mmr;

            Region region = GetRegion(team1, team2);

            string hostMention = "<host_not_found>";
            if (context.Guild.Members.ContainsKey(leaderid))
            {
                var host = context.Guild.Members[leaderid];
                hostMention = host.Mention;
            }

            string radiantMention = "Radiant = \n" +
                "Win: " + team1gain + " mmr /// Lose: " + team1loss + " mmr \n" + GetTeamLineup(context, team1);
            string direMention = "Dire = \n" +
                "Win: " + team1loss + " mmr /// Lose: " + team1gain + " mmr \n" + GetTeamLineup(context, team2);

            /*string casterMention = "Casters = \n" +
                _utilities.GetTeamLineup(context, casters);

            string spectatorMention = "Spectators = \n" +
                _utilities.GetTeamLineup(context, spectators);*/

            string lobbyName = "grin" + gameid;
            string lobbyPass = "grin" + DateTime.Now.Millisecond;

            string preinstructions = "\n\nLobby host can now create the game under **LobbyName = " + lobbyName + "**, and **Password = " + lobbyPass + "**.\n\n";
            string instructions = "After the game, the host can report the winner by command '!radiant game_id_here' , '!dire game_id_here', or !draw 'game_id_here. If you need any help or something isn't working please contact an admin/mod.";

            string final = "Server = " + region + "\n" +
                "Game ID = " + gameid + ". \n\n" +
                "Lobby host = " + hostMention + "\n\n" +

                preinstructions + "\n\n" +

                radiantMention + "\n" + direMention + "\n\n" +

                instructions;

            return final;
        }

        public async Task<string> GetLobbyPoolInfo(CommandContext context, int gameid)
        {
            var list = await _context.lobby_pool.ToListAsync();
            var players = list.FindAll(p => p._gameid == gameid);
            var radiant = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == gameid && p._side == (int)Side.Radiant);
            var dire = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == gameid && p._side == (int)Side.Dire);


            string lobby = "--- Game ID = " + gameid + "---\n";
            lobby += "Radiant Captain = <@" + radiant._p1 + ">\n";
            lobby += "Dire Captain = <@" + dire._p1 + ">\n\n";


            if (radiant._canpick == 1)
            {
                lobby += "Picking = Radiant\n";
            }
            if (dire._canpick == 1)
            {
                lobby += "Picking = Dire\n";
            }

            lobby += "\n";

            lobby += "--- Team Radiant ---\n\n" + await GetTeamDesc(radiant) + "\n\n";

            lobby += "--- Team Dire --- \n\n" + await GetTeamDesc(dire) + "\n";


            lobby += "\n\nPool = \n";

            foreach (var player in players)
            {
                string des = await GetPlayerDesc(player._discordid);
                lobby += des;
            }

            lobby += "\n\n";
            lobby += "Captains can pick players in the available pool using command !pick @mention ... Example: !pick <@126922582208282624>";

            return lobby;
        }

        private async Task<string> GetTeamDesc(TeamRecord record)
        {
            string players = "";

            players += await GetPlayerDesc(record._p1);
            players += await GetPlayerDesc(record._p2);
            players += await GetPlayerDesc(record._p3);
            players += await GetPlayerDesc(record._p4);
            players += await GetPlayerDesc(record._p5);

            return players;
        }

        private async Task<string> GetPlayerDesc(ulong id)
        {
            string player = "";

            var record = await _context.player_data.FindAsync(id);

            if (record != null)
                player += "<@" + record._id + ">, " + (Region)record._region + " - Dotammr = " + record._dotammr + ", Ihlmmr = " + record._ihlmmr + ", Preferred roles = " + record._role1 + "(Best), " + record._role2 + ".\n";

            return player;
        }

        public string GetTeamLineup(CommandContext context, List<Player> team)
        {
            string text = "";

            for (int i = 0; i < team.Count; i++)
            {
                string addon = "<not_found>";
                addon = "<@" + team[i]._id + ">" + " -- " + team[i]._ihlmmr + " inhouse mmr / " + team[i]._mmr + " dota mmr.\n";
                text += addon + "\n";
            }

            return text;
        }

        private Region GetRegion(List<Player> team1, List<Player> team2)
        {
            int east = 0;
            int west = 0;
            foreach (Player player in team1)
            {
                var rec = _context.player_data.Find(player._id);
                if (rec != null)
                {
                    if (rec._region == (int)Region.USEAST)
                    {
                        east++;
                    }
                    if (rec._region == (int)Region.USWEST)
                    {
                        west++;
                    }
                }
            }

            foreach (Player player in team2)
            {
                var rec = _context.player_data.Find(player._id);
                if (rec != null)
                {
                    if (rec._region == (int)Region.USEAST)
                    {
                        east++;
                    }
                    if (rec._region == (int)Region.USWEST)
                    {
                        west++;
                    }
                }

            }

            if (east > west)
            {
                return Region.USEAST;
            }
            else if (west > east)
            {
                return Region.USWEST;
            }
            else
            {
                return Region.USEAST;
            }
        }

    }
}
