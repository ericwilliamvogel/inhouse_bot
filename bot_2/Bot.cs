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
        public Bot(IServiceProvider services)
        {
            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

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

            Client.ConnectAsync();
        }

        private async Task OnClientReady(DiscordClient c, ReadyEventArgs e)
        {
            //await UpdatedQueue.ResetVariables();
            //Console.WriteLine("Variables reset, new update thread should've been started");
        }

        private async Task OnMessageCreated(DiscordClient c, MessageCreateEventArgs e)
        {
            try
            {

                var array = e.Message.Content;
                char prefix;

                if(array !=null && array != "")
                prefix = array[0];

                if(e!=null)
                {
                    if(e.Guild !=null)
                    {
                        if (e.Guild.Channels.ContainsKey(839336462431289374) || e.Guild.Channels.ContainsKey(839331703776083989))
                        {
                            if (e.Channel == e.Guild.Channels[839336462431289374] || e.Channel == e.Guild.Channels[839331703776083989])
                            {
                                Task task = await Task.Factory.StartNew(async () =>
                                {
                                    await Task.Delay(6000);

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
                }

                /*var newmsg = array.Substring(1);


                //hard coded this cuz we dont know what prefix is?
                if (prefix == '!' && !Commands.RegisteredCommands.ContainsKey(newmsg))
                {
                    Task task = await Task.Factory.StartNew(async () =>
                    {
                        var msg = await e.Channel.SendMessageAsync(e.Author.Mention + ", '" + e.Message.Content + "' is an invalid command. Check the #commands channel to see valid commands.");

                        await e.Message.DeleteAsync();


                    }, TaskCreationOptions.LongRunning);

                }
                */
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }

        }
    }
}
