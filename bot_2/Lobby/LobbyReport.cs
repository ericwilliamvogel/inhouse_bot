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
    public class LobbyReport
    {
        private Context _context;
        private LobbyUtilities _utilities;
        private LobbyInfo _info;
        private QOL QOL;
        private int xpincrement = 100;
        public LobbyReport(Context context, LobbyUtilities utilities, LobbyInfo info)
        {
            QOL = new QOL();
            this._context = context;
            this._utilities = utilities;
            this._info = info;
        }

        public async Task ReportWinner(CommandContext context, string side, long steamid)
        {
            Conditions._locked = true;

            var gameProfile = await _context.discord_channel_info.FindAsync(context.Message.Author.Id);
            Profile _profile = new Profile(context);

            if (gameProfile == null)
            {

                await _profile.SendDm("You aren't recognized as the host of any game. If this is an error, message an admin or submit a ticket to credit your team with a victory.");
                //await context.Channel.SendMessageAsync("You aren't recognized as the host of any game. If this is an error, message an admin to submit a ticket to credit your team with a victory.");
            }
            else
            {
                var record1 = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == gameProfile._gameid && p._side == (int)Side.Team1);
                var record2 = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == gameProfile._gameid && p._side == (int)Side.Team2);
                

                if(record1 == null || record2 == null)
                {
                    await _profile.SendDm("No records exists under this game! Are you trying to execute a command while the lobby is starting?");
                }
                else
                {
                    await AddSteamIdToGameRecord(gameProfile, steamid);
                    //here


                    if (side.ToLower() == "team1")
                    {
                        await CreditVictor(context, gameProfile, Side.Team1);
                        await DiscreditLoser(context, gameProfile, Side.Team2);
                    }
                    else if (side.ToLower() == "team2")
                    {
                        await CreditVictor(context, gameProfile, Side.Team2);
                        await DiscreditLoser(context, gameProfile, Side.Team1);
                    }
                    else if (side.ToLower() == "draw")
                    {
                        //new code 5/14/2021
                        var game = await _context.game_data.FindAsync(gameProfile._gameid);
                        var id = game._id;
                        game._winner = (int)Side.Draw;
                        //await _context.SaveChangesAsync();
                        //var record1 = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == game._id && p._side == (int)Side.Radiant);
                        //var record2 = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == game._id && p._side == (int)Side.Dire);

                        if (record1 != null)
                            await _utilities.ChangeTeamStatus(record1, 0);
                        if (record2 != null)
                            await _utilities.ChangeTeamStatus(record2, 0);

                        var list = await _context.lobby_pool.ToListAsync();
                        var records = list.FindAll(p => p._gameid == id);
                        if(records != null)
                        {
                            if(records.Count > 0)
                            {
                                foreach (LobbyPool pool in records)
                                {
                                    await _utilities.ChangeGameStatus(pool._discordid, 0);
                                }

                            }
                        }

                        try //this is nested so this doesnt crash bot
                        {
                            var largeBetList = await _context.game_bets.ToListAsync();
                            var specificList = largeBetList.FindAll(p => p._gameid == gameProfile._gameid);

                            foreach (var betRecord in specificList)
                            {
                                    var playerRecord = await _context.player_data.FindAsync(betRecord._discordid);
                                    if (playerRecord != null)
                                    {
                                        playerRecord._xp += betRecord._amount;
                                        //await _context.SaveChangesAsync();
                                    }

                                

                                _context.game_bets.Remove(betRecord);
                                //await _context.SaveChangesAsync();
                            }

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                        _context.game_data.Remove(game);
                        //await _context.SaveChangesAsync();
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync("Not a valid command, report 'team1', 'team2', or 'draw'.");
                    }


 

                    await WrapUp(context, gameProfile._number);

                    if (Bot._game == GameType.Dota2)
                    {
                        await QOL.SendMessage(context, "game-history",
                        "waiting for opendota api to pick up game under id " + steamid);


                        var openDota = new OpenDotaApi();
                        var gameID = gameProfile._gameid;
                        try
                        {
                            var gameDetails = await openDota.Matches.GetMatchByIdAsync(steamid);
                            if (gameDetails != null)
                            {
                                string gHistory = await _info.GetGameHistory(openDota, gameID, gameDetails);
                                await QOL.SendMessage(context, "game-history", gHistory);
                            }
                            else
                            {
                                await _profile.SendDm("That game key was not recognized, the game was not stored in our history. Open a ticket and include the GrinHouse lobby # and the true game id. If you don't know where to find it feel free to take a screenshot of the match endscreen in the dota client.");
                            }
                        }
                        catch (Exception e)
                        {
                            await _profile.SendDm("The game wasn't found with a OpenDota api search which means it was most likely not ticketed. Be sure to ticket games in the future to keep pip from pulling his hair out!!");
                        }
                    }
                    
                }




            }

            Conditions._locked = false;

        }


        public async Task GiveTeamXP(TeamRecord record)
        {
            await GivePlayerXP(record._p1);
            await GivePlayerXP(record._p2);
            await GivePlayerXP(record._p3);
            await GivePlayerXP(record._p4);
            await GivePlayerXP(record._p5);
        }
        public async Task GivePlayerXP(ulong id)
        {
            var player = await _context.player_data.FindAsync(id);
            player._xp += xpincrement;
            //await _context.SaveChangesAsync();
        }

        public async Task WrapUp(CommandContext context, int lobbyNumber)
        {
            await RemoveHostRecord(context);

            UpdatedQueue info = new UpdatedQueue(_context);
            await info.UpdateLeaderboard(context);

            Task task = await Task.Factory.StartNew(async () =>
            {
                await Task.Delay(2000);
                await CloseLobby(context, lobbyNumber);
            }, TaskCreationOptions.LongRunning);

        }
        public async Task AddSteamIdToGameRecord(ChannelInfo record, long steamid)
        {
            var gameid = record._gameid;

            var game = await _context.game_data.FindAsync(gameid);
            game._steamid = steamid;
            //await _context.SaveChangesAsync();
        }
        public async Task RemoveHostRecord(CommandContext context)
        {
            var record = await _context.discord_channel_info.FindAsync(context.Message.Author.Id);
            if(record != null)
            {
                _context.discord_channel_info.Remove(record);
                //await _context.SaveChangesAsync();
            }

        }
        public async Task CreditVictor(CommandContext context, ChannelInfo gameProfile, Side side)
        {
            var game = await _context.game_data.FindAsync(gameProfile._gameid);
            game._winner = (int)side;
            //await _context.SaveChangesAsync();
            var record = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == game._id && p._side == (int)side);

            var increment = record._onwin; // change

            await CreditPlayer(record._p1, increment);
            await CreditPlayer(record._p2, increment);
            await CreditPlayer(record._p3, increment);
            await CreditPlayer(record._p4, increment);
            await CreditPlayer(record._p5, increment);

            await _utilities.ChangeTeamStatus(record, 0);
            //await _context.SaveChangesAsync();

            try //this is nested so this doesnt crash bot
            {
                var largeBetList = await _context.game_bets.ToListAsync();
                var specificList = largeBetList.FindAll(p => p._gameid == gameProfile._gameid);

                DiscordChannel channel = null;
                if(await Bot._validator.Exists(context, "bet-history"))
                {
                    channel = await Bot._validator.Get(context, "bet-history");
                }

                foreach (var betRecord in specificList)
                {
                    if (betRecord._side == (int)side)
                    {
                        var playerRecord = await _context.player_data.FindAsync(betRecord._discordid);
                        if (playerRecord != null)
                        {
                            int payout = betRecord._amount * 2;
                            playerRecord._xp += payout;
                            //await _context.SaveChangesAsync();

                            if(channel != null)
                                await channel.SendMessageAsync("<@" + betRecord._discordid + "> won their bet, paid" + payout + " coins.");
                            
                        }

                    }
                    else
                    {
                        if (channel != null)
                            await channel.SendMessageAsync("<@" + betRecord._discordid + "> lost their bet in the amount of " + betRecord._amount + " coins. Nice try!");
                    }

                    _context.game_bets.Remove(betRecord);
                    //await _context.SaveChangesAsync();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


        }

        public async Task DiscreditLoser(CommandContext context, ChannelInfo gameProfile, Side side)
        {
            var game = await _context.game_data.FindAsync(gameProfile._gameid);
            //game._winner = (int)side;
            ////await _context.SaveChangesAsync();
            var record = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == game._id && p._side == (int)side);

            var increment = record._onlose; // change

            await DiscreditPlayer(record._p1, increment);
            await DiscreditPlayer(record._p2, increment);
            await DiscreditPlayer(record._p3, increment);
            await DiscreditPlayer(record._p4, increment);
            await DiscreditPlayer(record._p5, increment);

            await _utilities.ChangeTeamStatus(record, 0);
            //await _context.SaveChangesAsync();
        }
        public async Task CreditPlayer(ulong player, int increment)
        {
            var playerRecord = await _context.player_data.FindAsync(player);
            if (playerRecord != null)
            {
                if(playerRecord._totalgames == 0)
                {
                    playerRecord._ihlmmr = 400;
                }
                playerRecord._ihlmmr += increment;
                playerRecord._gameswon += 1;
                playerRecord._totalgames += 1;
                playerRecord._xp += xpincrement;
            }

        }

        public async Task DiscreditPlayer(ulong player, int increment)
        {
            var playerRecord = await _context.player_data.FindAsync(player);
            if (playerRecord != null)
            {
                if (playerRecord._totalgames == 0)
                {
                    playerRecord._ihlmmr = 400;
                }
                playerRecord._ihlmmr -= increment;
                playerRecord._gameslost += 1;
                playerRecord._totalgames += 1;
                playerRecord._xp += xpincrement / 2;
            }
        }
        public async Task CloseLobby(CommandContext context, int number) //only ex by trusted
        {
            //var number = 1;//look up the game id under host and number!
            //return number and report victory under gameid.



            var Roles = _utilities.GetRoles(context, number);

            if (Roles != null)
            {
                foreach (DiscordRole role in Roles)
                {
                    await role.DeleteAsync();
                }
            }

            var Lobby = _utilities.GetLobby(context, number);

            if (Lobby != null)
            {
                foreach (DiscordChannel channel in Lobby.Children)
                {
                    await channel.DeleteAsync();
                }

                await Lobby.DeleteAsync();
            }

        }



    }
}
