using db;
using DSharpPlus.CommandsNext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_2.Commands
{
    public class ContextualAction
    {
        public ContextualAction(CommandContext context, Func<Task> action)
        {
            Profile profile = new Profile(context);
            _commandContext = context;
            _action = action;
            _profile = profile;
        }
        public Func<Task> _action { get; set; }
        public Profile _profile { get; set; }

        public CommandContext _commandContext { get; set; }

        public Func<Task> GetAction()
        {
            Func<Task> action = async () =>
            {

                await _action();
            };

            return action;
        }
    }
    public class ActionIterator
    {
        public List<Func<Task>> _actions = new List<Func<Task>>();
        public async Task AddAction(ContextualAction action)
        {
            _actions.Add(async () =>
            {
                await action._action();
                await action._commandContext.Message.DeleteAsync();
                await CompleteAction();
            });


            await CompleteAction();
        }
        public async Task AddDelayedAction(int ms, ContextualAction action)
        {
            await Task.Delay(ms);

            _actions.Add(async () =>
            {
                await action._action();
                await action._commandContext.Message.DeleteAsync();
                await CompleteAction();
            });


            await CompleteAction();
        }
        private bool active = false;
        public async Task CompleteAction()
        {
            if(!active && _actions.Count > 0)
            {
                active = true;
                await _actions[0]();
            }
            else if(active && _actions.Count > 0)
            {
                _actions.Remove(_actions[0]);
                if(_actions.Count <= 0)
                {
                    active = false;
                }
                else
                {
                    await _actions[0]();
                }
            }
        }
    }
    public enum Arg
    {
        IsReady,
        IsRegistered,
        IsntRegistered,
        IsInCommandChannel,
        IsInAdminCommandChannel,
        IsntQueued,
        IsQueued,
        IsntSpectatorQueued,
        IsSpectatorQueued,
        IsntCasterQueued,
        IsCasterQueued,
        HasAdminRole,
        HasModRole,
        HasMemberRole,
        HasTrustedRole,
        HasCasterRole,
        IsLobbyCaptain,
        CanPick,
        HasMention,
        ProfileComplete,
        IsLobbyHost,
        CanEmote,
        InhouseIsOpen
    }
    public class Conditions
    {
        Context _context;

        /// <summary>
        /// 
        /// We should make an argument dictionary and an enum key????
        /// 
        /// </summary>
        /// 

        private QOL QOL = new QOL();
        public Dictionary<Arg, Argument> _check;
        public Conditions(Context context)
        {
            //_actionIterator = new ActionIterator();
            _context = context;
            _check = new Dictionary<Arg, Argument>();
            _check.Add(
                Arg.IsReady,
                async (CommandContext context, Profile _profile) =>
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
                        await _profile.SendDm("Our records show that you are already in a game/already in a queue. Some restrictions apply if you are already occupied, an example being you cannot queue.");
                    }

                    return false;
                }

            );

                _check.Add(
                    Arg.IsLobbyHost,
                    async (CommandContext context, Profile _profile) =>
                    {
                        var checkIfHost = await _context.discord_channel_info.FindAsync(_profile._id);
                        if (checkIfHost != null)
                        {
                            return true;
                        }
                        else
                        {
                            await _profile.SendDm("You need to be a lobby host, and our records show that you aren't the host of any lobby.");
                        }

                        return false;
                    }

                );
                        _check.Add(
                Arg.IsRegistered,
                async (CommandContext context, Profile _profile) =>
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
                }

            );

            _check.Add(
                Arg.IsntRegistered,
                async (CommandContext context, Profile _profile) =>
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
                }

            );
            _check.Add(
                Arg.IsInCommandChannel,

                    async (CommandContext context, Profile _profile) =>
                    {
                        ulong channel = 839331703776083989;
                        ulong channel2 = 839336462431289374;

                        if (context.Channel.Id == channel || context.Channel.Id == channel2)
                        {
                            return true;
                        }
                        else
                        {
                            await _profile.SendDm("You cannot issue this command in this channel. This command can only be executed in channel '#commands'.");
                        }
                        return false;
                    }
            );
            _check.Add(
                Arg.IsInAdminCommandChannel, CorrectChannel(839336496484974622)
            );

            _check.Add(
                Arg.IsntQueued,
                async (CommandContext context, Profile _profile) =>
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
                }

            );
            _check.Add(
                Arg.IsQueued,
                async (CommandContext context, Profile _profile) =>
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
                }

                );
            _check.Add(
                Arg.IsntCasterQueued,
                async (CommandContext context, Profile _profile) =>
                {
                    var player = await _context.caster_queue.FindAsync(_profile._id);
                    if (player == null)
                    {
                        return true;
                    }
                    else
                    {
                        await _profile.SendDm("You're already in queue.");
                        return false;
                    }
                }

                );
            _check.Add(
                Arg.IsCasterQueued,
                async (CommandContext context, Profile _profile) =>
                {
                    var player = await _context.caster_queue.FindAsync(_profile._id);
                    if (player != null)
                    {
                        return true;
                    }
                    else
                    {
                        await _profile.SendDm("You're not in any queue.");
                        return false;
                    }
                }

                );
            _check.Add(
                Arg.IsntSpectatorQueued,
                async (CommandContext context, Profile _profile) =>
                {
                    var player = await _context.spectator_queue.FindAsync(_profile._id);
                    if (player == null)
                    {
                        return true;
                    }
                    else
                    {
                        await _profile.SendDm("You're already in queue.");
                        return false;
                    }
                }

                );
            _check.Add(
                Arg.IsSpectatorQueued,
                async (CommandContext context, Profile _profile) =>
                {
                    var player = await _context.spectator_queue.FindAsync(_profile._id);
                    if (player != null)
                    {
                        return true;
                    }
                    else
                    {
                        await _profile.SendDm("You're not in any queue.");
                        return false;
                    }
                }

                );

            _check.Add(
                Arg.HasAdminRole,
                CorrectRole("Administration")
                );
            _check.Add(
                Arg.HasModRole,
                CorrectRole("Mod")
                );
            _check.Add(
            Arg.HasTrustedRole,
            CorrectRole("Trusted")
            );
                        _check.Add(
            Arg.HasMemberRole,
            CorrectRole("Member")
            );
                        _check.Add(
            Arg.HasCasterRole,
            CorrectRole("Caster")
            );

            _check.Add(Arg.IsLobbyCaptain,
                async (CommandContext context, Profile _profile) =>
                { 
                    var record = await _context.game_record.FirstOrDefaultAsync(p => p._p1 == _profile._id && p._p5 == 0);
                    if(record == null)
                    {
                        await _profile.SendDm("You were not found as a leader in any database records.");
                        return false;
                    }
                    else
                    {
                        return true;
                    }

                }
            );

            _check.Add(Arg.CanEmote,
                async (CommandContext context, Profile _profile) =>
                {
                    var record = await _context.emote_unlocked.FindAsync(_profile._id);

                    if (record == null)
                    {
                        await _profile.SendDm("You do not have emote permissions.");
                        return false;
                    }
                    else
                    {
                        return true;
                    }

                }
            );

            _check.Add(Arg.CanPick,
                async (CommandContext context, Profile _profile) =>
                {
                    var record = await _context.game_record.FirstOrDefaultAsync(p => p._p1 == _profile._id && p._p5 == 0);
                    if (record == null)
                    {
                        await _profile.SendDm("You were not found as a leader in any database records.");
                        return false;
                    }

                    if(record._canpick == 1)
                    {
                        return true;
                    }
                    else if(record._canpick == 0)
                    {
                        await _profile.SendDm("It's not your turn to pick.");
                        return false;
                    }

                    return true;


                });

            _check.Add(Arg.HasMention,
                async (CommandContext context, Profile _profile) =>
                {
                    if (context.Message.MentionedUsers.Count > 0)
                    {
                        return true;
                    }

                    await _profile.SendDm("There were no mentions recognized in your command.");
                    return false;

                });

            _check.Add(Arg.ProfileComplete,
                    async (CommandContext context, Profile _profile) =>
                    {
                        /*
                        var role1 = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == "c_rank").Value;
                        var role2 = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == "c_region_pref").Value;
                        var role3 = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == "c_playercap_pref").Value;
                        var role4 = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == "c_dotacap_pref").Value;
                        var role5 = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == "c_favrole_pref").Value;
                        var role6 = context.Guild.Roles.FirstOrDefault(p => p.Value.Name == "c_positions_pref").Value;

                        if (!_profile._member.Roles.Contains(role1) &&
                        !_profile._member.Roles.Contains(role2) &&
                        !_profile._member.Roles.Contains(role3) &&
                        !_profile._member.Roles.Contains(role4) &&
                        !_profile._member.Roles.Contains(role5) &&
                        !_profile._member.Roles.Contains(role6))
                        {
                            return true;
                        }
                        */
                        var record = await _context.player_data.FindAsync(_profile._id);
                        if (record._role1 != 0 && record._role2 != 0 && record._region != (int)Region.NONE && record._dotammr != 0)
                        {
                            return true;
                        }

                        await _profile.SendDm("Your profile is incomplete, please type !preferences for information on how to complete your profile.");
                        return false;

                    });

            _check.Add(Arg.InhouseIsOpen,
                async (CommandContext context, Profile _profile) =>
                {
                    if (TaskScheduler._inhouseOpen)
                    {
                        return true;
                    }

                    await _profile.SendDm("The inhouse isn't open right now. Inhouse opens at 10pmEST and closes at 1amEST.");
                    return false;

                });
            /*
            HasAtLeastAdminRole = CorrectRole("Administration");

            HasAtLeastModRole = CorrectRole("Mod");

            HasAtLeastTrustedRole = CorrectRole("Trusted");

            HasAtLeastMemberRole = CorrectRole("Member");

            HasCasterRole = CorrectRole("Caster");
            */
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

        public async Task<bool> AreMet(CommandContext context, Profile _profile, List<Arg> tasks)
        {
            bool pass = true;
            foreach (Arg task in tasks)
            {

                var conf = await _check[task](context, _profile);
                if (conf == false)
                {
                    return false;
                }
            }


            //deprecated, but may need in future 5/20/21 
            if (!pass)
            {
                return false;
            }

            return true;

        }

        public static bool _locked = false;

        public async Task TryConditionedAction(CommandContext context, Profile _profile, List<Arg> tasks, Func<Task> action)
        {
            var conditions = await AreMet(context, _profile, tasks);

            if (!conditions)
            {
                await context.Message.DeleteAsync();
                return;
            }

            if(_locked)
            {
                await _profile.SendDm("The bot is currently in the middle of a large action. Try running this command again in a few seconds.");
                await context.Message.DeleteAsync();
                return;
            }

            try
            {
                await AssureSchedulingIsRunning(context);
                await action();
                await context.Message.DeleteAsync();
            }
            catch(DSharpPlus.Exceptions.NotFoundException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                await _profile.SendDm("The command you entered may have encountered an error(probably from a large amount of commands coming from different users). Try entering it again. If it still doesn't work, create a ticket and detail the problem. Thank you.");
                await _profile.ReportError(context, e);
            }


        }

        private async Task AssureSchedulingIsRunning(CommandContext context)
        {
            if (!TaskScheduler.active)
            {
                TaskScheduler.Instance.ScheduleTask(17, 0, 24, async () =>
                {
                    if (!TaskScheduler._inhouseOpen)
                        await QOL.SendMessage(context, Bot.Channels.GeneralChannel, "Inhouse queue opens in 2 hours.");
                });

                TaskScheduler.Instance.ScheduleTask(18, 0, 24, async () =>
                {
                    if (!TaskScheduler._inhouseOpen)
                        await QOL.SendMessage(context, Bot.Channels.GeneralChannel, "Inhouse queue opens in 1 hour.");
                });
                TaskScheduler.Instance.ScheduleTask(19, 0, 24, async () =>
                {
                    if (!TaskScheduler._inhouseOpen)
                        await QOL.SendMessage(context, Bot.Channels.GeneralChannel, "<@&852359190453288981>, Queueing is now available tonight from 10pmEST to 1amEST. \n\nReminder to use !pingme to be given the <@&852359190453288981> role. If you change your mind use !dontpingme.");

                    TaskScheduler._inhouseOpen = true;

                });

                TaskScheduler.Instance.ScheduleTask(23, 0, 24, async () =>
                {
                    TaskScheduler._inhouseOpen = false;
                    await QOL.SendMessage(context, Bot.Channels.GeneralChannel, "Queueing is now closed for the night. GrinHouse will open again at 10pmEST tomorrow.");
                });
            }
            await Task.CompletedTask;
        }

    }
}
