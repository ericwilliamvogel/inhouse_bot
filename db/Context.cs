using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace db
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) { }
        public DbSet<DataSet> primary_table { get; set; } //test

        public DbSet<PlayerData> player_data { get; set; }

        public DbSet<LobbyPool> lobby_pool { get; set; }

        public DbSet<QueueData> player_queue { get; set; }

        public DbSet<CasterQueueData> caster_queue { get; set; }

        public DbSet<SpectatorQueueData> spectator_queue { get; set; }

        public DbSet<GameData> game_data { get; set; }

        public DbSet<TeamRecord> game_record { get; set; }

        public DbSet<ChannelInfo> discord_channel_info { get; set; }

        //
        //pls type !register <steamid> - help 
        //

        //registeredplayer(primary key) name(discordname), setname, ihl mmr, games won, games lost, steamid, admin assigned mmr
        //status
        //0 unregistered - no access
        //1 registered - access + initial mmr assigned
        //2 trusted > 8 games played
        //3 mod = promoted by admin
        //4 admin
        //7 banned

        //on completed game return a match id

        //match_table
        //autoincrement gameid, winner(bool?) true for radiant false for dire, attached matchid(assigned by mod)

        //team_table
        //gameid, player1, player2, player3, player4, player5, "radiant"
        //gameid, player1 player2 player4 player4 player5, "dire"

        //queue_table
        //playerid, datetime started

        //discord_group_availability
        //groupname(ie. team1), available(boolean)

        //on game finish, set available to true and remove roles.


        //!flame
        //most games played, highest mmr, second highest mmr, third highest mmr
        //faction with highest mmr (grin, wss, etc)

        //update queue w/ how many have been in queue for how long w/ 1 sec poll. start w/ 10 and see how it goes until 1

        //queue popped -> auto balance teams into radiant and dire + assign role for team1 rad and team1 dire.
        //caster queue, if queue pops put caster in equal team chat to get game info.
        //spectator queue, if queue pops put spectator in equal team chat to get game info. 
    }
}
