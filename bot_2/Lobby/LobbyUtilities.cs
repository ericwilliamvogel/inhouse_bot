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
    public class LobbyUtilities
    {
        Context _context;
        public LobbyUtilities(Context context)
        {
            this._context = context;
        }

        public async Task RemoveSpectatorRoles(CommandContext context, List<Player> team, int number)
        {
            var role = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == "spectatorLobby" + number).Value;
            foreach (Player player in team)
            {
                if (context.Guild.Members.ContainsKey(player._id))
                {
                    var member = context.Guild.Members[player._id];
                    if (member.Roles.Contains(role))
                    {
                        await member.RevokeRoleAsync(role);
                    }
                }
            }
        }

        public async Task PlacePlayersInChannel(CommandContext context, List<Player> players, DiscordChannel channel) //not functional
        {
            foreach (Player player in players)
            {
                if (context.Guild.Members.ContainsKey(player._id))
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
            var target = roles.FirstOrDefault(x => x.Value.Name == "team1Lobby" + start).Value;
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



                List<string> definedRoles = new List<string> { "team1", "team2", "spectator", "caster" };

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

        public Player GetLeader(CommandContext context, List<Player> players)
        {
            Player leader = null;
            foreach (Player player in players)
            {
                if (context.Guild.Members.ContainsKey(player._id))
                {
                    var member = context.Guild.Members[player._id];
                    var role = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == "Trusted").Value;

                    if (member.Roles.Contains(role))
                    {
                        leader = player;
                        break;
                    }
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


    }

}
