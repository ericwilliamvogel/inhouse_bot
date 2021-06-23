using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using bot_2.Commands;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using System.Linq;
using DSharpPlus.Entities;

namespace bot_2
{
    /*
      SELECT * FROM primary_table;

INSERT INTO primary_table (_id, _name, _desc) VALUES(1, 'swag', 'bag');

DROP TABLE primary_table;

CREATE TABLE primary_table(
	_id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	_name varchar(25) NOT NULL,
	_desc varchar(25)
);
    */
    public class Bot
    {
        public DiscordClient Client { get; private set; }

        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public DotaClient DotaClient { get; set; }

        public static ChannelConfigJson Channels { get; set; }

        public ReactMessageConfigJson ReactMessages { get; set; }
        private QOL QOL { get; set; }
        private EmojiHandler _registration { get; set; }
        public Bot(IServiceProvider services)
        {
            QOL = new QOL();

            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);


            var channeljson = string.Empty;

            using (var fs = File.OpenRead("channelConfig.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                channeljson = sr.ReadToEnd();

            Channels = JsonConvert.DeserializeObject<ChannelConfigJson>(channeljson);

            var reactjson = string.Empty;

            using (var fs = File.OpenRead("reactConfig.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                reactjson = sr.ReadToEnd();

            ReactMessages = JsonConvert.DeserializeObject<ReactMessageConfigJson>(reactjson);

            SetExistingChannelCheck();

            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                //UseInternalLogHandler = true,
            };

            //DotaClient = new DotaClient();
            //DotaClient.Connect("zg12958703", "fuckiiou.");

            Client = new DiscordClient(config);

            Client.Resumed += OnClientReady;
            Client.MessageCreated += OnMessageCreated;
            Client.MessageReactionAdded += OnReactionAdded;
            /*Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            });*/

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableDms = false,
                EnableMentionPrefix = false,
                IgnoreExtraArguments = false,
                CaseSensitive = false,
                DmHelp = true,
                Services = services,
                UseDefaultCommandHandler = true //set to false to do Nyefan's UTF correction
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<StandardCommands>();
            Commands.RegisterCommands<AdminCommands>();
            Commands.RegisterCommands<CasterCommands>();
            Commands.RegisterCommands<QueueCommands>();
            Commands.RegisterCommands<LobbyCommands>();

            _registration = new EmojiHandler(this);


            Client.ConnectAsync();
        }

        private async Task OnReactionAdded(DiscordClient c, MessageReactionAddEventArgs args)
        {
            /*_ = args ?? throw new ArgumentNullException();

            _ = args.Guild ?? throw new ArgumentNullException();


            var channel = args.Guild.Channels[857007086311440414];

            if (args.Channel != channel)
                return;

            if (!IsCorrect(args.Message.Id))
                return;

            await _registration._reactLogic[args.Message.Id](args);*/
        }
        private bool IsCorrect(ulong id)
        {
            if (id == 857007304758526014 ||
                id == 857007319632183297 ||
                id == 857007374963441694 ||
                id == 857008472700747846 ||
                id == 857008502127067186 ||
                id == 857008523376197642 ||
                id == 857008551545536532 ||
                id == 857008579394535485 ||
                id == 857008588925304842 ||
                id == 857008592200269834 ||
                id == 857008595873693777 ||
                id == 857011121659052083 ||
                id == 857011160741576704)
            {
                return true;
            }
            return false;
        }



        private async Task OnClientReady(DiscordClient c, ReadyEventArgs e)
        {
            await Task.CompletedTask;
            //await UpdatedQueue.ResetVariables();
            //Console.WriteLine("Variables reset, new update thread should've been started");
        }

        private List<ulong> _existingChannelCheck;

        private void SetExistingChannelCheck()
        {
            _existingChannelCheck = new List<ulong>();
            _existingChannelCheck.Add(Channels.CommandsChannel);
            _existingChannelCheck.Add(Channels.QueueChannel);
            _existingChannelCheck.Add(Channels.CommandsPlaybackChannel);
            _existingChannelCheck.Add(Channels.AdminCommandsChannel);

        }
        private async Task OnMessageCreated(DiscordClient c, MessageCreateEventArgs e)
        {
            try
            {
                _ = e ?? throw new ArgumentNullException();

                _ = e.Guild ?? throw new ArgumentNullException();


                if (QOL.ChannelsExist(e, _existingChannelCheck))
                {
                    if (e.Channel == e.Guild.Channels[Channels.CommandsChannel] ||
                        e.Channel == e.Guild.Channels[Channels.QueueChannel] ||
                        e.Channel == e.Guild.Channels[Channels.AdminCommandsChannel])
                    {
                        Task task = await Task.Factory.StartNew(async () =>
                        {
                            await Task.Delay(4000);

                            var channel = e.Guild.Channels[Channels.CommandsPlaybackChannel];
                            await channel.SendMessageAsync("<@" + e.Message.Author.Id + "> : " + e.Message.Content);
                            if (e != null)
                            {
                                if (e.Message != null)
                                {
                                    await e.Message.DeleteAsync();
                                }
                            }

                        }, TaskCreationOptions.LongRunning);



                    }
                }



            }
            catch(Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }

        }


    }
}
