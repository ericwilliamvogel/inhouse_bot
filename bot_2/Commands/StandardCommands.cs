using db;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using OpenDotaDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bot_2.Commands
{


    public class StandardCommands : BaseCommands
    {

        private Dictionary<string, int> _medalDictionary;


        public StandardCommands(Context context) : base(context)
        {
            _medalDictionary = new Dictionary<string, int>();
            SetRankDictionary();

        }
        private void SetRankDictionary()
        {
            _medalDictionary.Add("herald", 0);
            _medalDictionary.Add("guardian", 1);
            _medalDictionary.Add("crusader", 2);
            _medalDictionary.Add("archon", 3);
            _medalDictionary.Add("legend", 4);
            _medalDictionary.Add("ancient", 5);
            _medalDictionary.Add("divine", 6);
            _medalDictionary.Add("immortal", 7);
        }



        


        [Command("generatemessage")]
        public async Task GenMessage(CommandContext context)
        {
            Profile _profile = new Profile(context);

            var verified = await _conditions.AreMet(context, _profile,
                new List<Arg> {

                });

            if (!verified)
            {
                await context.Message.DeleteAsync();
                return;
            }
            await context.Channel.SendMessageAsync("manuallygeneratedmessage");
            await context.Message.DeleteAsync();
        
        }

        /*[Command("deletemessage")]
        public async Task RefreshQueue(CommandContext context, ulong id)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

                new List<Argument> {
                            _conditions.IsRegistered,
                            _conditions.IsInCommandChannel
                },

                async () =>
                {

                    var msg = await context.Channel.GetMessageAsync(id);
                    await msg.DeleteAsync();
                });
        }
        */










        [Command("mmr")]
        public async Task ShowMMR(CommandContext context)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                            Arg.IsRegistered,
                            Arg.IsInCommandChannel
            },

            async () =>
            {

                var player = await _context.player_data.FindAsync(_profile._id);

                await _profile.SendDm("Your GHL mmr is " + player._ihlmmr);
                await _profile.SendDm("Your Dota mmr is " + player._dotammr);
                await _profile.SendDm("You've won " + player._gameswon + " games.");
                await _profile.SendDm("You've lost " + player._gameslost + " games.");

            });

        }

        [Command("updateregion")]
        public async Task UpdateRegion(CommandContext context, string input)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                            Arg.IsRegistered,
                            Arg.IsInCommandChannel
            },

            async () =>
            {

                var player = await _context.player_data.FindAsync(_profile._id);
                string msg = input.ToLower();
                if(msg == "east" || msg == "useast" || msg == "use")
                {
                    player._region = (int)Region.USEAST;
                    await _context.SaveChangesAsync();
                    await _profile.SendDm("Updated your region to " + (Region)player._region);
                }
                else if (msg == "west" || msg == "uswest" || msg == "usw")
                {
                    player._region = (int)Region.USWEST;
                    await _context.SaveChangesAsync();
                    await _profile.SendDm("Updated your region to " + (Region)player._region);
                }
                else
                {
                    await _profile.SendDm("Input " + input + " was not recognized as valid. Just type west or east :)");
                }

            });

        }

        [Command("updatepositions")]
        public async Task UpdateRoleOne(CommandContext context, int input, int input2)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel
            },

            async () =>
            {
                var player = await _context.player_data.FindAsync(_profile._id);
                if (input <= 0 || input >= 6 || input2 <= 0 || input2 >= 6)
                {
                    await _profile.SendDm("Input " + input + " was not recognized as valid. Use a number between 1 and 5.");
                }
                else
                {
                    player._role1 = input;
                    player._role2 = input2;
                    await _context.SaveChangesAsync();
                    await _profile.SendDm("Updated your primary role to " + player._role1 + ". Your second role has been set as " + player._role2 + ".");
                }


            });

        }

        [Command("updateposition")]
        public async Task URO(CommandContext context, int input, int input2)
        {
            await UpdateRoleOne(context, input, input2);
        }

        [Command("preferences")]
        public async Task Preferences(CommandContext context)
        {
            Profile _profile = new Profile(context);

            await _conditions.TryConditionedAction(context, _profile,

            new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel
            },

            async () =>
            {
                var player = await _context.player_data.FindAsync(_profile._id);

                string error = "";
                string info = "Your profile:";
                info += "\nDotaFriendID = " + player._steamid;
                info += "\nRegion = " + (Region)player._region;
                info += "\nRole1 = " + player._role1;
                info += "\nRole2 = " + player._role2;

                if (player._steamid == 0)
                {
                    error += "\n\nYou can update your friendid by using !updateid your_steam_id in the general #commands channel. Example: !updateid 199304122";
                }
                if (player._dotammr == 0)
                {
                    error += "\n\nYou can update your mmr by using !updateran in the general #commands channel. Example: !updaterank ancient 5";
                }
                if (player._region == (int)Region.NONE)
                {
                    error += "\n\nYou can update your region by using !updateregion your_region in the general #commands channel. Examples: !updateregion useast, !updateregion east, !updateregion uswest, !updateregion west";
                }
                if (player._role1 <= 0 || player._role1 > 5)
                {
                    error += "\n\nYou can update your positions by using !updatepositions most_comfortable second_most_comfortable in the general #commands channel. 2 positions must be entered. Examples: !updatepositions 5 4, !updatepositions 1 3, etc. ";
                }
                await _profile.SendDm(info + error);
                if (error == "")
                {
                    await _profile.SendDm("\n\nYour profile is up to date, nothing is missing.");
                }
    



            });

        }




        

        [Command("register")]
        public async Task RegisterPlayer(CommandContext context, long steamid)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsntRegistered,
                    Arg.IsInCommandChannel
                },

                async () =>
                {

                    var openDota = new OpenDotaApi();
                    var playerDetails = await openDota.Players.GetPlayerByIdAsync(steamid);

                    if (playerDetails == null)
                    {
                        await _profile.SendDm("Your dota friend id did not return a record. " +
                            "Please refer back to our welcome page for instructions on how to get your dota friend id and try again." +
                            " If you're sure everything is correct feel free to contact a mod / admin to get you set up.");
                    }
                    else
                    {
                        await _context.player_data.AddAsync(new PlayerData { _id = _profile._id, _steamid = steamid, _status = 1, _ihlmmr = 400, _dotammr = 0, _region = 0, _role1 = 0, _role2 = 0 });

                        await _context.SaveChangesAsync();
                        await _profile.SendDm("You are now registered. Type !queue to enter the queue and !leave to leave the queue. Your GHL mmr starts at 400 and has a base increment of 15. Your initial mmr was not recongized, use the !updaterank command to update your mmr. Example: !updaterank ancient 5");
                        /*if (playerDetails.MmrEstimate.Estimate == null)
                        {

                           
                            .ConfigureAwait(false);

                        }
                        else
                        {
                            await _context.player_data.AddAsync(new PlayerData { _id = _profile._id, _steamid = steamid, _status = 1, _ihlmmr = 400, _dotammr = (int)playerDetails.MmrEstimate.Estimate, _region = 0, _role1 = 0, _role2 = 0 }).ConfigureAwait(false);
                            await _profile.SendDm("You are now registered. Type !queue to enter the queue and !leave to leave the queue. Your GHL mmr starts at 400 and has a base increment of 15. Your initial mmr was recognized as " + (int)playerDetails.MmrEstimate.Estimate + ". If this is off by 300mmr or above, use the !updaterank command to update your mmr. Example: !updaterank ancient 5").ConfigureAwait(false);


                            await _context.SaveChangesAsync();
                        }*/

                       
                    }


                });

        }



        [Command("updaterank")]
        public async Task UpdateRank(CommandContext context, string medal, int rank)
        {

            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel
                },

                async () =>
                {

                    var updatedmedal = medal.ToLower();
                    if (_medalDictionary.ContainsKey(updatedmedal))
                    {
                        var start = _medalDictionary[updatedmedal];
                        var pos = start * 6 * 130;
                        var adj = rank * 130;
                        var final = pos + adj;

                        var record = await _context.player_data.FindAsync(_profile._id);
                        record._dotammr = final;
                        await _context.SaveChangesAsync();

                        await _profile.SendDm("Updated mmr to " + final + ".");
                    }
                    else
                    {

                        await _profile.SendDm("The rank you entered wasn't recognized. It was received as " + medal + " " + rank + " Please enter a valid medal ie. Ancient, Legend, Archon, then a valid number between 1-5. Example: !updaterank ancient 3");
                    }
                });
        }

        [Command("updateid")]
        public async Task UpdatePlayer(CommandContext context, long steamid)
        {
            Profile _profile = new Profile(context);
            await _conditions.TryConditionedAction(context, _profile,

                new List<Arg> {
                    Arg.IsRegistered,
                    Arg.IsInCommandChannel
                },

                async () =>
                {

                    var openDota = new OpenDotaApi();
                    var playerDetails = await openDota.Players.GetPlayerByIdAsync(steamid);


                    if (playerDetails == null)
                    {
                        await _profile.SendDm("Your dota friend id did not return a record. " +
                            "Please refer back to our welcome page for instructions on how to get your dota friend id and try again." +
                            " If you're sure everything is correct feel free to contact a mod / admin to get you set up.");
                    }
                    else
                    {
                        var record = await _context.player_data.FindAsync(_profile._id);
                        record._steamid = steamid;
                        if(playerDetails.MmrEstimate.Estimate!=null)
                        {
                            record._dotammr = (int)playerDetails.MmrEstimate.Estimate;
                        }
                        else
                        {
                            record._dotammr = 0;
                        }
                        await _context.SaveChangesAsync();

                        await _profile.SendDm("Your player id was updated.");
                    }

                });
        }





    }
}
