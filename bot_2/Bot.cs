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

            //Client.Ready += OnClientReady;
            Client.Resumed += OnClientReady;

            //Client.MessageCreated += OnMessage;
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
            //Commands.RegisterCommands<ProfileCommands>();
            //Commands.RegisterCommands<TeamCommands>();

            Client.ConnectAsync();
        }

        private async Task OnClientReady(DiscordClient c, ReadyEventArgs e)
        {
            await UpdatedQueue.ResetVariables();
            Console.WriteLine("Variables reset, new update thread should've been started");
        }
    }
}
