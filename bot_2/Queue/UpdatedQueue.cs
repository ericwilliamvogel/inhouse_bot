using bot_2.Json;
using db;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace bot_2.Commands
{
    public enum QueueMessageSegment
    {
        Introduction,
        Leaderboard,
        Queue,
        SpectatorQueue,
        CasterQueue,
        OngoingGames
    }
    public class UpdatedQueue
    {
        Context _context;
        GeneralDatabaseInfo _info;
        QueueInfo _queueInfo;
        static bool started = false;

        MmrCalculator calculator = new MmrCalculator();

        public Dictionary<QueueMessageSegment, ulong> PreLoadedMessages { get; set; }
        public Dictionary<QueueMessageSegment, string> PreLoadedTitles { get; set; }
        
        public UpdatedQueue(Context context)
        {
            PreLoadedMessages = new Dictionary<QueueMessageSegment, ulong>();
            PreLoadedTitles = new Dictionary<QueueMessageSegment, string>();

            this._context = context;
            _info = new GeneralDatabaseInfo(context);
            _queueInfo = new QueueInfo(context);

            JsonCommunicator comm = new JsonCommunicator();
            PreLoadedMessages.Add(QueueMessageSegment.Introduction, comm.GetValue("misc", "queue_message_intro"));
            PreLoadedMessages.Add(QueueMessageSegment.Queue, comm.GetValue("misc", "queue_message_queue"));
            PreLoadedMessages.Add(QueueMessageSegment.SpectatorQueue, comm.GetValue("misc", "queue_message_spectator"));
            PreLoadedMessages.Add(QueueMessageSegment.CasterQueue, comm.GetValue("misc", "queue_message_caster"));
            PreLoadedMessages.Add(QueueMessageSegment.OngoingGames, comm.GetValue("misc", "queue_message_ongoing"));
            PreLoadedMessages.Add(QueueMessageSegment.Leaderboard, comm.GetValue("misc", "queue_message_leaderboard"));

            PreLoadedTitles.Add(QueueMessageSegment.Introduction, "Intro");
            PreLoadedTitles.Add(QueueMessageSegment.Queue, "Queue");
            PreLoadedTitles.Add(QueueMessageSegment.SpectatorQueue, "Spectator Queue");
            PreLoadedTitles.Add(QueueMessageSegment.CasterQueue, "Caster Queue");
            PreLoadedTitles.Add(QueueMessageSegment.OngoingGames, "Ongoing Games");
            PreLoadedTitles.Add(QueueMessageSegment.Leaderboard, "Leaderboard");
        }



        public static async Task ResetVariables()
        {
            started = false;
            await Task.CompletedTask;
        }

        public async Task StartThread(CommandContext context)
        {
            if (!started)
            {
                started = true;

                await UpdateQueue(context);



                
            }
        }

        public async Task<DiscordMessage> GetMessage(CommandContext context, ulong channel, ulong message)
        {
            if(context.Guild.Channels.ContainsKey(channel))
            {
                var returnedMessage = await context.Guild.Channels[channel].GetMessageAsync(message);
                return returnedMessage;
            }
            return null;
        }

        public static bool _update = false;
        public async Task UpdateQueue(CommandContext context)
        {

            try
            {
                if(Conditions._locked)
                {
                    return;
                }

                var channel = await Bot._validator.Get(context, "queue");
                
                var players = await _queueInfo.GetPlayerQueueInfo(context);
                var casters = await _queueInfo.GetCasterQueueInfo(context);
                var spectators = await _queueInfo.GetSpectatorQueueInfo(context);

                var gamesBeingPlayed = await _context.discord_channel_info.ToListAsync();

                string currentGames = await _info.CreateGameProfile(context, gamesBeingPlayed);
                var leaderboard = await _queueInfo.GetLeaderboard();
                var participationAward = await _queueInfo.GetParticipationAward();

                string open = string.Empty;

                if(TaskScheduler._inhouseOpen)
                {
                    open = "Open";
                }
                else
                {
                    open = "Closed";
                }

                string intro = DateTime.Now.ToString() + " PST\nWelcome to GrinHouseLeague. Inhouse opens at 8pmEST and closes at 1amEST. Inhouse is currently **" + open + "**. There are **" + gamesBeingPlayed.Count + " games** currently being played.\n\n";

                await ModifySegment(channel, intro, QueueMessageSegment.Introduction);

                if(await _queueInfo.MoreThanZeroPlayersIn(QueueType.Player) || _update)
                    await ModifySegment(channel, players, QueueMessageSegment.Queue);

                await ModifySegment(channel, leaderboard + participationAward, QueueMessageSegment.Leaderboard);

                if (await _queueInfo.MoreThanZeroPlayersIn(QueueType.Spectator) || _update)
                    await ModifySegment(channel, spectators, QueueMessageSegment.SpectatorQueue);

                if (await _queueInfo.MoreThanZeroPlayersIn(QueueType.Caster) || _update)
                    await ModifySegment(channel, casters, QueueMessageSegment.CasterQueue);

                if(gamesBeingPlayed.Count > 0)
                {
                    var ongoing = await channel.GetMessageAsync(PreLoadedMessages[QueueMessageSegment.OngoingGames]);
                    await ongoing.ModifyAsync(currentGames);
                }


                _update = false;
            }
            catch (ServerErrorException e)
            {
                await ReportErrorToAdmin(context, e);

            }
            catch (Exception e)
            {
                await ReportErrorToAdmin(context, e);
            }
            finally
            {
                await Task.Delay(2400);

                await UpdateQueue(context);
            }
                

            

        }

        public async Task ModifySegment(DiscordChannel channel, string message, QueueMessageSegment segment)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            .AddField(PreLoadedTitles[segment], message, false)
            .WithColor(DiscordColor.Blue);


            builder.Footer = new EmbedFooter() { Text = "" };

            DiscordEmbed embed = builder.Build();

            var discordmessage = await channel.GetMessageAsync(PreLoadedMessages[segment]);
            await discordmessage.ModifyAsync("", embed);
        }

        public async Task UpdateLeaderboard(CommandContext context)
        {

            try
            {

                if(!await Bot._validator.Exists(context, "leaderboard"))
                {
                    return;
                }

                var leaderboardChannel = await Bot._validator.Get(context, "leaderboard");

                var availableLeaderboardMessages = await _context.leaderboard_messages.ToListAsync();

                var leaderboardMessages = await GetSplitMessages(context);

                int neededChannels = leaderboardMessages.Count() - availableLeaderboardMessages.Count();
                for (int i = 0; i < neededChannels; i++)
                {
                    var message = await leaderboardChannel.SendMessageAsync("Starting new message...");
                    await _context.leaderboard_messages.AddAsync(new LeaderboardData { _message = message.Id }); //TO BE SAVED IN THE LOOP! hopefully does not crash uwu

                }

                int counter = 0;
                foreach (var message in availableLeaderboardMessages)
                {

                    var newMessage = await leaderboardChannel.GetMessageAsync(message._message);
                    if (newMessage != null)
                    {

                        await newMessage.ModifyAsync(leaderboardMessages[counter]);
                    }

                    counter++;
                }
            }
            catch (ServerErrorException e)
            {
                await ReportErrorToAdmin(context, e);

            }
            catch (Exception e)
            {
                await ReportErrorToAdmin(context, e);
            }



                //await UpdateLeaderboard(context);
                
        }

        public async Task<List<string>> GetSplitMessages(CommandContext context)
        {
            var players = await _context.player_data.ToListAsync();
            players = players.OrderByDescending(p => p._ihlmmr).ToList();
            string fulllist = "";
            int counter = 0;
            List<string> allMessages = new List<string>();// = SeperateText(fulllist);
            int discordMessageCharLimit = 1000;

            foreach (var player in players)
            {
                counter++;

                //this wont always return the player, since sometimes they are not considered a guild member.
                //var member = context.Guild.Members[player._id];
                //int mmr = calculator.GetMMR(context, member);
                
                string newstring = "-- #" + counter.ToString() + ": <@" + player._id + "> / " + player._ihlmmr + " grin mmr / " + player._dotammr/*mmr*/ + " dota mmr / " + player._gameswon + "W/"+ player._gameslost +"L -- \n";
                fulllist += newstring;

                if (fulllist.Length > discordMessageCharLimit)
                {
                    allMessages.Add(fulllist);
                    fulllist = "";
                }
            }
            allMessages.Add(fulllist);
            return allMessages;

        }

                
        

        private List<string> SeperateText(string fullstring)
        {
            int stringTotalLength = 0;
            stringTotalLength = fullstring.Length;

            List<string> partitions = new List<string>();
            int discordMessageCharacterLimit = 1000;

            if (stringTotalLength >= discordMessageCharacterLimit)
            {

                int numOfPartitions = stringTotalLength / discordMessageCharacterLimit;
                if (stringTotalLength % discordMessageCharacterLimit != 0)
                {
                    numOfPartitions += 1;
                }


                for (int i = 0; i < numOfPartitions; i++)
                {

                    var substring = fullstring.Substring(
                        discordMessageCharacterLimit * i,
                        GetTrueMessageLength(i, discordMessageCharacterLimit, stringTotalLength)
                        );
                    //var tempstring = array.ToArray();
                    //string newstring = new string(tempstring);
                    partitions.Add(substring);
                }
            }
            else
            {
                partitions.Add(fullstring);

            }
            return partitions;
        }

        public int GetTrueMessageLength(int iterator, int messageCap, int stringLength)
        {
            int remainder = 0;
            if (messageCap * iterator + messageCap > stringLength)
            {
                remainder = stringLength % messageCap - 1;
            }
            else
            {
                remainder = messageCap;
            }
            return remainder;
        }
        public async Task ReportErrorToAdmin(CommandContext context, Exception e)
        {
            Console.WriteLine(e);
            if (context != null)
            {
                if (context.Guild != null)
                {
                    if (context.Guild.Members.ContainsKey(Bot._admins.Admin))
                    {
                        var pip = context.Guild.Members[Bot._admins.Admin];
                        await pip.SendMessageAsync(e.ToString());
                    }
                }
            }
        }

    }


}
