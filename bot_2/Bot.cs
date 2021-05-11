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

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;

            Client.MessageCreated += OnMessage;
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
            //Commands.RegisterCommands<ItemCommands>();
            //Commands.RegisterCommands<ProfileCommands>();
            //Commands.RegisterCommands<TeamCommands>();

            Client.ConnectAsync();
        }

        private async Task OnMessage(DiscordClient c, MessageCreateEventArgs a)
        {
            if(a.Channel.Id != 838883268413620256)
            {
            }
           
        }
        private async Task UpdateMessage()
        {
            var first = Client.Guilds.First().Value;
            if(first == null)
            {
                Console.WriteLine("badkey guild");

            }

            ulong specificChannel = 774822115194699807;

            ulong preloadedID = 838232985919946763;

            if(first.Channels.ContainsKey(specificChannel))
            {
                Console.WriteLine("channel loaded");
            }
            else
            {
                Console.WriteLine("badkey channel");
            }
            Console.WriteLine(Client.Guilds.Count);
            Console.WriteLine(first.Channels.Count);
            var channel = first.Channels[specificChannel];
            try
            {
                var message = await first.Channels[specificChannel].GetMessageAsync(preloadedID);
                if (message == null)
                {
                    Console.WriteLine("was null");
                }

                await message.ModifyAsync("swaggy : " + DateTime.Now.ToString());

                await Task.Delay(1000);

                Console.WriteLine("trying");
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

        }

        private async Task OnClientReady(DiscordClient c, ReadyEventArgs e)
        {
            
            /*Task task = Task.Factory.StartNew(() =>
            {
               Task.Delay(3000);
            }, TaskCreationOptions.LongRunning);


            */
            //await UpdateMessage();
            //await Task.Delay(1000);
        }
    }
}
