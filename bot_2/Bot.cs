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
using Newtonsoft.Json.Linq;

namespace bot_2
{

    public enum GameType
    {
        Dota2,
        PokemonUnite
    }

    public class Bot
    {
        public DiscordClient Client { get; private set; }

        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        private QOL QOL { get; set; }
        private EmojiHandler _registration { get; set; }

        public static FileValidator _validator;

        public static GameType _game;
        public static Positions _positions;

        public static Admins _admins;
        public Bot(IServiceProvider services)
        {
            QOL = new QOL();

            _positions = new Positions();
            _validator = new FileValidator();

            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);


            using (var fs = File.OpenRead("positions.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();

            _positions = JsonConvert.DeserializeObject<Positions>(json);

            var temptemp = JObject.Parse(json);
            if ((string)temptemp["Pos1"] == "")
            {
                string exception = "\n\n\n\n !!!! Position names have not been assigned. Go into positions.json to assign the names, then restart the bot. \n\n\n\n !! \n\n";
                Console.WriteLine(exception);
            }

            using (var fs = File.OpenRead("admins.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();

            _admins = JsonConvert.DeserializeObject<Admins>(json);

            temptemp = JObject.Parse(json);
            if ((string)temptemp["Admin"] == "")
            {
                string exception = "\n\n\n\n !!!! Main admin has not been assigned. Go into admins.json to assign the id, then restart the bot. \n\n\n\n !! \n\n";
                Console.WriteLine(exception);
            }

            using (var fs = File.OpenRead("game.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();

            temptemp = JObject.Parse(json);
            _game = (GameType)(int)temptemp["Game"];

            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                //UseInternalLogHandler = true,
            };

            Client = new DiscordClient(config);

            Client.Resumed += OnClientReady;
            Client.MessageCreated += OnMessageCreated;
            Client.MessageReactionAdded += OnReactionAdded;
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromDays(7)
            });

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
            Commands.RegisterCommands<ShopCommands>();

            _registration = new EmojiHandler(this, services);


            Client.ConnectAsync();
        }

        ActionIterator actions = new ActionIterator();
        private async Task OnReactionAdded(DiscordClient c, MessageReactionAddEventArgs args)
        {
            _ = args ?? throw new ArgumentNullException();

            _ = args.Guild ?? throw new ArgumentNullException();


            /*var channel = args.Guild.Channels[857308958300569661];

            if (args.Channel != channel)
                return;

            if (!IsCorrect(args.Message.Id))
                return;*/

            var channel = args.Channel;

            /*if (channel == await _validator.Get(args.Guild, "control-panel"))
            {

            }*/


            if(_validator._messages.ContainsKey(args.Message.Id))
            {
                await actions.AddAction(async () =>
                {
                    await _registration._reactLogic[_validator._messages[args.Message.Id]](args);
                });
            }








        }


        private async Task OnClientReady(DiscordClient c, ReadyEventArgs e)
        {
            await Task.CompletedTask;
            //await UpdatedQueue.ResetVariables();
            //Console.WriteLine("Variables reset, new update thread should've been started");
        }

        private async Task OnMessageCreated(DiscordClient c, MessageCreateEventArgs e)
        {
            try
            {
                //_ = e ?? throw new ArgumentNullException();

                //_ = e.Guild ?? throw new ArgumentNullException();

                if(e == null)
                {
                    return;
                }
                if(e.Guild == null)
                {
                    return;
                }

                if (e.Channel == await _validator.Get(e, "commands") ||
                    e.Channel == await _validator.Get(e, "queue") ||
                    e.Channel == await _validator.Get(e, "admin-commands") /*||
                    e.Channel == await _validator.Get(e, "setup")*/)
                {
                    Task task = await Task.Factory.StartNew(async () =>
                    {
                        await Task.Delay(4000);

                        var channel = await _validator.Get(e, "commands-playback");
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
            catch(Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }

        }


    }
}
