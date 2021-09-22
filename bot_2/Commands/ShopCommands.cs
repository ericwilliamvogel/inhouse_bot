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
    public class ShopCommands : BaseCommands
    {
        public ShopCommands(Context context) : base(context)
        {

        }


        [Command("buy")]
        public async Task GenMessage(CommandContext context, int number)
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
                if(number < 1 || number > 11)
                {
                    await _profile.SendDm("Use a number between 1 and 11");
                    return;
                }

                int deduction = 0;
                if(number ==  11)
                {
                    deduction = 1500;
                }
                else
                {
                    deduction = 350;
                }

                if(player._xp < deduction)
                {
                    await _profile.SendDm("You don't have enough coins to complete this transaction. \nYour coins = " + player._xp + "\nPrice = " + deduction);
                    return;
                }


                var record = await _context.emote_unlocked.FirstOrDefaultAsync(p => p._playerid == _profile._id && p._emoteid == number);
                if (record == null)
                {

                    await _profile.SendDm("Emote powers granted.");
                    await _context.emote_unlocked.AddAsync(new EmoteUnlockedData { _playerid = _profile._id, _emoteid = number });
                    //await _context.SaveChangesAsync();

                    player._xp -= deduction;
                    //await _context.SaveChangesAsync();
                }
                else
                {
                    await _profile.SendDm("You already have that emote.");
                }



            });

        }
    }
}
