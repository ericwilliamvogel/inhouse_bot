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

    
    public class LobbySorter
    {
        private Context _context;
        private LobbyUtilities _utilities;
        private QueueGrabber _queueGrabber;


        public LobbyInfo _info;

        public LobbyReport _report;

        public LobbySorter(Context context)
        {
            this._context = context;
            this._utilities = new LobbyUtilities(context);
            this._queueGrabber = new QueueGrabber(context);
            this._info = new LobbyInfo(context, _utilities);
            this._report = new LobbyReport(context, _utilities, _info);
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
                List<Player> players = await _queueGrabber.FormPlayerList(maxPlayers);

                Console.WriteLine("Forming caster list...");
                List<Player> casters = await _queueGrabber.FormCasterList();

                Console.WriteLine("Forming spectator list...");
                List<Player> spectators = await _queueGrabber.FormSpectatorList();


                Console.WriteLine("Granting placeholder roles...");
                await _utilities.GrantRole(context, players, _perms.LobbyRoleSpectator);
                await _utilities.GrantRole(context, spectators, _perms.LobbyRoleSpectator);
                await _utilities.GrantRole(context, casters, _perms.LobbyRoleCaster);

                Console.WriteLine("Dming players...");
                await DmPlayers(context, casters, _perms.LobbyNumber);
                await DmPlayers(context, spectators, _perms.LobbyNumber);
                await DmPlayers(context, players, _perms.LobbyNumber);



                Console.WriteLine("Selecting leader...");

                Player leader = _utilities.GetLeader(context, players);

                Console.WriteLine("Creating game to db...");

                await _context.game_data.AddAsync(new GameData { _host = leader._id });
                await _context.SaveChangesAsync();


                Console.WriteLine("Grabbing game in db..");

                var databaseList = await _context.game_data.ToListAsync();
                var orderedGames = databaseList.OrderByDescending(p => p._id).ToList();

                var gameid = orderedGames[0];

                Console.WriteLine("Creating DCI record...");
                await _context.discord_channel_info.AddAsync(new ChannelInfo { _id = leader._id, _number = _perms.LobbyNumber, _gameid = gameid._id, _messageid = _perms.message.Id });
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

                await UpdateLobbyPool(context, _profile, gameid._id);



                //game_admin table
                //user id, game id, radiantrole name, direrole name, spectatorrole name


                //var castervoice = await context.Guild.CreateChannelAsync("Caster", DSharpPlus.ChannelType.Text, parent);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }



        }





       

        public async Task FormLobby(CommandContext context, Profile _profile, int gameid)
        {
            Permissions perms = new Permissions(_context);
            await perms.Get(context, _utilities, gameid);

            var radiant = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == gameid && p._side == (int)Side.Radiant);
            var dire = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == gameid && p._side == (int)Side.Dire);

            var radiantteam = await _utilities.GetPlayers(radiant);
            var direteam = await _utilities.GetPlayers(dire);



            int basemmr = 15;

            int team1mmr = _utilities.GetTeamTrueMmr(radiantteam);
            int team2mmr = _utilities.GetTeamTrueMmr(direteam);

            int team1gain = basemmr * team2mmr / team1mmr;

            int team1loss = basemmr * team1mmr / team2mmr;

            radiant._onwin = team1gain;
            radiant._onlose = team1loss;

            await _context.SaveChangesAsync();

            dire._onwin = team1loss;
            dire._onlose = team1gain;

            await _context.SaveChangesAsync();
             



            await _utilities.GrantRole(context, radiantteam, perms.LobbyRoleRadiant);
            await _utilities.GrantRole(context, direteam, perms.LobbyRoleDire);

            string lobbyinfo = _info.GetFullLobbyInfo(context, radiantteam, direteam, gameid);

            if(perms.message != null)
            {
                await perms.message.ModifyAsync(lobbyinfo);
            }
            else
            {
                await perms.generaltext.SendMessageAsync(lobbyinfo);
            }
        }

        public async Task UpdateLobbyPool(CommandContext context, Profile _profile, int gameid)
        {
            Permissions perms = new Permissions(_context);
            await perms.Get(context, _utilities, gameid);

            string msg = await _info.GetLobbyPoolInfo(context, gameid);

            try
            {
                if (perms.message != null)
                {
                    await perms.message.ModifyAsync(msg);
                }
                else
                {
                    await perms.generaltext.SendMessageAsync(msg);
                }
            }
            catch(Exception e)
            {
                await perms.generaltext.SendMessageAsync(msg);
            }
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