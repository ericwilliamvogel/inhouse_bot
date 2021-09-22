using bot_2.Json;
using db;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_2.Commands
{
    public class ContextualAction
    {
        Context _context;
        public ContextualAction(CommandContext context, Func<Task> action, Context dbContext)
        {
            Profile profile = new Profile(context);
            _context = dbContext;
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
        public async Task AddAction(Func<Task> action)
        {
            bool start = false;
            if(_actions.Count == 0)
            {
                start = true;
            }
            _actions.Add(async () =>
            {
                await action();
                await CompleteAction();
            });

            if(start)
            {
                await CompleteAction();
            }
            
        }
        public async Task AddDelayedAction(int ms, Func<Task> action)
        {
            await Task.Delay(ms);

            _actions.Add(async () =>
            {
                await action();
                await CompleteAction();
            });


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
        InhouseIsOpen,
        IsInRegistrationChannel
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
                        await _profile.SendIncompleteDm("Our records show that you are already in a game/already in a queue. Some restrictions apply if you are already occupied, an example being you cannot queue.");
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
                            await _profile.SendIncompleteDm("You need to be a lobby host, and our records show that you aren't the host of any lobby.");
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
                        await _profile.SendIncompleteDm("You aren't registered. Register by going to the #commands channel and typing !register your_steam_id_here to get signed up. We need you to register in order to track your GHL mmr after every game.");
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
                        await _profile.SendIncompleteDm("You're already registered.");
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
                            await _profile.SendIncompleteDm("You cannot issue this command in this channel. This command can only be executed in channel '#commands'.");
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
                        await _profile.SendIncompleteDm("You cannot execute this command because you're in queue.");
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
                        await _profile.SendIncompleteDm("You're not in any queue.");
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
                        await _profile.SendIncompleteDm("You're already in queue.");
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
                        await _profile.SendIncompleteDm("You're not in any queue.");
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
                        await _profile.SendIncompleteDm("You're already in queue.");
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
                        await _profile.SendIncompleteDm("You're not in any queue.");
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
                        await _profile.SendIncompleteDm("You were not found as a leader in any database records.");
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
                        await _profile.SendIncompleteDm("You do not have emote permissions.");
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
                        await _profile.SendIncompleteDm("You were not found as a leader in any database records.");
                        return false;
                    }

                    if(record._canpick == 1)
                    {
                        return true;
                    }
                    else if(record._canpick == 0)
                    {
                        await _profile.SendIncompleteDm("It's not your turn to pick.");
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

                    await _profile.SendIncompleteDm("There were no mentions recognized in your command.");
                    return false;

                });

            _check.Add(Arg.ProfileComplete,
                    async (CommandContext context, Profile _profile) =>
                    {  
                        var progress = _profile._member.Roles.FirstOrDefault(p => p.Name == "0" || p.Name == "1" || p.Name == "2" || p.Name == "3" || p.Name == "4" || p.Name == "5");

                        var rank = _profile._member.Roles.FirstOrDefault(p => p.Name == "Herald" || p.Name == "Guardian" || p.Name == "Crusader" || p.Name == "Archon" || p.Name == "Legend" || p.Name == "Ancient" || p.Name == "Divine" || p.Name == "Immortal");

                        var region = _profile._member.Roles.FirstOrDefault(p => p.Name.Contains("US "));

                        var pos = _profile._member.Roles.FirstOrDefault(p => p.Name == Bot._positions.Pos1 || p.Name == Bot._positions.Pos2 ||  p.Name == Bot._positions.Pos3 || p.Name == Bot._positions.Pos4 || p.Name == Bot._positions.Pos5 );

                        var prefpos = _profile._member.Roles.FirstOrDefault(p => p.Name == Bot._positions.Pos1 + "(Favorite)" || p.Name == Bot._positions.Pos2 + "(Favorite)" || p.Name == Bot._positions.Pos3 + "(Favorite)" || p.Name == Bot._positions.Pos4 + "(Favorite)" || p.Name == Bot._positions.Pos5 + "(Favorite)" );

                        if(progress == null || rank == null || region == null || pos == null || prefpos == null)
                        {
                            await _profile.SendIncompleteDm("Your profile is incomplete, please type !profile for information on how to complete your profile.");
                            return false;
                        }

                        return true;

                    });

            _check.Add(Arg.InhouseIsOpen,
                async (CommandContext context, Profile _profile) =>
                {
                    if (TaskScheduler._inhouseOpen)
                    {
                        return true;
                    }

                    await _profile.SendIncompleteDm("The inhouse isn't open right now. See scheduling details under #scheduling.");
                    return false;

                });

            _check.Add(Arg.IsInRegistrationChannel,
                async (CommandContext context, Profile _profile) =>
                {
                    if(context.Channel != await Bot._validator.Get(context, "registration"))
                    {
                        await _profile.SendIncompleteDm("You can only register in the registration channel.");
                        return false;
                    }

                    return true;

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
                    await _profile.SendIncompleteDm("You cannot issue this command in this channel. This command can only be executed in channel '" + channelName + "'.");
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
                    await _profile.SendIncompleteDm("Role check for '" + roleName + "' was not found. The check for the role is case sensitive, this may be on the dev end to fix! Please open a ticket to report the issue.");
                    return false;
                }


                if(_profile._member.Roles.Contains(role))
                {
                    return true;
                }
                else
                {
                    await _profile.SendIncompleteDm("You don't have the minimum role necessary to execute this command.");
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

        public async Task TryConditionedAction(CommandContext context, Profile _profile, List<Arg> tasks, DbDependentAction action)
        {
            var conditions = await AreMet(context, _profile, tasks);

            if (!conditions)
            {
                await context.Message.DeleteAsync();
                return;
            }

            if(_locked)
            {
                await _profile.SendIncompleteDm("The bot is currently busy doing routine database update / running a large action. Try running this command again in a few seconds.");
                await context.Message.DeleteAsync();
                return;
            }

            try
            {
                await AssureSchedulingIsRunning(context);


                    await action();


                //await action();
                await context.Message.DeleteAsync();
            }
            catch(DSharpPlus.Exceptions.NotFoundException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                await _profile.SendIncompleteDm("The command you entered may have encountered an error(probably from a large amount of commands coming from different users). Try entering it again. If it still doesn't work, create a ticket and detail the problem. Thank you.");
                await _profile.ReportError(context, e);
            }


        }


        private async Task StartSaveChangesLoop(CommandContext context)
        {
            var channel = await Bot._validator.Get(context, "error-logs");// context.Guild.Channels[Bot.Channels.ErrorChannel];

            Task task = await Task.Factory.StartNew(async () =>
            {
                await SaveChangesEvery(channel, 5000);
            }, TaskCreationOptions.LongRunning);

        }

        public async Task SaveChanges()
        {
            _locked = true;
            await Task.Delay(300);

            await _context.SaveChangesAsync();

            await Task.Delay(50);
            _locked = false;
        }
        private async Task SaveChangesEvery(DiscordChannel channel, int ms)
        {
            await Task.Delay(ms);
            if(!_locked)
            {

                await SaveChanges();
            }
            else
            {
                await channel.SendMessageAsync("Changes skipped");
            }

            await SaveChangesEvery(channel, ms);
        }
        private async Task AssureSchedulingIsRunning(CommandContext context)
        {
            if (!TaskScheduler.active)
            {

                await StartSaveChangesLoop(context);

                JsonCommunicator comm = new JsonCommunicator();
                string serialized = comm.GetStringValue("schedule", "schedule");

                ScheduleJson schedule = JsonConvert.DeserializeObject<ScheduleJson>(serialized);
                foreach (var day in schedule.Schedule)
                {
                    await ScheduleTasks(day, context);
                }


                TaskScheduler.Instance.ScheduleTask(5, 0, 24, async () =>
                {
                    try
                    {
                        foreach (var day in schedule.Schedule)
                        {
                            await ScheduleTasks(day, context);
                        }

                    }
                    catch (Exception e)
                    {

                    }

                });
            }
            await Task.CompletedTask;
        }

        private async Task ScheduleTasks(DayScheduleJson day, CommandContext context)
        {
            if (DateTime.Now.DayOfWeek == (System.DayOfWeek)day.Day)
            {
                Console.WriteLine("Schedule set for " + (System.DayOfWeek)day.Day);
                TaskScheduler.Instance.ScheduleTask(day.StartHour - 2, day.StartMinute, 1144, async () =>
                {
                    try
                    {
                        if (!TaskScheduler._inhouseOpen)
                        {
                            await QOL.SendMessage(context, "inhouse-general", "Inhouse queue opens in 2 hours.");
                        }

                    }
                    catch (Exception e)
                    {

                    }

                });

                TaskScheduler.Instance.ScheduleTask(day.StartHour - 1, day.StartMinute, 1144, async () =>
                {
                    try
                    {
                        if (!TaskScheduler._inhouseOpen)
                        {
                            await QOL.SendMessage(context, "inhouse-general", "Inhouse queue opens in 1 hour.");
                        }

                    }
                    catch (Exception e)
                    {

                    }

                });
                TaskScheduler.Instance.ScheduleTask(day.StartHour, day.StartMinute, 1144, async () =>
                {
                    try
                    {
                        if (!TaskScheduler._inhouseOpen)
                        {
                            await QOL.SendMessage(context, "inhouse-general", "<@&775151515442741258>, Queueing is now available. \n\nReminder to go to the control-panel tab to disable notifications if you don't want to be pinged.");
                        }

                    }
                    catch (Exception e)
                    {

                    }


                    TaskScheduler._inhouseOpen = true;

                });

                TaskScheduler.Instance.ScheduleTask(day.EndHour, day.EndMinute, 1144, async () =>
                {
                    TaskScheduler._inhouseOpen = false;
                    try
                    {
                        await QOL.SendMessage(context, "inhouse-general", "Queueing is now closed for the night. Refer to #scheduling for inhouse hours.");
                    }
                    catch (Exception e)
                    {

                    }

                });
            }
            await Task.CompletedTask;
        }
    }
}
