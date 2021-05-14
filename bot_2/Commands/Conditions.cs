using db;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_2.Commands
{
    public class Conditions
    {
        Context _context;

        /// <summary>
        /// 
        /// We should make an argument dictionary and an enum key????
        /// 
        /// </summary>
        public Argument IsReady { get; set; } //not occupied code 0
        public Argument IsRegistered { get; set; } //already !registered

        public Argument IsntRegistered { get; set; }
        public Argument IsInCommandChannel { get; set; }
        public Argument IsInAdminCommandChannel { get; set; }

        public Argument IsntQueued { get; set; }

        public Argument IsQueued { get; set; }

        public Argument HasAtLeastAdminRole { get; set; }

        public Argument HasAtLeastModRole { get; set; }

        public Argument HasAtLeastMemberRole { get; set; }

        public Argument HasAtLeastTrustedRole { get; set; }
        public Conditions(Context context)
        {
            _context = context;
            IsReady = async (CommandContext context, Profile _profile) =>
            {
                var record = await _context.player_data.FindAsync(_profile._id);
                if (record == null)
                    return false;

                if (record._gamestatus == 0) //notocc
                {
                    return true;
                }
                else //occ
                {
                    await _profile.SendDm("Our records show that you are already in a game. Some restrictions apply if you are already occupied, an example being you cannot queue.");
                }

                return false;
            };

            IsRegistered = async (CommandContext context, Profile _profile) =>
            {
                var checkIfRegistered = await _context.player_data.FindAsync(_profile._id);
                if (checkIfRegistered != null)
                {
                    return true;
                }
                else
                {
                    await _profile.SendDm("You aren't registered. Register by going to the #commands channel and typing !register your_steam_id_here to get signed up. We need you to register in order to track your GHL mmr after every game.");
                }

                return false;

            };

            IsntRegistered = async (CommandContext context, Profile _profile) =>
            {
                var checkIfRegistered = await _context.player_data.FindAsync(_profile._id);
                if (checkIfRegistered == null)
                {
                    return true;
                }
                else
                {
                    await _profile.SendDm("You're already registered.");
                }

                return false;

            };

            IsInCommandChannel = async (CommandContext context, Profile _profile) =>
            {
                ulong channel = 838883268413620256;
                string channelName = "<channel_not_found>";

                if (context.Guild.Channels.ContainsKey(channel))
                {
                    channelName = context.Guild.Channels[channel].Name;
                }

                if (context.Channel.Id == channel)
                {
                    return true;
                }
                else
                {
                    await _profile.SendDm("You cannot issue this command in this channel. This command can only be executed in channel '" + channelName + "'.");
                }
                return false;

            };

            IsInCommandChannel = CorrectChannel(839336462431289374);
            IsInAdminCommandChannel = CorrectChannel(839336496484974622);

            IsntQueued = async (CommandContext context, Profile _profile) =>
            {
                var player = await _context.player_queue.FindAsync(_profile._id);
                if (player == null)
                {
                    return true;
                }
                else
                {
                    await _profile.SendDm("You're already in queue.");
                    return false;
                }
            };

            IsQueued = async (CommandContext context, Profile _profile) =>
            {
                var player = await _context.player_queue.FindAsync(_profile._id);
                if (player != null)
                {
                    return true;
                }
                else
                {
                    await _profile.SendDm("You're not in any queue.");
                    return false;
                }
            };

            HasAtLeastAdminRole = CorrectRole("Administration");

            HasAtLeastModRole = CorrectRole("Mod");

            HasAtLeastTrustedRole = CorrectRole("Trusted");

            HasAtLeastMemberRole = CorrectRole("Member");


        }

        public Argument CorrectChannel(ulong id)
        {
            Argument args = async (CommandContext context, Profile _profile) =>
            {
                ulong channel = id;
                string channelName = "<channel_not+found>";

                if (context.Guild.Channels.ContainsKey(channel))
                {
                    channelName = context.Guild.Channels[channel].Name;
                }

                if (context.Channel.Id == channel)
                {
                    return true;
                }
                else
                {
                    await _profile.SendDm("You cannot issue this command in this channel. This command can only be executed in channel '" + channelName + "'.");
                }
                return false;
            };

            return args;
        }

        public Argument CorrectRole(string roleName)
        {
            Argument args = async (CommandContext context, Profile _profile) =>
            {

                var role = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == roleName).Value; //i dont think we can access value if it returns null? could cause errors, kinda spooky
                if(role == null)
                {
                    await _profile.SendDm("Role check for '" + roleName + "' was not found. The check for the role is case sensitive, this may be on the dev end to fix! Please open a ticket to report the issue.");
                    return false;
                }


                if(_profile._member.Roles.Contains(role))
                {
                    return true;
                }
                else
                {
                    await _profile.SendDm("You don't have the minimum role necessary to execute this command.");
                }
                return false;
            };

            return args;
        }

        public async Task<bool> AreMet(CommandContext context, Profile _profile, List<Argument> tasks)
        {
            bool pass = true;
            foreach (Argument task in tasks)
            {
                var conf = await task(context, _profile);
                if (conf == false)
                {
                    pass = false;
                }
            }
            if (!pass)
            {
                return false;
            }

            return true;

        }

        public async Task TryConditionedAction(CommandContext context, Profile _profile, List<Argument> tasks, Action action)
        {
            var conditions = await AreMet(context, _profile, tasks);

            if(!conditions)
            {
                await context.Message.DeleteAsync();
                return;
            }
            

            try
            {
                action();
                await context.Message.DeleteAsync();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }


        }

    }
}
