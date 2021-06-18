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
using OpenDotaDotNet;

namespace bot_2.Commands
{
    public class Permissions
    {
        public Context _context;

        public DiscordRole AverageRole { get; set; }
        public DiscordRole TrustedRole { get; set; }

        public int LobbyNumber { get; set; }

        public DiscordRole LobbyRoleTeam1 { get; set; }

        public DiscordRole LobbyRoleTeam2 { get; set; }

        public DiscordRole LobbyRoleSpectator { get; set; }

        public DiscordRole LobbyRoleCaster { get; set; }

        public DiscordChannel parent { get; set; }

        public DiscordChannel generaltext { get; set; }

        public DiscordChannel team1voice { get; set; }

        public DiscordChannel team2voice { get; set; }

        public DiscordChannel castervoice { get; set; }

        public DiscordChannel spectatorvoice { get; set; }

        public DiscordMessage message { get; set; }
        public Permissions(Context context)
        {
            this._context = context;


        }

        public async Task Get(CommandContext context, LobbyUtilities _utilities, int gameid)
        {
            AverageRole = _utilities.GetRole(context, "Member");
            TrustedRole = _utilities.GetRole(context, "Trusted");

            //HERE
            var record =  _context.discord_channel_info.First(p => p._gameid == gameid);

            LobbyNumber = record._number;

            var lobby = _utilities.GetLobby(context, LobbyNumber);
            var roles = _utilities.GetRoles(context, LobbyNumber);

            this.parent = lobby;

            team1voice = lobby.Children.First(p => p.Name == "Team1");
            team2voice = lobby.Children.First(p => p.Name == "Team2");
            generaltext = lobby.Children.First(p => p.Name == "general");
            spectatorvoice = lobby.Children.First(p => p.Name == "Spectator");
            castervoice = lobby.Children.First(p => p.Name == "Caster");

            LobbyRoleTeam1 = roles.First(p => p.Name == "team1Lobby" + LobbyNumber);
            LobbyRoleTeam2 = roles.First(p => p.Name == "team2Lobby" + LobbyNumber);
            LobbyRoleCaster = roles.First(p => p.Name == "casterLobby" + LobbyNumber);
            LobbyRoleSpectator = roles.First(p => p.Name == "spectatorLobby" + LobbyNumber);

            message = await generaltext.GetMessageAsync(record._messageid);
            //var msg = await generaltext.GetMessagesBeforeAsync(0, 1);
            //message = msg.First();

        }
        public async Task Create(CommandContext context, LobbyUtilities _utilities)
        {

            AverageRole = _utilities.GetRole(context, "Member");
            TrustedRole = _utilities.GetRole(context, "Trusted");
            LobbyNumber = await _utilities.RecursiveGetLobbyNumber(context);


            LobbyRoleTeam1 = await _utilities.RecursiveCreateRole(context, "team1");
            LobbyRoleTeam2 = await _utilities.RecursiveCreateRole(context, "team2");
            LobbyRoleSpectator = await _utilities.RecursiveCreateRole(context, "spectator");
            LobbyRoleCaster = await _utilities.RecursiveCreateRole(context, "caster");
            parent = await context.Guild.CreateChannelCategoryAsync("Lobby" + LobbyNumber);

            generaltext = await context.Guild.CreateChannelAsync("general", DSharpPlus.ChannelType.Text, parent);
            team1voice = await context.Guild.CreateChannelAsync("Team1", DSharpPlus.ChannelType.Voice, parent);
            team2voice = await context.Guild.CreateChannelAsync("Team2", DSharpPlus.ChannelType.Voice, parent);

            spectatorvoice = await context.Guild.CreateChannelAsync("Spectator", DSharpPlus.ChannelType.Voice, parent);
            castervoice = await context.Guild.CreateChannelAsync("Caster", DSharpPlus.ChannelType.Voice, parent);

            LobbyInfo _info = new LobbyInfo(_context, _utilities);
            await generaltext.SendMessageAsync(_info.GetLobbyPass("grinlobby" + LobbyNumber));
            message = await generaltext.SendMessageAsync("Starting...");

            Console.WriteLine("Perms...");

            await RevokeAllPermissions(team2voice, LobbyRoleSpectator);
            await RevokeAllPermissions(team1voice, LobbyRoleSpectator);

            await generaltext.AddOverwriteAsync(LobbyRoleTeam1, DSharpPlus.Permissions.ReadMessageHistory);
            await generaltext.AddOverwriteAsync(LobbyRoleTeam2, DSharpPlus.Permissions.ReadMessageHistory);
            await generaltext.AddOverwriteAsync(LobbyRoleSpectator, DSharpPlus.Permissions.ReadMessageHistory);
            await generaltext.AddOverwriteAsync(LobbyRoleCaster, DSharpPlus.Permissions.ReadMessageHistory);

            await generaltext.AddOverwriteAsync(LobbyRoleTeam1, DSharpPlus.Permissions.SendMessages);
            await generaltext.AddOverwriteAsync(LobbyRoleTeam2, DSharpPlus.Permissions.SendMessages);
            await generaltext.AddOverwriteAsync(LobbyRoleSpectator, DSharpPlus.Permissions.SendMessages);
            await generaltext.AddOverwriteAsync(LobbyRoleCaster, DSharpPlus.Permissions.SendMessages);

            await generaltext.AddOverwriteAsync(LobbyRoleTeam1, DSharpPlus.Permissions.AccessChannels);
            await generaltext.AddOverwriteAsync(LobbyRoleTeam2, DSharpPlus.Permissions.AccessChannels);
            await generaltext.AddOverwriteAsync(LobbyRoleSpectator, DSharpPlus.Permissions.AccessChannels);
            await generaltext.AddOverwriteAsync(LobbyRoleCaster, DSharpPlus.Permissions.AccessChannels);


            await generaltext.AddOverwriteAsync(TrustedRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
            await generaltext.AddOverwriteAsync(AverageRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
            await team1voice.AddOverwriteAsync(TrustedRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
            await team1voice.AddOverwriteAsync(AverageRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
            await team2voice.AddOverwriteAsync(TrustedRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
            await team2voice.AddOverwriteAsync(AverageRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);

            await RevokeAllPermissions(generaltext, AverageRole);
            await RevokeAllPermissions(generaltext, TrustedRole);

            await RevokeAllPermissions(team1voice, AverageRole);
            await RevokeAllPermissions(team1voice, TrustedRole);

            await team1voice.AddOverwriteAsync(LobbyRoleTeam1, DSharpPlus.Permissions.AccessChannels);

            await RevokeAllPermissions(team2voice, AverageRole);
            await RevokeAllPermissions(team2voice, TrustedRole);

            await team2voice.AddOverwriteAsync(LobbyRoleTeam2, DSharpPlus.Permissions.AccessChannels);

            await RevokeAllPermissions(spectatorvoice, AverageRole);
            await RevokeAllPermissions(spectatorvoice, TrustedRole);

            await spectatorvoice.AddOverwriteAsync(LobbyRoleSpectator, DSharpPlus.Permissions.AccessChannels);

            await RevokeAllPermissions(castervoice, AverageRole);
            await RevokeAllPermissions(castervoice, TrustedRole);

            await castervoice.AddOverwriteAsync(LobbyRoleCaster, DSharpPlus.Permissions.AccessChannels);


            await RevokeAllPermissions(team2voice, LobbyRoleTeam1);
            await RevokeAllPermissions(team1voice, LobbyRoleTeam2);

            await RevokeAllPermissions(team2voice, LobbyRoleCaster);
            await RevokeAllPermissions(team1voice, LobbyRoleCaster);

            await RevokeAllPermissions(castervoice, LobbyRoleTeam1);
            await RevokeAllPermissions(spectatorvoice, LobbyRoleTeam1);

            await RevokeAllPermissions(castervoice, LobbyRoleTeam2);
            await RevokeAllPermissions(spectatorvoice, LobbyRoleTeam2);
        }

        public async Task RevokeAllPermissions(DiscordChannel channel, DiscordRole role)
        {
            await channel.AddOverwriteAsync(role, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
        }


    }
}
