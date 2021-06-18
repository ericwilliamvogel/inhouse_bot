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

namespace bot_2.Commands
{
    public class UpdatedQueue
    {
        Context _context;
        GeneralDatabaseInfo _info;
        QueueInfo _queueInfo;
        static bool started = false;
        public ulong PreLoadedChannel { get; private set; }
        public ulong PreLoadedMessage { get; private set; }

        
        public UpdatedQueue(Context context)
        {
            this._context = context;
            _info = new GeneralDatabaseInfo(context);
            _queueInfo = new QueueInfo(context);
            ReadJsonFile();
        }



        public static async Task ResetVariables()
        {
            started = false;
            await Task.CompletedTask;
        }
        private void ReadJsonFile()
        {
            var json = string.Empty;

            using (var fs = File.OpenRead("channelConfig.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();

            var channelConfigJson = JsonConvert.DeserializeObject<ChannelConfigJson>(json);
            PreLoadedChannel = channelConfigJson.Channel;
            PreLoadedMessage = channelConfigJson.Message;
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
        public async Task UpdateQueue(CommandContext context)
        {

            try
            {
                if(Conditions._locked)
                {
                    return;
                }

                var queueMessage = await GetMessage(context, PreLoadedChannel, PreLoadedMessage);
                if (queueMessage == null)
                {
                    return;
                }



                List<QueueData> recordsToBeRemoved = new List<QueueData>();

                var players = await _queueInfo.GetPlayerQueueInfo(context);
                var casters = await _queueInfo.GetCasterQueueInfo(context);
                var spectators = await _queueInfo.GetSpectatorQueueInfo(context);

                //this await triggers threading issue??
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

                string finalString = DateTime.Now.ToString() + " PST\nWelcome to GrinHouseLeague. Inhouse opens at 10pmEST and closes at 1amEST. Inhouse is currently **" + open + "**. There are **" + gamesBeingPlayed.Count + " games** currently being played.\n\n" + leaderboard + participationAward +
                    players +
                    "\n\n" +
                    casters +
                    "\n\n" +
                    spectators +
                    "\n\n" +
                    //availableplayers +
                    //"\n\n" +
                    currentGames;

                await queueMessage.ModifyAsync(finalString);

                //await UpdateLeaderboard(context);


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

        public async Task UpdateLeaderboard(CommandContext context)
        {

                try
                {


                    if (!context.Guild.Channels.ContainsKey(851934555350630420))
                    {
                        return;
                    }



                    DiscordChannel leaderboardChannel = context.Guild.Channels[851934555350630420];

                    var availableLeaderboardMessages = await _context.leaderboard_messages.ToListAsync();

                    var leaderboardMessages = await GetSplitMessages();

                    int neededChannels = leaderboardMessages.Count() - availableLeaderboardMessages.Count();
                    for (int i = 0; i < neededChannels; i++)
                    {
                        var message = await leaderboardChannel.SendMessageAsync("Starting new message...");
                        await _context.leaderboard_messages.AddAsync(new LeaderboardData { _message = message.Id });
                        await _context.SaveChangesAsync();
                    }

                    int counter = 0;
                    foreach (var message in availableLeaderboardMessages)
                    {

                        var newMessage = await context.Guild.Channels[851934555350630420].GetMessageAsync(message._message);
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

    
        public async Task<List<string>> GetSplitMessages()
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
                string newstring = "-- #" + counter.ToString() + ": <@" + player._id + "> / " + player._ihlmmr + " grin mmr / " + player._dotammr + " dota mmr -- \n";
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
                    if (context.Guild.Members.ContainsKey(126922582208282624))
                    {
                        var pip = context.Guild.Members[126922582208282624];
                        await pip.SendMessageAsync(e.ToString());
                    }
                }
            }
        }

    }


}
