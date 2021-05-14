using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using db;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace bot_2.Commands
{
    public class Player
    {
        public ulong _id;
        public int _status { get; set; }
        //ihlmmr starts at 400? increments by 20.
        public Player(ulong _id, int _mmr, int _ihlmmr, int _adjmmr, int _status)
        {
            this._id = _id;
            this._mmr = _mmr;
            this._ihlmmr = _ihlmmr;
            this._adjmmr = _adjmmr;
            this._status = _status;
        }
        public int _truemmr
        {
            get
            {
                return _mmr + _ihlmmr * 3;
            }
        }
        public int _mmr { private get; set; }
        public int _ihlmmr { private get; set; }
        public int _adjmmr { private get; set; }

    }

    public class Profile
    {
        public ulong _id { get; set; }
        public DiscordMember _member { get; set; }
        public Profile(CommandContext context)
        {
            ulong id = context.Member.Id;
            _id = id;

            _member = context.Guild.Members[id];


        }


        public Profile(CommandContext context, ulong id)
        {
            _id = id;

            if(context.Guild.Members.ContainsKey(id))
            _member = context.Guild.Members[id];


        }

        public async Task<bool> Exists(Context _context)
        {

            var preliminaryCheck = await _context.player_data.FindAsync(_id);
            if (preliminaryCheck == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task SendDm(string input)
        {
            try
            {
                if(_member != null)
                {
                    var channel = await _member.CreateDmChannelAsync();

                    await channel.SendMessageAsync(input).ConfigureAwait(false);
                }

            }
            catch
            {


            }
        }
        public async Task ReportError(Exception input)
        {
            await SendDm("Some weird error occurred. Send a screenshot of this to #bugs or an admin/mod. Thank you. \n Details = \n" + input.ToString());
        }
    }
}
