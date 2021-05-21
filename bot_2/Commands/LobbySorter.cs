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
    public class LobbySorterUtilities
    {
        Context _context;
        public LobbySorterUtilities(Context context)
        {
            this._context = context;
        }
        public async Task PlacePlayersInChannel(CommandContext context, List<Player> players, DiscordChannel channel)
        {
            foreach (Player player in players)
            {
                if(context.Guild.Members.ContainsKey(player._id))
                {
                    var member = context.Guild.Members[player._id];
                    await member.PlaceInAsync(channel);
                }


            }

        }
        public async Task<DiscordRole> RecursiveCreateRole(CommandContext context, string type)
        {
            return await RecursiveCreateRole(context, type, 0);
        }
        public async Task<DiscordRole> RecursiveCreateRole(CommandContext context, string type, int counter)
        {
            var roles = context.Guild.Roles;
            var target = roles.FirstOrDefault(x => x.Value.Name == type + "Lobby" + counter).Value;

            if (target == null)
            {
                var role = await context.Guild.CreateRoleAsync(type + "Lobby" + counter);
                return role;

            }
            else
            {
                return await RecursiveCreateRole(context, type, counter + 1);
            }
        }

        public async Task<int> RecursiveGetLobbyNumber(CommandContext context)
        {
            return await RecursiveGetLobbyNumber(context, 0);
        }

        public async Task<int> RecursiveGetLobbyNumber(CommandContext context, int start)
        {
            var roles = context.Guild.Roles;
            var target = roles.FirstOrDefault(x => x.Value.Name == "radiantLobby" + start).Value;
            if (target == null)
            {

                return start;

            }
            else
            {
                return await RecursiveGetLobbyNumber(context, start + 1);
            }
        }

        public DiscordChannel GetLobby(CommandContext context, int identifier)
        {
            var channels = context.Guild.Channels;
            var target = channels.FirstOrDefault(x => x.Value.Name == "Lobby" + identifier).Value;
            return target;
        }

        public List<DiscordRole> GetRoles(CommandContext context, int identifier)
        {
            List<DiscordRole> returnedRoles = new List<DiscordRole>();
            if (context.Guild != null)
            {
                var roles = context.Guild.Roles;



                List<string> definedRoles = new List<string> { "radiant", "dire", "spectator", "caster" };

                foreach (string role in definedRoles)
                {
                    var target = roles.FirstOrDefault(x => x.Value.Name == role.ToLower() + "Lobby" + identifier).Value;
                    if (target != null)
                    {
                        returnedRoles.Add(target);
                    }

                }


            }
            return returnedRoles;
        }
        public int GetTeamTrueMmr(List<Player> team)
        {
            int counter = 0;
            foreach (Player player in team)
            {
                counter += player._truemmr;
            }

            return counter;
        }
        public string GetTeamLineup(CommandContext context, List<Player> team)
        {
            string text = "";

            for (int i = 0; i < team.Count; i++)
            {
                string addon = "<not_found>";
                addon = "<@"+team[i]._id+ ">" + " -- " + team[i]._ihlmmr + " inhouse mmr / " + team[i]._mmr + " dota mmr.\n";
                text += addon + "\n";
            }

            return text;
        }
        public async Task GrantRole(CommandContext context, List<Player> players, DiscordRole role)
        {
            var members = context.Guild.Members;
            for (int i = 0; i < players.Count; i++)
            {


                if (members.ContainsKey(players[i]._id))
                {
                    //await context.Channel.SendMessageAsync("granting role WORKED");
                    await members[players[i]._id].GrantRoleAsync(role);
                }
                else
                {
                    //await context.Channel.SendMessageAsync("granting role did not work, not found in db");
                }
            }
        }

        public DiscordRole GetRole(CommandContext context, string role)
        {
            var roles = context.Guild.Roles;
            var target = roles.FirstOrDefault(x => x.Value.Name == role).Value;
            return target;
        }

        public async Task ReportWinner(CommandContext context, string side, long steamid)
        {
            var gameProfile = await _context.discord_channel_info.FindAsync(context.Message.Author.Id);
            Profile _profile = new Profile(context);

            if (gameProfile == null)
            {

                await _profile.SendDm("You aren't recognized as the host of any game. If this is an error, message an admin or submit a ticket to credit your team with a victory.");
                //await context.Channel.SendMessageAsync("You aren't recognized as the host of any game. If this is an error, message an admin to submit a ticket to credit your team with a victory.");
            }
            else
            {
                await AddSteamIdToGameRecord(gameProfile, steamid);


                if (side.ToLower() == "radiant")
                {
                    await CreditVictor(context, gameProfile, Side.Radiant);
                    await DiscreditLoser(context, gameProfile, Side.Dire);
                }
                else if (side.ToLower() == "dire")
                {
                    await CreditVictor(context, gameProfile, Side.Dire);
                    await DiscreditLoser(context, gameProfile, Side.Radiant);
                }
                else if (side.ToLower() == "draw")
                {
                    //new code 5/14/2021
                    var game = await _context.game_data.FindAsync(gameProfile._gameid);
                    game._winner = (int)Side.Draw;
                    await _context.SaveChangesAsync();
                    var record1 = _context.game_record.FirstOrDefault(p => p._gameid == game._id && p._side == (int)Side.Radiant);
                    var record2 = _context.game_record.FirstOrDefault(p => p._gameid == game._id && p._side == (int)Side.Dire);

                    await ChangeTeamStatus(record1, 0);
                    await ChangeTeamStatus(record2, 0);
                }
                else
                {
                    await context.Channel.SendMessageAsync("Not a valid command, report 'radiant', 'dire', or 'draw'.");
                }
                var awaiting = await context.Guild.Channels[842870150994591764].SendMessageAsync("waiting for opendota api to pick up game under id " + steamid);
                //send msg to game history channel

                await RemoveHostRecord(context);

                Task task = await Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(2000);
                    await CloseLobby(context, gameProfile._number);
                }, TaskCreationOptions.LongRunning);


                if (context.Guild.Channels.ContainsKey(842870150994591764))
                {

                    var openDota = new OpenDotaApi();
                    var gameID = gameProfile._gameid;
                    try
                    {
                        var gameDetails = await openDota.Matches.GetMatchByIdAsync(steamid);
                        if (gameDetails != null)
                        {
                            string gHistory = await GetGameHistory(openDota, gameID, gameDetails);
                            var channel = context.Guild.Channels[842870150994591764];
                            await channel.SendMessageAsync(gHistory);
                        }
                        else
                        {
                            await _profile.SendDm("That game key was not recognized, the game was not stored in our history. Open a ticket and include the GrinHouse lobby # and the true game id. If you don't know where to find it feel free to take a screenshot of the match endscreen in the dota client.");
                        }
                    }
                    catch(Exception e)
                    {
                        await _profile.SendDm("The game wasn't found with a OpenDota api search which means it was most likely not ticketed. Be sure to ticket games in the future to keep pip from pulling his hair out!!");
                    }
   


                }


            }

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


                    int kills = player.HeroKills;
                int deaths = player.Deaths;
                long assists = player.Assists;
                int gpm = player.GoldPerMin;
                int xpm = player.XpPerMin;

                int lasthits = player.LastHits;
                int denies = player.Denies;

                long heroid = player.HeroId;

                var hero = heroes.FirstOrDefault(p => p.Id == heroid).LocalizedName;

                long dmg = player.HeroDamage;

                if(record!=null)
                {
                    info += "<@" + record._id + ">, Hero: " + hero + "\n - K/D/A: " + kills + "/" + deaths + "/" + assists;
                }
                else
                {
                    info += "<unkownplayer>, Hero: " + hero + "\n - K/D/A: " + kills + "/" + deaths + "/" + assists;
                }

                info +=  " - GPM/XPM: " + gpm + "/" + xpm + " - \n - LH/D: " + lasthits + "/" + denies + " - Dmg: " + dmg + "-\n";
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

        public async Task AddSteamIdToGameRecord(ChannelInfo record, long steamid)
        {
            var gameid = record._gameid;

            var game = await _context.game_data.FindAsync(gameid);
            game._steamid = steamid;
            await _context.SaveChangesAsync();
        }
        public async Task RemoveHostRecord(CommandContext context)
        {
            var record = await _context.discord_channel_info.FindAsync(context.Message.Author.Id);
            _context.discord_channel_info.Remove(record);
            await _context.SaveChangesAsync();
        }
        public async Task CreditVictor(CommandContext context, ChannelInfo gameProfile, Side side)
        {
            var game = await _context.game_data.FindAsync(gameProfile._gameid);
            game._winner = (int)side;
            await _context.SaveChangesAsync();
            var record = _context.game_record.FirstOrDefault(p => p._gameid == game._id && p._side == (int)side);

            var increment = record._onwin; // change

            await CreditPlayer(record._p1, increment);
            await CreditPlayer(record._p2, increment);
            await CreditPlayer(record._p3, increment);
            await CreditPlayer(record._p4, increment);
            await CreditPlayer(record._p5, increment);

            await ChangeTeamStatus(record, 0);
            await _context.SaveChangesAsync();
        }

        public async Task DiscreditLoser(CommandContext context, ChannelInfo gameProfile, Side side)
        {
            var game = await _context.game_data.FindAsync(gameProfile._gameid);
            //game._winner = (int)side;
            //await _context.SaveChangesAsync();
            var record = _context.game_record.FirstOrDefault(p => p._gameid == game._id && p._side == (int)side);

            var increment = record._onlose; // change

            await DiscreditPlayer(record._p1, increment);
            await DiscreditPlayer(record._p2, increment);
            await DiscreditPlayer(record._p3, increment);
            await DiscreditPlayer(record._p4, increment);
            await DiscreditPlayer(record._p5, increment);

            await ChangeTeamStatus(record, 0);
            await _context.SaveChangesAsync();
        }
        public async Task CreditPlayer(ulong player, int increment)
        {
            var playerRecord = await _context.player_data.FindAsync(player);
            if (playerRecord != null)
            {
                playerRecord._ihlmmr += increment;
                playerRecord._gameswon += 1;
            }

        }

        public async Task DiscreditPlayer(ulong player, int increment)
        {
            var playerRecord = await _context.player_data.FindAsync(player);
            if (playerRecord != null)
            {
                playerRecord._ihlmmr -= increment;
                playerRecord._gameslost += 1;
            }
        }
        public async Task CloseLobby(CommandContext context, int number) //only ex by trusted
        {
            //var number = 1;//look up the game id under host and number!
            //return number and report victory under gameid.



            var Roles = GetRoles(context, number);

            if (Roles != null)
            {
                foreach (DiscordRole role in Roles)
                {
                    await role.DeleteAsync();
                }
            }

            var Lobby = GetLobby(context, number);

            if (Lobby != null)
            {
                foreach (DiscordChannel channel in Lobby.Children)
                {
                    await channel.DeleteAsync();
                }

                await Lobby.DeleteAsync();
            }

        }

        public async Task ChangeTeamStatus(TeamRecord record, int status)
        {
            await ChangeGameStatus(record._p1, status);
            await ChangeGameStatus(record._p2, status);
            await ChangeGameStatus(record._p3, status);
            await ChangeGameStatus(record._p4, status);
            await ChangeGameStatus(record._p5, status);
            await _context.SaveChangesAsync();
        }
        public async Task ChangeGameStatus(ulong player, int status) //0 for notocc //1 for occ
        {
            var record = await _context.player_data.FindAsync(player);
            if (record != null)
            {
                record._gamestatus = status;
                await _context.SaveChangesAsync();
            }

        }
    }

    public class LobbySorter
    {
        Context _context;
        public LobbySorterUtilities _utilities;

        public LobbySorter(Context context)
        {
            this._context = context;
            this._utilities = new LobbySorterUtilities(context);
        }




        public async Task Setup(CommandContext context, Profile _profile)
        {
            try
            {
                Console.WriteLine("Creating roles, permissions, channels...");

                Permissions _perms = new Permissions(_context);
                await _perms.Create(context, _utilities);




                var maxPlayers = 10;

                Console.WriteLine("Forming player list...");
                List<Player> players = await FormPlayerList(maxPlayers);

                Console.WriteLine("Forming caster list...");
                List<Player> casters = await FormCasterList();

                Console.WriteLine("Forming spectator list...");
                List<Player> spectators = await FormSpectatorList();


                Console.WriteLine("Granting placeholder roles...");
                await _utilities.GrantRole(context, players, _perms.LobbyRoleSpectator);
                await _utilities.GrantRole(context, spectators, _perms.LobbyRoleSpectator);
                await _utilities.GrantRole(context, casters, _perms.LobbyRoleCaster);

                Console.WriteLine("Dming players...");
                await DmPlayers(context, casters, _perms.LobbyNumber);
                await DmPlayers(context, spectators, _perms.LobbyNumber);
                await DmPlayers(context, players, _perms.LobbyNumber);



                Console.WriteLine("Selecting ledaer...");

                Player leader = GetLeader(context, players);

                Console.WriteLine("Creating game to db...");

                await _context.game_data.AddAsync(new GameData { _host = leader._id });
                await _context.SaveChangesAsync();


                Console.WriteLine("Grabbing game in db..");

                var databaseList = await _context.game_data.ToListAsync();
                var orderedGames = databaseList.OrderByDescending(p => p._id).ToList();

                var gameid = orderedGames[0];

                Console.WriteLine("Creating DCI record...");
                await _context.discord_channel_info.AddAsync(new ChannelInfo { _id = leader._id, _number = _perms.LobbyNumber, _gameid = gameid._id });
                await _context.SaveChangesAsync();

                players = players.OrderByDescending(p => p._truemmr).ToList();

                Player captain1 = players[0];
                Player captain2 = players[1];

                players.Remove(captain1);
                players.Remove(captain2);
                
                var recordRadiant = await _context.game_record.AddAsync(new TeamRecord { _side = (int)Side.Radiant, _gameid = gameid._id, _p1 = captain2._id, _canpick = 1 });
                await _context.SaveChangesAsync();

                var recordDire = await _context.game_record.AddAsync(new TeamRecord { _side = (int)Side.Dire, _gameid = gameid._id, _p1 = captain1._id, _canpick = 0 });
                await _context.SaveChangesAsync();
                
                foreach(Player player in players)
                {
                    await _context.lobby_pool.AddAsync(new LobbyPool { _gameid = gameid._id, _discordid = player._id });
                    await _context.SaveChangesAsync();
                }




                //game_admin table
                //user id, game id, radiantrole name, direrole name, spectatorrole name


                //var castervoice = await context.Guild.CreateChannelAsync("Caster", DSharpPlus.ChannelType.Text, parent);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }



        }

        public async Task UpdateLobbyPool(CommandContext context, Profile _profile, int gameid)
        {
            Permissions perms = new Permissions(_context);
            perms.Get(context, _utilities, gameid);

            string msg = await GetLobbyPoolInfo(context, gameid);
            await perms.message.ModifyAsync(msg);
        }

        private async Task<string> GetLobbyPoolInfo(CommandContext context, int gameid)
        {
            var list = await _context.lobby_pool.ToListAsync();
            var players = list.FindAll(p => p._gameid == gameid);
            var radiant = await _context.game_record.FirstAsync(p => p._gameid == gameid && p._side == (int)Side.Radiant);
            var dire = await _context.game_record.FirstAsync(p => p._gameid == gameid && p._side == (int)Side.Dire);

             
            string lobby = "--- Game ID = " + gameid + "---\n";
            lobby += "Radiant Captain = <@" + radiant._p1 + ">, Picking? = " + CanPick(radiant) + "\n";
            lobby += "Dire Captain = <@" + dire._p1 + ">, Picking? = " + CanPick(dire) + "\n";
            lobby += "\n\nPool = \n";

            foreach (var player in players)
            {
                var record = await _context.player_data.FindAsync(player._discordid);
                lobby += "<@" + player._discordid + ">, Dotammr = " + record._dotammr + ", Ihlmmr = " + record._ihlmmr + ", Preferred roles = " + record._role1  +"(Best), " + record._role2 + ".\n";
            }

            return lobby;
        }

        private string CanPick(TeamRecord record)
        {
            if(record._canpick == 0)
            {
                return "FALSE";
            }
            return "TRUE";
        }
        private Player GetLeader(CommandContext context, List<Player> players)
        {
            Player leader = null;
            foreach (Player player in players)
            {
                var member = context.Guild.Members[player._id];
                var role = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Trusted").Value;

                if (member.Roles.Contains(role))
                {
                    leader = player;
                    break;
                }

            }

            if (leader == null)
            {
                var seed = Int32.Parse(DateTime.Now.ToString("fff"));
                Random rand = new Random(seed);
                int num = rand.Next(players.Count);
                leader = players[num];
            }

            return leader;
        }
       

        public async Task FormLobby(CommandContext context, Profile _profile, int gameid)
        {
            Permissions perms = new Permissions(_context);
            perms.Get(context, _utilities, gameid);

            var radiant = await _context.game_record.FirstAsync(p => p._gameid == gameid && p._side == (int)Side.Radiant);
            var dire = await _context.game_record.FirstAsync(p => p._gameid == gameid && p._side == (int)Side.Dire);

            var radiantteam = await GetPlayers(radiant);
            var direteam = await GetPlayers(dire);

            await _utilities.GrantRole(context, radiantteam, perms.LobbyRoleRadiant);
            await _utilities.GrantRole(context, direteam, perms.LobbyRoleDire);


            await perms.message.ModifyAsync(GetFullLobbyInfo(context, radiantteam, direteam, gameid));
        }

        public async Task<List<Player>> GetPlayers(TeamRecord teamrecord)
        {
            List<Player> players = new List<Player>();
            await RecordPlayer(players, teamrecord._p1);
            await RecordPlayer(players, teamrecord._p2);
            await RecordPlayer(players, teamrecord._p3);
            await RecordPlayer(players, teamrecord._p4);
            await RecordPlayer(players, teamrecord._p5);
            return players;

        }

        private async Task RecordPlayer(List<Player> players, ulong id)
        {
            var player = await _context.player_data.FindAsync(id);
            players.Add(new Player(player._id, player._dotammr, player._ihlmmr));
        }
        public string GetFullLobbyInfo(CommandContext context, List<Player> team1, List<Player> team2, int gameid)
        {
            var game = _context.discord_channel_info.First(p => p._gameid == gameid);
            ulong leaderid = game._id;
            int basemmr = 15;

            int team1mmr = _utilities.GetTeamTrueMmr(team1);
            int team2mmr = _utilities.GetTeamTrueMmr(team2);

            int team1gain = basemmr * team2mmr / team1mmr;

            int team1loss = basemmr * team1mmr / team2mmr;

            string hostMention = "<host_not_found>";
            if (context.Guild.Members.ContainsKey(leaderid))
            {
                var host = context.Guild.Members[leaderid];
                hostMention = host.Mention;
            }

            string radiantMention = "Radiant = \n" +
                "Win: " + team1gain + " mmr /// Lose: " + team1loss + " mmr \n" + _utilities.GetTeamLineup(context, team1);
            string direMention = "Dire = \n" +
                "Win: " + team1loss + " mmr /// Lose: " + team1gain + " mmr \n" + _utilities.GetTeamLineup(context, team2);

            /*string casterMention = "Casters = \n" +
                _utilities.GetTeamLineup(context, casters);

            string spectatorMention = "Spectators = \n" +
                _utilities.GetTeamLineup(context, spectators);*/

            string lobbyName = "grin" + gameid;
            string lobbyPass = "grin" + DateTime.Now.Millisecond;

            string preinstructions = "\n\nLobby host can now create the game under **LobbyName = " + lobbyName + "**, and **Password = " + lobbyPass + "**.\n\n";
            string instructions = preinstructions + "After the game, the host can report the winner by command '!radiant game_id_here' , '!dire game_id_here', or !draw 'game_id_here. If you need any help or something isn't working please contact an admin/mod.";

            string final = "Lobby Created. \n" +
                "Game ID = " + gameid + ". \n\n" +
                "Lobby host = " + hostMention + "\n\n" +

                radiantMention + "\n" + direMention + "\n" +

                instructions;

            return final;
        }

        public async Task<List<Player>> FormPlayerList(int maxPlayers)
        {
            List<Player> temp = new List<Player>();

            var list = await _context.player_queue.ToListAsync();

            List<QueueData> tempo = new List<QueueData>();
            for (int i = 0; i < maxPlayers; i++)
            {

                var record = list[i];
                ulong id = record._id;


                tempo.Add(record);



                var playerInfo = await _context.player_data.FindAsync(id);

                ulong newId = playerInfo._id;
                temp.Add(new Player(newId, playerInfo._dotammr, playerInfo._ihlmmr));
            }

            foreach(QueueData data in tempo)
            {
                _context.player_queue.Remove(data);
               await _context.SaveChangesAsync();
            }
            return temp;
        }

        public async Task<List<Player>> FormCasterList()
        {
            List<Player> temp = new List<Player>();
            var list = await _context.caster_queue.ToListAsync();

            List<CasterQueueData> tempo = new List<CasterQueueData>();
            for (int i = 0; i < list.Count; i++)
            {

                var record = list.First();
                ulong id = record._id;


                tempo.Add(record);

                var c_record = await _context.player_data.FindAsync(id);
                c_record._gamestatus = 0;
                await _context.SaveChangesAsync();

                var playerInfo = await _context.player_data.FindAsync(id);

                ulong newId = playerInfo._id;
                temp.Add(new Player(newId, playerInfo._dotammr, playerInfo._ihlmmr));
            }
            foreach (CasterQueueData data in tempo)
            {
                _context.caster_queue.Remove(data);
                await _context.SaveChangesAsync();
            }
            return temp;
        }

        public async Task<List<Player>> FormSpectatorList()
        {
            List<Player> temp = new List<Player>();
            var list = await _context.spectator_queue.ToListAsync();

            List<SpectatorQueueData> tempo = new List<SpectatorQueueData>();

            for (int i = 0; i < list.Count; i++)
            {

                var record = list.First();
                ulong id = record._id;

                tempo.Add(record);

                var s_record = await _context.player_data.FindAsync(id);
                s_record._gamestatus = 0;
                await _context.SaveChangesAsync();

                var playerInfo = await _context.player_data.FindAsync(id);

                ulong newId = playerInfo._id;
                temp.Add(new Player(newId, playerInfo._dotammr, playerInfo._ihlmmr));
            }
            foreach (SpectatorQueueData data in tempo)
            {
                _context.spectator_queue.Remove(data);
                await _context.SaveChangesAsync();
            }
            return temp;
        }
        
        public async Task DmPlayers(CommandContext context, List<Player> players, int LobbyNumber)
        {
            foreach (Player player in players)
            {
                Profile profile = new Profile(context, player._id);
                //profiles.Add(profile);
                await profile.SendDm("Match found. Find game details in #general under Lobby" + LobbyNumber);
            }
        }

    }
}
/*private async Task Yeet(CommandContext context)
       {

           Console.WriteLine("Granting role...");
           await _utilities.GrantRole(context, team1, LobbyRoleRadiant);
           await _utilities.GrantRole(context, team2, LobbyRoleDire);
           //if gameid is active in lobbyhost record --

           //CREATE LOBBY
           Console.WriteLine("Creating team1..");
           List<Player> team1 = new List<Player>();


           team1.Add(players[0]);
           team1.Add(players[3]);
           team1.Add(players[4]);
           team1.Add(players[7]);
           team1.Add(players[8]);



           int teamCount = 5;
           for (int i = 0; i < teamCount; i++)
           {
               players.Remove(team1[i]);
           }

           Console.WriteLine("Creating team2..");
           List<Player> team2 = players;


           Console.WriteLine("Getting teams' true mmr..");
           int basemmr = 15;

           int team1mmr = _utilities.GetTeamTrueMmr(team1);
           int team2mmr = _utilities.GetTeamTrueMmr(team2);

           int team1gain = basemmr * team2mmr / team1mmr;

           int team1loss = basemmr * team1mmr / team2mmr;


           Console.WriteLine("Inserting dire/radiant records...");


           /*if (maxPlayers != 10)
           {
               var recordRadiant = await _context.game_record.AddAsync(new TeamRecord { _side = 0, _gameid = gameid._id, _onwin = team1gain, _onlose = team1loss, _p1 = team1[0]._id });
               await _context.SaveChangesAsync();

               var recordDire = await _context.game_record.AddAsync(new TeamRecord { _side = 1, _gameid = gameid._id, _onwin = team1loss, _onlose = team1gain, _p1 = team2[0]._id });
               await _context.SaveChangesAsync();
           }
           else
           {
               var recordRadiant = await _context.game_record.AddAsync(new TeamRecord { _side = 0, _gameid = gameid._id, _onwin = team1gain, _onlose = team1loss, _p1 = team1[0]._id, _p2 = team1[1]._id, _p3 = team1[2]._id, _p4 = team1[3]._id, _p5 = team1[4]._id });
               await _context.SaveChangesAsync();
               var recordDire = await _context.game_record.AddAsync(new TeamRecord { _side = 1, _gameid = gameid._id, _onwin = team1loss, _onlose = team1gain, _p1 = team2[0]._id, _p2 = team2[1]._id, _p3 = team2[2]._id, _p4 = team2[3]._id, _p5 = team2[4]._id });
               await _context.SaveChangesAsync();
           }


           Console.WriteLine("Changing status of both teams to 1..."); //not needed, this is dont by queueing! i think.
           var team1record = _context.game_record.FirstOrDefault(p => p._gameid == gameid._id && p._side == 0);
           await _utilities.ChangeTeamStatus(team1record, 1);
           var team2record = _context.game_record.FirstOrDefault(p => p._gameid == gameid._id && p._side == 1);
           await _utilities.ChangeTeamStatus(team2record, 1);

           /*await _utilities.PlacePlayersInChannel(context, team1, radiantvoice);
           await _utilities.PlacePlayersInChannel(context, team2, direvoice);

           await _utilities.PlacePlayersInChannel(context, casters, castervoice);
           await _utilities.PlacePlayersInChannel(context, spectators, spectatorvoice);

           string hostMention = "<host_not_found>";
           if (context.Guild.Members.ContainsKey(leader._id))
           {
               var host = context.Guild.Members[leader._id];
               hostMention = host.Mention;
           }

           string radiantMention = "Radiant = \n" +
               "Win: " + team1gain + " mmr /// Lose: " + team1loss + " mmr \n" + _utilities.GetTeamLineup(context, team1);
           string direMention = "Dire = \n" +
               "Win: " + team1loss + " mmr /// Lose: " + team1gain + " mmr \n" + _utilities.GetTeamLineup(context, team2);

           string casterMention = "Casters = \n" +
               _utilities.GetTeamLineup(context, casters);

           string spectatorMention = "Spectators = \n" +
               _utilities.GetTeamLineup(context, spectators);

           string lobbyName = "grin" + gameid._id;
           string lobbyPass = "grin" + DateTime.Now.Millisecond;

           string preinstructions = "\n\nLobby host can now create the game under **LobbyName = " + lobbyName + "**, and **Password = " + lobbyPass + "**.\n\n";
           string instructions = preinstructions + "After the game, the host can report the winner by command '!radiant game_id_here' , '!dire game_id_here', or !draw 'game_id_here. If you need any help or something isn't working please contact an admin/mod.";
           await generaltext.SendMessageAsync("Lobby Created. \n" +
               "Game ID = " + gameid + ". \n\n" +
               "Lobby host = " + hostMention + "\n\n" +

               radiantMention + "\n" + direMention + "\n" +

               casterMention + "\n" + spectatorMention + "\n" +

               instructions);
       }*/