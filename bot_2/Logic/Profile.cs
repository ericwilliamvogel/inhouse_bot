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
using DSharpPlus.EventArgs;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace bot_2.Commands
{
    public class Player
    {
        public ulong _id { get; set; }

        public DateTimeOffset _start { get; set; }

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

        public Profile(CustomContext context)
        {
            ulong id = context.Member.Id;
            _id = id;

            _member = context.Guild.Members[id];


        }

        public Profile(MessageCreateEventArgs context)
        {
            ulong id = context.Author.Id;
            _id = id;

            _member = context.Guild.Members[id];
        }


        public Profile(CommandContext context, ulong id)
        {
            _id = id;

            if (context.Guild.Members.ContainsKey(id))
                _member = context.Guild.Members[id];


        }

        public Profile(DiscordGuild guild, DiscordUser user)
        {
            _id = user.Id;
            var member = (DiscordMember)user;
            _member = member;


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

                    DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                        .AddField("--Inhouse Message--", input, false)
                        .WithColor(DiscordColor.Gold);


                    builder.Footer = new EmbedFooter() { Text = "" };

                    DiscordEmbed embed = builder.Build();

                    await channel.SendMessageAsync("", false, embed).ConfigureAwait(false);
                }

            }
            catch
            {


            }
        }

        public async Task SendCompleteDm(string input)
        {
            await SendDm(":white_check_mark: " + input);
        }

        public async Task SendIncompleteDm(string input)
        {
            await SendDm(":no_entry: " + input);
        }
        public async Task SendDm(string input, DiscordEmbed embed)
        {
            try
            {
                if (_member != null)
                {
                    var channel = await _member.CreateDmChannelAsync();

                    await channel.SendMessageAsync(input, false, embed).ConfigureAwait(false);
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

        public async Task ReportError(MessageCreateEventArgs context, string input)
        {
            var channel = await Bot._validator.Get(context, "error-logs");
            await channel.SendMessageAsync("<@"  + Bot._admins.Admin + "> : " + input);
        }

        public async Task ReportError(CommandContext context, Exception input)
        {
            var channel = await Bot._validator.Get(context, "error-logs");
            await channel.SendMessageAsync("<@" + Bot._admins.Admin  + "> : " + input);
        }

        public async Task ReportError(CommandContext context, string input)
        {
            var channel = await Bot._validator.Get(context, "error-logs");
            await channel.SendMessageAsync("<@" + Bot._admins.Admin + "> : " + input);
        }

        public async Task ReportError(CustomContext context, string input)
        {
            var channel = await Bot._validator.Get(context.Guild, "error-logs");
            await channel.SendMessageAsync("<@" + Bot._admins.Admin + "> : " + input);
        }
        public async Task ReportError(CustomContext context, Exception input)
        {
            var channel = await Bot._validator.Get(context.Guild, "error-logs");
            await channel.SendMessageAsync("<@" + Bot._admins.Admin + "> : " + input);
        }
    }
}
