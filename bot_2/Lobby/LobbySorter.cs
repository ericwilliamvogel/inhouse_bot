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
            Conditions._locked = true;

            await Task.Delay(3000); //to wait for updatethread to finish!
            var maxPlayers = 10;

            Console.WriteLine("Forming player list...");
            List<Player> players = await _queueGrabber.FormPlayerList(maxPlayers);

            Console.WriteLine("Forming caster list...");
            List<Player> casters = await _queueGrabber.FormCasterList();

            Console.WriteLine("Forming spectator list...");
            List<Player> spectators = await _queueGrabber.FormSpectatorList();

            Console.WriteLine("Selecting leader...");
            Player leader = _utilities.GetLeader(context, players);

            try
            {
                Console.WriteLine("Creating roles, permissions, channels...");

                Permissions _perms = new Permissions(_context);
                await _perms.Create(context, _utilities);

                Console.WriteLine("Granting placeholder roles...");
                await _utilities.GrantRole(context, players, _perms.LobbyRoleSpectator);
                await _utilities.GrantRole(context, spectators, _perms.LobbyRoleSpectator);
                await _utilities.GrantRole(context, casters, _perms.LobbyRoleCaster);

                Console.WriteLine("Dming players...");
                await DmPlayers(context, casters, _perms.LobbyNumber);
                await DmPlayers(context, spectators, _perms.LobbyNumber);
                await DmPlayers(context, players, _perms.LobbyNumber);


                Console.WriteLine("Creating game to db...");

                DateTimeOffset start = DateTimeOffset.Now;

                await _context.game_data.AddAsync(new GameData { _host = leader._id, _start = start });
                await _context.SaveChangesAsync();


                Console.WriteLine("Grabbing game in db..");

                var databaseList = await _context.game_data.ToListAsync();
                var orderedGames = databaseList.OrderByDescending(p => p._id).ToList();

                var gameid = orderedGames[0];

                Console.WriteLine("Creating DCI record...");
                await _context.discord_channel_info.AddAsync(new ChannelInfo { _id = leader._id, _number = _perms.LobbyNumber, _gameid = gameid._id, _messageid = _perms.message.Id });
                await _context.SaveChangesAsync();

                int ms = DateTime.Now.Millisecond;
                Random rand = new Random(ms);
                int decider = rand.Next(1, 3);

                if(decider == 1)
                {
                    players = players.OrderByDescending(p => p._truemmr).ToList();
                }
                else if(decider == 2)
                {
                    players = players.OrderBy(p => p._truemmr).ToList();
                }
                else
                {
                    players = players.OrderBy(p => p._truemmr).ToList();
                }


                Player captain1 = players[0];
                Player captain2 = players[1];

                players.Remove(captain1);
                players.Remove(captain2);
                
                var recordTeam1 = await _context.game_record.AddAsync(new TeamRecord { _side = (int)Side.Team1, _gameid = gameid._id, _p1 = captain2._id, _canpick = 1 });
                await _context.SaveChangesAsync();

                var recordTeam2 = await _context.game_record.AddAsync(new TeamRecord { _side = (int)Side.Team2, _gameid = gameid._id, _p1 = captain1._id, _canpick = 0 });
                await _context.SaveChangesAsync();
                
                foreach(Player player in players)
                {
                    await _context.lobby_pool.AddAsync(new LobbyPool { _gameid = gameid._id, _discordid = player._id });
                    await _context.SaveChangesAsync();
                }

                await UpdateLobbyPool(context, _profile, gameid._id);
            }
            catch (Exception e)
            {
                var record = await _context.discord_channel_info.FindAsync(leader._id);
                if(record!=null)
                {
                    _context.discord_channel_info.Remove(record);
                    await _context.SaveChangesAsync();
                }
                await _profile.ReportError(context, e);
                Console.WriteLine(e);
            }

            Conditions._locked = false;

        }





       

        public async Task FormLobby(CommandContext context, Profile _profile, int gameid)
        {
            Permissions perms = new Permissions(_context);
            await perms.Get(context, _utilities, gameid);

            var team1 = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == gameid && p._side == (int)Side.Team1);
            var team2 = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == gameid && p._side == (int)Side.Team2);

            var team1players = await _utilities.GetPlayers(team1);
            var team2players = await _utilities.GetPlayers(team2);




            int basemmr = 15;

            int team1mmr = _utilities.GetTeamTrueMmr(team1players);
            int team2mmr = _utilities.GetTeamTrueMmr(team2players);

            int team1gain = basemmr * team2mmr / team1mmr;

            int team1loss = basemmr * team1mmr / team2mmr;

            team1._onwin = team1gain;
            team1._onlose = team1loss;

            await _context.SaveChangesAsync();

            team2._onwin = team1loss;
            team2._onlose = team1gain;

            await _context.SaveChangesAsync();

            await _utilities.RemoveSpectatorRoles(context, team1players, perms.LobbyNumber);
            await _utilities.RemoveSpectatorRoles(context, team2players, perms.LobbyNumber);

            await _utilities.GrantRole(context, team1players, perms.LobbyRoleTeam1);
            await _utilities.GrantRole(context, team2players, perms.LobbyRoleTeam2);

            string lobbyinfo = await _info.GetFullLobbyInfo(context, team1players, team2players, gameid);




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