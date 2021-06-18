﻿using System;
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
        public ulong _id { get; set; }
        public int _status { get; set; }

        public DateTimeOffset _start { get; set; }
        //ihlmmr starts at 400? increments by 20.
        public Player(ulong _id, int _mmr, int _ihlmmr)
        {
            this._id = _id;
            this._mmr = _mmr;
            this._ihlmmr = _ihlmmr;
        }

        public Player(ulong _id, DateTimeOffset _start)
        {
            this._start = _start;
            this._id = _id;
        }
        public int _truemmr
        {
            get
            {
                return _mmr + _ihlmmr * 3;
            }
        }
        public int _mmr { get; set; }
        public int _ihlmmr { get; set; }
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
                if (_member != null)
                {
                    var channel = await _member.CreateDmChannelAsync();

                    await channel.SendMessageAsync("```" + input + "```").ConfigureAwait(false);
                }

            }
            catch
            {


            }
        }

        public async Task SendDmNaked(string input)
        {
            try
            {
                if (_member != null)
                {
                    var channel = await _member.CreateDmChannelAsync();

                    await channel.SendMessageAsync(input).ConfigureAwait(false);
                }

            }
            catch
            {


            }
        }
        public async Task ReportError(CommandContext context, Exception input)
        {
            if(context.Guild.Channels.ContainsKey(844949177347604531))
            {
                //error log channel
                var channel = context.Guild.Channels[844949177347604531];
                //tag me
                await channel.SendMessageAsync("<@126922582208282624> : " + input.ToString());
            }
            else
            {
                await SendDm("Some weird error occurred. Send a screenshot of this to #bugs or an admin/mod. Thank you. \n Details = \n" + input.ToString());
            }


        }

        public async Task ReportError(CommandContext context, string input)
        {
            if (context.Guild.Channels.ContainsKey(844949177347604531))
            {
                //error log channel
                var channel = context.Guild.Channels[844949177347604531];
                //tag me
                await channel.SendMessageAsync("<@126922582208282624> : " + input);
            }
            else
            {
                await SendDm("Some weird error occurred. Send a screenshot of this to #bugs or an admin/mod. Thank you. \n Details = \n" + input.ToString());
            }


        }
    }
}
