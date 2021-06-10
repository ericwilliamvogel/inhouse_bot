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

        public DiscordRole LobbyRoleRadiant { get; set; }

        public DiscordRole LobbyRoleDire { get; set; }

        public DiscordRole LobbyRoleSpectator { get; set; }

        public DiscordRole LobbyRoleCaster { get; set; }

        public DiscordChannel parent { get; set; }

        public DiscordChannel generaltext { get; set; }

        public DiscordChannel radiantvoice { get; set; }

        public DiscordChannel direvoice { get; set; }

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

            radiantvoice = lobby.Children.First(p => p.Name == "Radiant");
            direvoice = lobby.Children.First(p => p.Name == "Dire");
            generaltext = lobby.Children.First(p => p.Name == "general");
            spectatorvoice = lobby.Children.First(p => p.Name == "Spectator");
            castervoice = lobby.Children.First(p => p.Name == "Caster");

            LobbyRoleRadiant = roles.First(p => p.Name == "radiantLobby" + LobbyNumber);
            LobbyRoleDire = roles.First(p => p.Name == "direLobby" + LobbyNumber);
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


            LobbyRoleRadiant = await _utilities.RecursiveCreateRole(context, "radiant");
            LobbyRoleDire = await _utilities.RecursiveCreateRole(context, "dire");
            LobbyRoleSpectator = await _utilities.RecursiveCreateRole(context, "spectator");
            LobbyRoleCaster = await _utilities.RecursiveCreateRole(context, "caster");
            parent = await context.Guild.CreateChannelCategoryAsync("Lobby" + LobbyNumber);

            generaltext = await context.Guild.CreateChannelAsync("general", DSharpPlus.ChannelType.Text, parent);
            radiantvoice = await context.Guild.CreateChannelAsync("Radiant", DSharpPlus.ChannelType.Voice, parent);
            direvoice = await context.Guild.CreateChannelAsync("Dire", DSharpPlus.ChannelType.Voice, parent);

            spectatorvoice = await context.Guild.CreateChannelAsync("Spectator", DSharpPlus.ChannelType.Voice, parent);
            castervoice = await context.Guild.CreateChannelAsync("Caster", DSharpPlus.ChannelType.Voice, parent);

            LobbyInfo _info = new LobbyInfo(_context, _utilities);
            await generaltext.SendMessageAsync(_info.GetLobbyPass("grinlobby" + LobbyNumber));
            message = await generaltext.SendMessageAsync("Starting...");

            Console.WriteLine("Perms...");

            await RevokeAllPermissions(direvoice, LobbyRoleSpectator);
            await RevokeAllPermissions(radiantvoice, LobbyRoleSpectator);

            await generaltext.AddOverwriteAsync(LobbyRoleRadiant, DSharpPlus.Permissions.ReadMessageHistory);
            await generaltext.AddOverwriteAsync(LobbyRoleDire, DSharpPlus.Permissions.ReadMessageHistory);
            await generaltext.AddOverwriteAsync(LobbyRoleSpectator, DSharpPlus.Permissions.ReadMessageHistory);
            await generaltext.AddOverwriteAsync(LobbyRoleCaster, DSharpPlus.Permissions.ReadMessageHistory);

            await generaltext.AddOverwriteAsync(LobbyRoleRadiant, DSharpPlus.Permissions.SendMessages);
            await generaltext.AddOverwriteAsync(LobbyRoleDire, DSharpPlus.Permissions.SendMessages);
            await generaltext.AddOverwriteAsync(LobbyRoleSpectator, DSharpPlus.Permissions.SendMessages);
            await generaltext.AddOverwriteAsync(LobbyRoleCaster, DSharpPlus.Permissions.SendMessages);

            await generaltext.AddOverwriteAsync(LobbyRoleRadiant, DSharpPlus.Permissions.AccessChannels);
            await generaltext.AddOverwriteAsync(LobbyRoleDire, DSharpPlus.Permissions.AccessChannels);
            await generaltext.AddOverwriteAsync(LobbyRoleSpectator, DSharpPlus.Permissions.AccessChannels);
            await generaltext.AddOverwriteAsync(LobbyRoleCaster, DSharpPlus.Permissions.AccessChannels);


            await generaltext.AddOverwriteAsync(TrustedRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
            await generaltext.AddOverwriteAsync(AverageRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
            await radiantvoice.AddOverwriteAsync(TrustedRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
            await radiantvoice.AddOverwriteAsync(AverageRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
            await direvoice.AddOverwriteAsync(TrustedRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
            await direvoice.AddOverwriteAsync(AverageRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);

            await RevokeAllPermissions(generaltext, AverageRole);
            await RevokeAllPermissions(generaltext, TrustedRole);

            await RevokeAllPermissions(radiantvoice, AverageRole);
            await RevokeAllPermissions(radiantvoice, TrustedRole);

            await radiantvoice.AddOverwriteAsync(LobbyRoleRadiant, DSharpPlus.Permissions.AccessChannels);

            await RevokeAllPermissions(direvoice, AverageRole);
            await RevokeAllPermissions(direvoice, TrustedRole);

            await direvoice.AddOverwriteAsync(LobbyRoleDire, DSharpPlus.Permissions.AccessChannels);

            await RevokeAllPermissions(spectatorvoice, AverageRole);
            await RevokeAllPermissions(spectatorvoice, TrustedRole);

            await spectatorvoice.AddOverwriteAsync(LobbyRoleSpectator, DSharpPlus.Permissions.AccessChannels);

            await RevokeAllPermissions(castervoice, AverageRole);
            await RevokeAllPermissions(castervoice, TrustedRole);

            await castervoice.AddOverwriteAsync(LobbyRoleCaster, DSharpPlus.Permissions.AccessChannels);


            await RevokeAllPermissions(direvoice, LobbyRoleRadiant);
            await RevokeAllPermissions(radiantvoice, LobbyRoleDire);

            await RevokeAllPermissions(direvoice, LobbyRoleCaster);
            await RevokeAllPermissions(radiantvoice, LobbyRoleCaster);

            await RevokeAllPermissions(castervoice, LobbyRoleRadiant);
            await RevokeAllPermissions(spectatorvoice, LobbyRoleRadiant);

            await RevokeAllPermissions(castervoice, LobbyRoleDire);
            await RevokeAllPermissions(spectatorvoice, LobbyRoleDire);
        }

        public async Task RevokeAllPermissions(DiscordChannel channel, DiscordRole role)
        {
            await channel.AddOverwriteAsync(role, DSharpPlus.Permissions.None, DSharpPlus.Permissions.AccessChannels);
        }


    }
}
