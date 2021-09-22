using bot_2.Commands;
using bot_2.Json;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_2
{
    public enum GameType
    {
        Dota2,
        PokemonUnite
    }

    public class DiscordRoleValidation //THIS needs to be json'd 
    {
        public List<string> _roleNames = new List<string>(); //read positions

        public void SetNames()
        {
            SetNamesDota();
        }

        private void SetNamesPokemon()
        {

        }

        private void SetNamesDota()
        {
            _roleNames = new List<string>();

            /*_roleNames.Add("Member");
            _roleNames.Add("Trusted");
            _roleNames.Add("Moderator");
            */
            _roleNames.Add(Bot._positions.Pos1);
            _roleNames.Add(Bot._positions.Pos2);
            _roleNames.Add(Bot._positions.Pos3);
            _roleNames.Add(Bot._positions.Pos4);
            _roleNames.Add(Bot._positions.Pos5);

            _roleNames.Add(Bot._positions.Pos1 + "(Favorite)");
            _roleNames.Add(Bot._positions.Pos2 + "(Favorite)");
            _roleNames.Add(Bot._positions.Pos3 + "(Favorite)");
            _roleNames.Add(Bot._positions.Pos4 + "(Favorite)");
            _roleNames.Add(Bot._positions.Pos5 + "(Favorite)");
            /*
            _roleNames.Add("Player Drafter");
            _roleNames.Add("Dota Drafter");

            _roleNames.Add("0");
            _roleNames.Add("1");
            _roleNames.Add("2");
            _roleNames.Add("3");
            _roleNames.Add("4");
            _roleNames.Add("5");

            _roleNames.Add("Herald");
            _roleNames.Add("Guardian");
            _roleNames.Add("Crusader");
            _roleNames.Add("Archon");
            _roleNames.Add("Legend");
            _roleNames.Add("Ancient");
            _roleNames.Add("Divine");
            _roleNames.Add("Immortal");

            _roleNames.Add("Registered");*/
        }

        public async Task ReportErrorToAdmin(CommandContext context, string e)
        {
            Console.WriteLine(e);
            if (context != null)
            {
                if (context.Guild != null)
                {
                    if (context.Guild.Members.ContainsKey(Bot._admins.Admin))
                    {
                        var admin = context.Guild.Members[Bot._admins.Admin];
                        await admin.SendMessageAsync(e.ToString());
                    }
                }
            }
        }

        public async Task Run(CommandContext context)
        {
            var roles = context.Guild.Roles;
            Profile _profile = new Profile(context);
            foreach (string name in _roleNames)
            {

                var target = roles.FirstOrDefault(x => x.Value.Name == name).Value;

                if (target == null)
                {

                    var role = await context.Guild.CreateRoleAsync(name);
                    await _profile.ReportError(context, "Role '" + name + "' created.");

                }
                else
                {
                    //await target.DeleteAsync();
                    //await _profile.ReportError(context, "Role '" + name + "' NOT created, was already found.");
                    //LOG error

                }
            }
        }
    }

    public class DiscordCategoryValidation
    {
        public List<string> _channelNames = new List<string>();
        public string _categoryName;

        public DiscordCategoryValidation(string name, List<string> input)
        {
            _categoryName = name;
            _channelNames = input;
        }

        protected void SetNames()
        {

        }
    }


 
    public class JsonCollection
    {
        public List<ChannelRecord> _channels;
        public void Get(string file)
        {
            //open up each file
            string path = Directory.GetCurrentDirectory() + @file;
            string text = File.ReadAllText(path);
            var obj = JObject.Parse(text);

            foreach(var channel in obj["Channels"])
            {
                var newObj = (JObject)JsonConvert.DeserializeObject(channel.ToString());
                _channels.Add(new ChannelRecord() { Id = (ulong)newObj["Id"], Name = (string)newObj["Name"] });
            }
        }
    }
    public class FileValidator //startup
    {
        //check all channels in discord to make sure they have every name in setup
        //if name not found create the channel
        //then assign the ID in json documents OR eliminate json documents
        //maybe just have ONE config file for startup, noting the different categories and channels to validate
        //assign permissions to categories if they were created

        private List<DiscordCategoryValidation> _channelCollection = new List<DiscordCategoryValidation>();

        private DiscordRoleValidation _roleCollection = new DiscordRoleValidation();

        public List<ChannelRecord> _records = new List<ChannelRecord>();

        public Dictionary<ulong, string> _messages { get; set; }
        public FileValidator()
        {
            JsonCommunicator comm = new JsonCommunicator();
            _messages = CreateMessageDic();
        }

        public Dictionary<ulong, string> CreateMessageDic()
        {
            Dictionary<ulong, string> dic = new Dictionary<ulong, string>();
            dic.Add(DicPair("Rank").Key, DicPair("Rank").Value);
            dic.Add(DicPair("RankProgress").Key, DicPair("RankProgress").Value);
            dic.Add(DicPair("Region").Key, DicPair("Region").Value);
            dic.Add(DicPair("PrefPos").Key, DicPair("PrefPos").Value);
            dic.Add(DicPair("Pos").Key, DicPair("Pos").Value);
            dic.Add(DicPair("misc", "notifications").Key, DicPair("misc", "notifications").Value);
            dic.Add(DicPair("misc", "profile").Key, DicPair("misc", "profile").Value);

            return dic;
        }

        public KeyValuePair<ulong, string> DicPair(string input)
        {
            JsonCommunicator comm = new JsonCommunicator();
            KeyValuePair<ulong, string> pair = new KeyValuePair<ulong, string>(comm.GetValue("registration", input), input);
            return pair;
        }

        public KeyValuePair<ulong, string> DicPair(string file, string input)
        {
            JsonCommunicator comm = new JsonCommunicator();
            KeyValuePair<ulong, string> pair = new KeyValuePair<ulong, string>(comm.GetValue(file, input), input);
            return pair;
        }

        protected void SetNames()
        {
            _roleCollection = new DiscordRoleValidation();
            _channelCollection = new List<DiscordCategoryValidation>();

            _roleCollection.SetNames();

            _channelCollection.Add(new DiscordCategoryValidation("setup", new List<string>() { "setup", "registration" }));

            _channelCollection.Add(new DiscordCategoryValidation("inhouse", new List<string>() { "control-panel", "queue", "inhouse-general", "commands", "game-history", "streams", "memes", "feedback", "leaderboard", "bet-history", "shop" }));

            _channelCollection.Add(new DiscordCategoryValidation("admin", new List<string>() { "admin-commands", "admins-chat", "error-logs", "commands-playback" }));

            _channelCollection.Add(new DiscordCategoryValidation("trusted", new List<string>() { "general" }));
        }

        public async Task<DiscordMessage> GetMessage(MessageReactionAddEventArgs e, string messageName)
        {
            JsonCommunicator comm = new JsonCommunicator();
            var val = comm.GetValue("setup", messageName);
            var channel = await Get(e.Guild, "setup");

            var message = await channel.GetMessageAsync(val);

            return message;
        }

        public async Task<DiscordMessage> GetQueueMessage(MessageReactionAddEventArgs e)
        {
            JsonCommunicator comm = new JsonCommunicator();
            var val = comm.GetValue("misc", "queue_message");
            var channel = await Get(e.Guild, "misc");

            var message = await channel.GetMessageAsync(val);

            return message;
        }
        public async Task<DiscordChannel> Get(MessageCreateEventArgs e, string channelName)
        {
            if (_channelCollection.Count <= 0)
            {
                SetNames();
                await Validate(e.Guild);
            }

            var record = _records.FirstOrDefault(p => p.Name == channelName);
            if (record != null)
            {
                return e.Guild.Channels[record.Id];
            }
            else
            {
                return null;
            }

        }
        public async Task<DiscordChannel> Get(CommandContext context, string channelName)
        {
            if (_channelCollection.Count <= 0)
            {
                SetNames();
                await Validate(context);
            }

            var record = _records.FirstOrDefault(p => p.Name == channelName);
            if (record != null)
            {
                return context.Guild.Channels[record.Id];
            }
            else
            {
                Profile profile = new Profile(context);
                await profile.ReportError(context, "Channel not found under ID!");
                return null;
            }

        }

        public async Task<DiscordChannel> Get(DiscordGuild guild, string channelName)
        {
            if (_channelCollection.Count <= 0)
            {
                SetNames();
                await Validate(guild);
            }

            var record = _records.FirstOrDefault(p => p.Name == channelName);
            if (record != null)
            {
                return guild.Channels[record.Id];
            }
            else
            {
                //Profile profile = new Profile(context);
                //await profile.ReportError(context, "Channel not found under ID!");
                return null;
            }

        }
        public async Task<bool> Exists(CommandContext context, string channelName)
        {
            if (_channelCollection.Count <= 0)
            {
                SetNames();
                await Validate(context);
            }

            var record = _records.FirstOrDefault(p => p.Name == channelName);
            if (record != null)
            {
                return true;
            }
            else
            {
                Profile profile = new Profile(context);
                await profile.ReportError(context, "Channel not found under ID!");
                return false;
            }

        }

        public async Task Validate(DiscordGuild guild)
        {

            foreach (var validation in _channelCollection)
            {
                JsonCommunicator comm = new JsonCommunicator();
                var findCategory = guild.Channels.FirstOrDefault(p => p.Value.Name == validation._categoryName).Value;
                if (findCategory == null)
                {
                    var parent = await guild.CreateChannelCategoryAsync(validation._categoryName);
                    ///create category
                    //
                    List<ChannelRecord> records = new List<ChannelRecord>();
                    foreach (string channel in validation._channelNames)
                    {
                        //create channel, set parent, record id
                        var newChannel = await guild.CreateChannelAsync(channel, DSharpPlus.ChannelType.Text, parent);

                        var channelRecord = new ChannelRecord() { Id = newChannel.Id, Name = newChannel.Name };
                        records.Add(channelRecord);
                        _records.Add(channelRecord);
                    }

                    JsonRecord record = new JsonRecord() { Channels = records };


                    comm.ModifyFile(validation._categoryName, comm.ToString(record));
                    //create Category, create all channels in category, adjust json
                }
                else
                {

                    List<ChannelRecord> records = new List<ChannelRecord>();
                    foreach (string channel in validation._channelNames)
                    {
                        //find the channel
                        var discordchannel = guild.Channels.FirstOrDefault(p => p.Value.Name == channel && p.Value.Parent == findCategory).Value;
                        if (discordchannel == null)
                        {
                            var newChannel = await guild.CreateChannelAsync(channel, DSharpPlus.ChannelType.Text, findCategory);

                            var channelRecord = new ChannelRecord() { Id = newChannel.Id, Name = newChannel.Name };
                            records.Add(channelRecord);
                            _records.Add(channelRecord);
                        }
                        else
                        {
                            var channelRecord = new ChannelRecord() { Id = discordchannel.Id, Name = discordchannel.Name };
                            records.Add(channelRecord);
                            _records.Add(channelRecord);
                        }
                    }

                    JsonRecord record = new JsonRecord() { Channels = records };
                    //_records.Add(record);
                    comm.ModifyFile(validation._categoryName, comm.ToString(record));
                }

            }
        }

        public async Task Validate(CommandContext context)
        {
            await Run(context);
            foreach (var validation in _channelCollection)
            {
                JsonCommunicator comm = new JsonCommunicator();
                var findCategory = context.Guild.Channels.FirstOrDefault(p => p.Value.Name == validation._categoryName).Value;
                if (findCategory == null)
                {
                    var parent = await context.Guild.CreateChannelCategoryAsync(validation._categoryName);
                    ///create category
                    //
                    List<ChannelRecord> records = new List<ChannelRecord>();
                    foreach (string channel in validation._channelNames)
                    {
                        //create channel, set parent, record id
                        var newChannel = await context.Guild.CreateChannelAsync(channel, DSharpPlus.ChannelType.Text, parent);

                        var channelRecord = new ChannelRecord() { Id = newChannel.Id, Name = newChannel.Name };
                        records.Add(channelRecord);
                        _records.Add(channelRecord);
                    }

                    JsonRecord record = new JsonRecord() { Channels = records };


                    comm.ModifyFile(validation._categoryName, comm.ToString(record));
                    //create Category, create all channels in category, adjust json
                }
                else
                {

                    List<ChannelRecord> records = new List<ChannelRecord>();
                    foreach (string channel in validation._channelNames)
                    {
                        //find the channel
                        var discordchannel = context.Guild.Channels.FirstOrDefault(p => p.Value.Name == channel && p.Value.Parent == findCategory).Value;
                        if (discordchannel == null)
                        {
                            var newChannel = await context.Guild.CreateChannelAsync(channel, DSharpPlus.ChannelType.Text, findCategory);

                            var channelRecord = new ChannelRecord() { Id = newChannel.Id, Name = newChannel.Name };
                            records.Add(channelRecord);
                            _records.Add(channelRecord);
                        }
                        else
                        {
                            var channelRecord = new ChannelRecord() { Id = discordchannel.Id, Name = discordchannel.Name };
                            records.Add(channelRecord);
                            _records.Add(channelRecord);
                        }
                    }

                    JsonRecord record = new JsonRecord() { Channels = records };
                    //_records.Add(record);
                    comm.ModifyFile(validation._categoryName, comm.ToString(record));
                }

            }
        }
        public async Task Run(CommandContext context)
        {
            await _roleCollection.Run(context);
        }
    }

    
}
