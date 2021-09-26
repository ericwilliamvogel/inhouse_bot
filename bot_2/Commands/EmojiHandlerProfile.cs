using db;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace bot_2.Commands
{
    public class EmojiHandlerProfile
    {

        public EmojiHandlerProfile()
        {
            SetEmojiLib();
        }


        Dictionary<int, string> emojiLib = new Dictionary<int, string>();
        private void SetEmojiLib()
        {
            emojiLib.Add(1, "(• ε •)");
            emojiLib.Add(2, "( ͡° ͜ʖ ͡°)");
            emojiLib.Add(3, "[̲̅$̲̅(̲̅ ͡° ͜ʖ ͡°̲̅)̲̅$̲̅]");
            emojiLib.Add(4, "(◕‿◕✿)");
            emojiLib.Add(5, "ᕙ(⇀‸↼‶)ᕗ");
            emojiLib.Add(6, "⚆ _ ⚆");
            emojiLib.Add(7, "༼ つ ◕_◕ ༽つ");
            emojiLib.Add(8, "ಠ_ಠ");
            emojiLib.Add(9, "(ง'̀-'́)ง");
            emojiLib.Add(10, "¯\u005c_(ツ)_/¯");
            emojiLib.Add(11, "Hey <@" + Bot._admins.Admin + ">, Bot is kill. Go fix it you monkey.");
        }

        public async Task Profile(MessageReactionAddEventArgs context, Context _context)
        {
            Profile _profile = new Profile(context.Guild, context.User);

            var playerrecord = await _context.player_data.FindAsync(_profile._id);
            var member = (DiscordMember)context.User;
            string error = "";


            //make this an embed



            var progress = _profile._member.Roles.FirstOrDefault(p => p.Name == "0" || p.Name == "1" || p.Name == "2" || p.Name == "3" || p.Name == "4" || p.Name == "5");

            if (progress == null)
            {
                error += "\n\n:no_entry: You are missing progress for your rank!";
            }

            var rank = _profile._member.Roles.FirstOrDefault(p => p.Name == "Herald" || p.Name == "Guardian" || p.Name == "Crusader" || p.Name == "Archon" || p.Name == "Legend" || p.Name == "Ancient" || p.Name == "Divine" || p.Name == "Immortal");

            if (rank == null)
            {
                error += "\n\n:no_entry: You are missing your badge!";
            }

            var region = _profile._member.Roles.FirstOrDefault(p => p.Name.Contains("US "));

            if (region == null)
            {
                error += "\n\n:no_entry: You are missing your preferred region!";
            }


            var pos = _profile._member.Roles.FirstOrDefault(p => p.Name == Bot._positions.Pos1 || p.Name == Bot._positions.Pos2 || p.Name == Bot._positions.Pos3 || p.Name == Bot._positions.Pos4 || p.Name == Bot._positions.Pos5);

            if (pos == null)
            {
                error += "\n\n:no_entry: You are missing positions that you can play!";
            }


            var prefpos = _profile._member.Roles.FirstOrDefault(p => p.Name == Bot._positions.Pos1 + "(Favorite)" || p.Name == Bot._positions.Pos2 + "(Favorite)" || p.Name == Bot._positions.Pos3 + "(Favorite)" || p.Name == Bot._positions.Pos4 + "(Favorite)" || p.Name == Bot._positions.Pos5 + "(Favorite)");

            if (prefpos == null)
            {
                error += "\n\n:no_entry: You are missing your preferred position!";
            }


            string finalError = "";
            if (error == "")
            {
                finalError = ":white_check_mark: Everything up to date.";
            }
            else
            {
                finalError = error;
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
               .WithTitle("Action Required")
               .AddField("Actions:", finalError, false)
               .WithColor(DiscordColor.Gold);


            builder.Footer = new EmbedFooter() { Text = "" };

            DiscordEmbed embed = builder.Build();


            await _profile.SendDm("", embed);

            var roleslist = member.Roles.ToList();
            var roles = roleslist.FindAll(p => p.Name == Bot._positions.Pos1 || p.Name == Bot._positions.Pos2 || p.Name == Bot._positions.Pos3 || p.Name == Bot._positions.Pos4 || p.Name == Bot._positions.Pos5);

            string rolesdesc = "\nRoles = ";
            foreach (var role in roles)
            {
                rolesdesc += role.Name + " | ";
            }

            var bestrole = member.Roles.FirstOrDefault(p => p.Name.Contains("(Favorite)"));

            string bestdesc = "\nBest role = ";
            if (bestrole != null)
            {
                bestdesc += bestrole.Name;
            }
            else
            {
                bestdesc += "";
            }

            string profile = "---Player Profile---\n";

            if(Bot._game == GameType.Dota2)
            {
                profile += "\nDotaFriendID = " + playerrecord._steamid;
            }

            profile += rolesdesc;
            profile += bestdesc;
            profile += "\nIHL mmr: " + playerrecord._ihlmmr;
            profile += "\nIG mmr: " + playerrecord._dotammr;
            profile += "\nW/L: " + playerrecord._gameswon + "/" + playerrecord._gameslost;
            profile += "\nTotal games: " + playerrecord._totalgames;


            profile += "\nCoins: " + playerrecord._xp;

            var largeEmojiList = await _context.emote_unlocked.ToListAsync();
            var emojiList = largeEmojiList.FindAll(p => p._playerid == _profile._id);
            profile += "\n\n---Emotes owned---\n";
            foreach (KeyValuePair<int, string> pair in emojiLib)
            {
                string ifOwned = "Not owned";
                if (emojiList.FirstOrDefault(p => p._emoteid == pair.Key) != null)
                {
                    ifOwned = "Owned";
                }
                profile += pair.Value + " (" + pair.Key + ") : " + ifOwned + "\n";
            }

            builder = new DiscordEmbedBuilder()
               .WithTitle("Profile")
               .AddField("Stats:", profile, false)
               .WithColor(DiscordColor.Gold);


            builder.Footer = new EmbedFooter() { Text = "" };

            embed = builder.Build();


            await _profile.SendDm("", embed);
        }
    }
}
