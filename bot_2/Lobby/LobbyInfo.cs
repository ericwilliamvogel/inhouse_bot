﻿using System;
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

        public async Task<string> GetFullLobbyInfo(CommandContext context, List<Player> team1, List<Player> team2, int gameid)
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

            string team1Mention = "Team1 = \n" +
                "Win: " + team1gain + " mmr /// Lose: " + team1loss + " mmr \n" + await GetTeamLineup(context, team1);
            string team2Mention = "Team2 = \n" +
                "Win: " + team1loss + " mmr /// Lose: " + team1gain + " mmr \n" + await GetTeamLineup(context, team2);

            /*string casterMention = "Casters = \n" +
                _utilities.GetTeamLineup(context, casters);

            string spectatorMention = "Spectators = \n" +
                _utilities.GetTeamLineup(context, spectators);*/


            string instructions = "After the game, the host can report the winner by command '!team1 game_id_here' , '!team2 game_id_here', or !draw 'game_id_here. If you need any help or something isn't working please contact an admin/mod.";
            instructions += "If a player cannot play / abandons, the LOBBY HOST can kick that player using !kick @mention ... Example: !kick <@126922582208282624>";

            string final = "Server = " + region + "\n" +
                "Game ID = " + gameid + ". \n\n" +
                "Lobby host = " + hostMention +  "\n\n" +

                team1Mention + "\n" + team2Mention + "\n\n" +

                instructions;

            return final;
        }

        public string GetLobbyPass(string lobbyname)
        {
            string lobbyName = lobbyname;//"grin" + gameid;
            string lobbyPass = "grin" + DateTime.Now.Millisecond;

            string preinstructions = "\n\n**LobbyName = " + lobbyName + "**\n**Password = " + lobbyPass + "**.\n\n";

            return preinstructions;
        }

        public async Task<string> GetLobbyPoolInfo(CommandContext context, int gameid)
        {
            var list = await _context.lobby_pool.ToListAsync();
            var discordInfo = await _context.discord_channel_info.FirstOrDefaultAsync(p => p._gameid == gameid);
            var players = list.FindAll(p => p._gameid == gameid);
            var team1 = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == gameid && p._side == (int)Side.Team1);
            var team2 = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == gameid && p._side == (int)Side.Team2);


            string lobby = "--- Game ID = " + gameid + "---\n";

            if (discordInfo != null)
                lobby += "Lobby Host = <@" + discordInfo._id + ">\n";

            lobby += "Team1 Captain = <@" + team1._p1 + ">\n";
            lobby += "Team2 Captain = <@" + team2._p1 + ">\n\n";
            

            if (team1._canpick == 1)
            {
                lobby += "Picking = team1\n";
            }
            if (team2._canpick == 1)
            {
                lobby += "Picking = team2\n";
            }

            lobby += "\n";

            lobby += "--- Team 1 ---\n\n" + await GetTeamDesc(team1) + "\n\n";

            lobby += "--- Team 2 --- \n\n" + await GetTeamDesc(team2) + "\n";


            lobby += "\n\nPool = \n";

            foreach (var player in players)
            {
                string des = await GetPlayerDesc(player._discordid);
                lobby += des;
            }

            lobby += "\n";
            lobby += "```Captains can pick players in the available pool using command !pick @mention ``` Example: **!pick** <@126922582208282624>\n";
            lobby += "```Did someone have to leave your game during draft phase or midgame? Use command !draw 0 to kill the discord channels and internally reset all players.``` Example: **!draw 0**\n";
            lobby += "```Can some players not see the lobby? Simply !invite @mention so they can see the pick phase! These same players may also have trouble getting their lobby roles(team1, team2, etc...), refer below for info on how to invite them! ```" +
                " Example: **!invite** <@126922582208282624>\n";
            lobby += "```Are some players not able to see their respective discord voice channels? Simply !invite team1 @mention so they can get access to their voice channel! Example: **invite team1** <@126922582208282624>";
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

        public async Task<string> GetTeamLineup(CommandContext context, List<Player> team)
        {
            string text = "";

            for (int i = 0; i < team.Count; i++)
            {
                string addon = "<not_found>";
                addon = await GetPlayerDesc(team[i]._id);
                //addon = "<@" + team[i]._id + ">" + " -- " + team[i]._ihlmmr + " inhouse mmr / " + team[i]._mmr + " dota mmr.\n";
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
