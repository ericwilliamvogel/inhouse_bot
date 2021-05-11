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

namespace bot_2.Commands
{
    public class Sorter
    {
        Context _context;
        public Sorter(Context _context)
        {
            this._context = _context;
        }

        public DiscordRole GetRole(CommandContext context, string role)
        {
            var roles = context.Guild.Roles;
            var target = roles.FirstOrDefault(x => x.Value.Name == role).Value;
            return target;
        }

        public async Task RevokeAllPermissions(DiscordChannel channel, DiscordRole role)
        {
            await channel.AddOverwriteAsync(role, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
        }

        public async Task ReportWinner(CommandContext context, string side, long steamid)
        {
            var gameProfile = await _context.discord_channel_info.FindAsync(context.Message.Author.Id);
            if (gameProfile == null)
            {
                Profile profile = new Profile(context);
                await profile.SendDm("You aren't recognized as the host of any game. If this is an error, message an admin or submit a ticket to credit your team with a victory.");
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
                    //await CreditVictor(context, gameProfile, Side.Draw);
                }
                else
                {
                    await context.Channel.SendMessageAsync("Not a valid command, report 'radiant', 'dire', or 'draw'.");
                }

                await RemoveHostRecord(context);
                await CloseLobby(context, gameProfile._number);
            }

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
            if(playerRecord != null)
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
            if(record != null)
            {
                record._gamestatus = status;
                await _context.SaveChangesAsync();
            }

        }
        public async Task Setup(CommandContext context)
        {
            try
            {

                var LobbyNumber = await RecursiveGetLobbyNumber(context);

                var LobbyRoleRadiant = await RecursiveCreateRole(context, "radiant");
                var LobbyRoleDire = await RecursiveCreateRole(context, "dire");
                //var LobbyRoleSpectator = await RecursiveCreateRole(context, "spectator");


                //string radiant, dire, spectator, general, lobby

                //await _context.SaveChangesAsync();
                //ulong id = record._id;


                //_context.player_queue.Remove(record);
                //await _context.SaveChangesAsync();



                //var playerInfo = await _context.player_data.FindAsync(id);
                //temp.Add(new Player(newId, playerInfo._dotammr, playerInfo._ihlmmr, playerInfo._adjmmr));

                //
                var AverageRole = GetRole(context, "member");
                var TrustedRole = GetRole(context, "trusted");

                var maxPlayers = 10;


                List<Player> players = await FormPlayerList(maxPlayers);
                List<Profile> profiles = new List<Profile>();

                foreach (Player player in players)
                {
                    profiles.Add(new Profile(context, player._id));
                    
                }

                foreach(Profile profile in profiles)
                {
                    await profile.SendDm("Match found. Find game details in #general under Lobby" + LobbyNumber);
                }





                Player leader = null;

                var findLeader = players.Find(p => p._status == 2);

                if (findLeader == null)
                {
                    var seed = Int32.Parse(DateTime.Now.ToString("fff"));
                    Random rand = new Random(seed);
                    int num = rand.Next(players.Count);
                    leader = players[num];
                    //appoint a leader
                }
                else
                {
                    leader = findLeader;
                }

                await _context.game_data.AddAsync(new GameData { _host = leader._id });
                await _context.SaveChangesAsync();


                var databaseList = await _context.game_data.ToListAsync();
                var orderedGames = databaseList.OrderByDescending(p => p._id);

                var gameid = orderedGames[0];




                var latestGame = await _context.game_data.FindAsync(gameid);

                if (latestGame == null)
                {
                    return;
                    //await context.Channel.SendMessageAsync("RECORD WASN'T FOUND AT ALL, numOfGames returning as = " + numOfGames.ToString());
                }
                else
                {
                    //await context.Channel.SendMessageAsync("latest game found, returning as " + numOfGames.ToString());
                }


                await _context.discord_channel_info.AddAsync(new ChannelInfo { _id = leader._id, _number = LobbyNumber, _gameid = gameid });
                await _context.SaveChangesAsync();
                

                //players.OrderBy(p => p._truemmr).ToList();


                List<Player> team1 = new List<Player>();

                team1.Add(players[0]);
                team1.Add(players[1]);
                team1.Add(players[5]);
                team1.Add(players[7]);
                team1.Add(players[9]);



                int teamCount = 5;
                for (int i = 0; i < teamCount; i++)
                {
                    players.Remove(team1[i]);
                }


                List<Player> team2 = players;


                int basemmr = 15;

                int team1mmr = GetTeamTrueMmr(team1);
                int team2mmr = GetTeamTrueMmr(team2);

                int team1gain = basemmr * team2mmr / team1mmr;

                int team1loss = basemmr * team1mmr / team2mmr;

                if (maxPlayers != 10)
                {
                    var recordRadiant = await _context.game_record.AddAsync(new TeamRecord { _side = 0, _gameid = latestGame._id, _onwin = team1gain, _onlose = team1loss, _p1 = team1[0]._id });
                    await _context.SaveChangesAsync();

                    var recordDire = await _context.game_record.AddAsync(new TeamRecord { _side = 1, _gameid = latestGame._id, _onwin = team1loss, _onlose = team1gain, _p1 = team2[0]._id });
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var recordRadiant = await _context.game_record.AddAsync(new TeamRecord { _side = 0, _gameid = latestGame._id, _onwin = team1gain, _onlose = team1loss, _p1 = team1[0]._id, _p2 = team1[1]._id, _p3 = team1[2]._id, _p4 = team1[3]._id, _p5 = team1[4]._id });
                    await _context.SaveChangesAsync();
                    var recordDire = await _context.game_record.AddAsync(new TeamRecord { _side = 1, _gameid = latestGame._id, _onwin = team1loss, _onlose = team1gain, _p1 = team2[0]._id, _p2 = team2[1]._id, _p3 = team2[2]._id, _p4 = team2[3]._id, _p5 = team2[4]._id });
                    await _context.SaveChangesAsync();
                }


                var team1record = _context.game_record.FirstOrDefault(p => p._gameid == gameid && p._side == 0);
                //team1record._onwin = team1gain;
                //team1record._onlose = team1loss;

                await ChangeTeamStatus(team1record, 1);

                var team2record = _context.game_record.FirstOrDefault(p => p._gameid == gameid && p._side == 1);
                //team2record._onwin = team1loss;
                //team2record._onlose = team1gain;
                //logic logic logic log ic
                await ChangeTeamStatus(team2record, 1);




                await GrantRole(context, team1, LobbyRoleRadiant);
                await GrantRole(context, team2, LobbyRoleDire);
                //grant role to casters later

                var parent = await context.Guild.CreateChannelCategoryAsync("Lobby" + LobbyNumber);

                var generaltext = await context.Guild.CreateChannelAsync("general", DSharpPlus.ChannelType.Text, parent);
                var radiantvoice = await context.Guild.CreateChannelAsync("Radiant", DSharpPlus.ChannelType.Voice, parent);
                var direvoice = await context.Guild.CreateChannelAsync("Dire", DSharpPlus.ChannelType.Voice, parent);

                //await RevokeAllPermissions(generaltext, AverageRole);
                //await RevokeAllPermissions(generaltext, TrustedRole);

                await generaltext.AddOverwriteAsync(LobbyRoleRadiant, DSharpPlus.Permissions.AccessChannels);
                await generaltext.AddOverwriteAsync(LobbyRoleDire, DSharpPlus.Permissions.AccessChannels);

                await generaltext.AddOverwriteAsync(LobbyRoleRadiant, DSharpPlus.Permissions.SendMessages);
                await generaltext.AddOverwriteAsync(LobbyRoleDire, DSharpPlus.Permissions.SendMessages);

                await generaltext.AddOverwriteAsync(LobbyRoleRadiant, DSharpPlus.Permissions.ReadMessageHistory);
                await generaltext.AddOverwriteAsync(LobbyRoleDire, DSharpPlus.Permissions.ReadMessageHistory);

                //await generaltext.AddOverwriteAsync(TrustedRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
                //await generaltext.AddOverwriteAsync(AverageRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);

                await generaltext.AddOverwriteAsync(LobbyRoleRadiant, DSharpPlus.Permissions.SendMessages);
                await generaltext.AddOverwriteAsync(LobbyRoleDire, DSharpPlus.Permissions.SendMessages);

                await generaltext.AddOverwriteAsync(LobbyRoleRadiant, DSharpPlus.Permissions.AccessChannels);
                await generaltext.AddOverwriteAsync(LobbyRoleDire, DSharpPlus.Permissions.AccessChannels);

                await radiantvoice.AddOverwriteAsync(TrustedRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
                await radiantvoice.AddOverwriteAsync(AverageRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);

                await RevokeAllPermissions(radiantvoice, AverageRole);
                await RevokeAllPermissions(radiantvoice, TrustedRole);
                await radiantvoice.AddOverwriteAsync(LobbyRoleRadiant, DSharpPlus.Permissions.AccessChannels);


                await direvoice.AddOverwriteAsync(TrustedRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
                await direvoice.AddOverwriteAsync(AverageRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);

                await RevokeAllPermissions(direvoice, AverageRole);
                await RevokeAllPermissions(direvoice, TrustedRole);
                await direvoice.AddOverwriteAsync(LobbyRoleDire, DSharpPlus.Permissions.AccessChannels);

                await RevokeAllPermissions(direvoice, LobbyRoleRadiant);
                await RevokeAllPermissions(radiantvoice, LobbyRoleDire);



                string hostMention = "<host_not_found>";
                if (context.Guild.Members.ContainsKey(leader._id))
                {
                    var host = context.Guild.Members[leader._id];
                    hostMention = host.Mention;
                }

                string radiantMention = "Radiant = \n" +
                    "Win: " + team1gain + " mmr /// Lose: " +team1loss + " mmr \n" + GetTeamLineup(context, team1);
                string direMention = "Dire = \n" +
                    "Win: " + team1loss + " mmr /// Lose: " + team1gain + " mmr \n" + GetTeamLineup(context, team2);

                string lobbyName = "grin" + gameid;
                string lobbyPass = DateTime.Now.Millisecond.ToString("fff");

                string preinstructions = "\n\nLobby host can now create the game under **LobbyName = " + lobbyName + "**, and **Password = " + lobbyPass + "**.\n\n";
                string instructions = preinstructions + "After the game, the host can report the winner by command '!radiant game_id_here' , '!dire game_id_here', or !draw 'game_id_here. If you need any help or something isn't working please contact an admin/mod.";
                await generaltext.SendMessageAsync("Lobby Created. \n" +
                    "Game ID = " + gameid + ". \n\n" +
                    "Lobby host = " + hostMention + "\n\n" +

                    radiantMention + "\n" + direMention + instructions);


                //game_admin table
                //user id, game id, radiantrole name, direrole name, spectatorrole name

                //var castervoice = await context.Guild.CreateChannelAsync("Caster", DSharpPlus.ChannelType.Text, parent);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }



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
                if (context.Guild.Members.ContainsKey(team[i]._id))
                {
                    addon = context.Guild.Members[team[i]._id].Mention;
                }
                text += addon + "\n";
            }

            return text;
        }
        public async Task CreateRoom(CommandContext context, DiscordRole radiant, DiscordRole dire)
        {
            await Task.CompletedTask;
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

        public async Task<List<Player>> FormPlayerList(int maxPlayers)
        {
            List<Player> temp = new List<Player>();

            for (int i = 0; i < maxPlayers; i++)
            {
                var list = await _context.player_queue.ToListAsync();
                var record = list.First();
                ulong id = record._id;


                _context.player_queue.Remove(record);
                await _context.SaveChangesAsync();



                var playerInfo = await _context.player_data.FindAsync(id);

                ulong newId = playerInfo._id;
                temp.Add(new Player(newId, playerInfo._dotammr, playerInfo._ihlmmr, playerInfo._adjmmr, playerInfo._status));
            }

            return temp;
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
            var roles = context.Guild.Channels;
            var target = roles.FirstOrDefault(x => x.Value.Name == "Lobby" + identifier).Value;
            return target;
        }

        public List<DiscordRole> GetRoles(CommandContext context, int identifier)
        {
            List<DiscordRole> returnedRoles = new List<DiscordRole>();
            if (context.Guild != null)
            {
                var roles = context.Guild.Roles;



                List<string> definedRoles = new List<string> { "radiant", "dire", "spectator" };

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
    }
}
