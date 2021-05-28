using db;
using DSharpPlus.CommandsNext;
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
                await UpdateMessage(context);


            }
        }
        public async Task UpdateMessage(CommandContext context)
        {
            try
            {

                if (context.Guild.Channels.ContainsKey(PreLoadedChannel))
                {
                    Console.WriteLine("channel loaded");
                }
                else
                {
                    Console.WriteLine("badkey channel");
                    return;
                }

                var channel = context.Guild.Channels[PreLoadedChannel];

                var message = await context.Guild.Channels[PreLoadedChannel].GetMessageAsync(PreLoadedMessage);
                if (message == null)
                {
                    Console.WriteLine("was null");
                    return;
                }

                List<QueueData> recordsToBeRemoved = new List<QueueData>();

                var gamesBeingPlayed = await _context.discord_channel_info.ToListAsync();

                var players = await _queueInfo.GetPlayerQueueInfo(context);
                var casters = await _queueInfo.GetCasterQueueInfo(context);
                var spectators = await _queueInfo.GetSpectatorQueueInfo(context);

                string currentGames = await _info.CreateGameProfile(context, gamesBeingPlayed);

                var leaderboard = await _queueInfo.GetLeaderboard();

                var participationAward = await _queueInfo.GetParticipationAward();


                string finalString = DateTime.Now.ToString() + " PST\nWelcome to GrinHouseLeague. There are **" + gamesBeingPlayed.Count + " games** currently being played.\n\n" + leaderboard + participationAward +
                    players +
                    "\n\n" +
                    casters +
                    "\n\n" +
                    spectators +
                    "\n\n" +
                    //availableplayers +
                    //"\n\n" +
                    currentGames;

                await message.ModifyAsync(finalString);
            }
            catch (ServerErrorException e)
            {
                await ReportErrorToAdmin(context, e);

            }
            catch (Exception e)
            {
                await ReportErrorToAdmin(context, e);
            }

            await Task.Delay(2000);

            await UpdateMessage(context);

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
