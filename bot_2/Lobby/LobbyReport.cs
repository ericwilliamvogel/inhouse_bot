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
    public class LobbyReport
    {
        private Context _context;
        private LobbyUtilities _utilities;
        private LobbyInfo _info;
        public LobbyReport(Context context, LobbyUtilities utilities, LobbyInfo info)
        {
            this._context = context;
            this._utilities = utilities;
            this._info = info;
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

                    await _utilities.ChangeTeamStatus(record1, 0);
                    await _utilities.ChangeTeamStatus(record2, 0);
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
                            string gHistory = await _info.GetGameHistory(openDota, gameID, gameDetails);
                            var channel = context.Guild.Channels[842870150994591764];
                            await channel.SendMessageAsync(gHistory);
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
            var record = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == game._id && p._side == (int)side);

            var increment = record._onwin; // change

            await CreditPlayer(record._p1, increment);
            await CreditPlayer(record._p2, increment);
            await CreditPlayer(record._p3, increment);
            await CreditPlayer(record._p4, increment);
            await CreditPlayer(record._p5, increment);

            await _utilities.ChangeTeamStatus(record, 0);
            await _context.SaveChangesAsync();
        }

        public async Task DiscreditLoser(CommandContext context, ChannelInfo gameProfile, Side side)
        {
            var game = await _context.game_data.FindAsync(gameProfile._gameid);
            //game._winner = (int)side;
            //await _context.SaveChangesAsync();
            var record = await _context.game_record.FirstOrDefaultAsync(p => p._gameid == game._id && p._side == (int)side);

            var increment = record._onlose; // change

            await DiscreditPlayer(record._p1, increment);
            await DiscreditPlayer(record._p2, increment);
            await DiscreditPlayer(record._p3, increment);
            await DiscreditPlayer(record._p4, increment);
            await DiscreditPlayer(record._p5, increment);

            await _utilities.ChangeTeamStatus(record, 0);
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