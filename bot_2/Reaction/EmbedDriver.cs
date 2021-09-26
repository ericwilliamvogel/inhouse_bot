using bot_2.Json;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace bot_2
{
    public delegate Task ReactFunction(MessageReactionAddEventArgs args);
    public class EmbedDriver //setup = dota / pokemon unite / etc
    {
        //on initial generation of messages
        //read positions from json config
        //
        //read all message ids from generated json
        //have a generic positions message that assigns all roles you ca play
        //and which role is your favorite
        //if no msg # assigned, then regenerate all msg#s in that channel
        //then a dictionary that translates the reaction of a message into logic

        //json has GAME
        //key
        //positions
        //playersingame

        public async Task GenerateControlPanel(CommandContext context)
        {
            var channel = context.Channel;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            .WithTitle("React")
            .AddField("React if you understand the rules.", ":one: = I understand the rules.", false)
            .WithColor(DiscordColor.Red);

            builder.Footer = new EmbedFooter() { Text = "Yoooo" };

            DiscordEmbed embed = builder.Build();
            var message = await channel.SendMessageAsync("", false, embed);

            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":one:"));
            //create embed + reactions
        }

        public async Task<DiscordMessage> GeneratePositions(CommandContext context)
        {
            var channel = context.Channel;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            .WithTitle("Positions")
            .AddField("React with the roles you are able to play.", ":one: = Pos1\n:two: = Pos2\n:three: = Pos3\n:four: = Pos4\n:five: = Pos5", false)
            .WithColor(DiscordColor.Gold);


            builder.Footer = new EmbedFooter() { Text = "" };

            DiscordEmbed embed = builder.Build();
            var message = await channel.SendMessageAsync("", false, embed);

            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":one:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":two:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":three:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":four:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":five:"));
            //create embed + reactions

            JsonCommunicator comm = new JsonCommunicator();
            comm.OverwriteFile("Pos", message.Id.ToString());

            return message;
        }

        public async Task<DiscordMessage> GenerateFavPositions(CommandContext context)
        {
            var channel = context.Channel;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            .WithTitle("Favorite Position")
            .AddField("React with your favorite role.", ":one: = Pos1\n:two: = Pos2\n:three: = Pos3\n:four: = Pos4\n:five: = Pos5", false)
            .WithColor(DiscordColor.Cyan);

            builder.Footer = new EmbedFooter() { Text = "" };

            DiscordEmbed embed = builder.Build();
            var message = await channel.SendMessageAsync("", false, embed);

            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":one:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":two:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":three:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":four:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":five:"));


            JsonCommunicator comm = new JsonCommunicator();
            comm.OverwriteFile("PrefPos", message.Id.ToString());

            return message;
            //create embed + reactions
        }

        public async Task<DiscordMessage> GenerateRegion(CommandContext context)
        {
            var channel = context.Channel;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            .WithTitle("Region")
            .AddField("React with your preferred region.", ":regional_indicator_e: = US EAST\n:regional_indicator_w: = US WEST", false)
            .WithColor(DiscordColor.Purple);

            builder.Footer = new EmbedFooter() { Text = "" };

            DiscordEmbed embed = builder.Build();
            var message = await channel.SendMessageAsync("", false, embed);

            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":regional_indicator_e:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":regional_indicator_w:"));
            //create embed + reactions

            JsonCommunicator comm = new JsonCommunicator();
            comm.OverwriteFile("Region", message.Id.ToString());

            return message;
        }
        public async Task<DiscordMessage> GenerateRank(CommandContext context)
        {
            var channel = context.Channel;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            .WithTitle("Rank")
            .AddField("React with your rank.", ":one: = HERALD\n:two: = GUARDIAN\n:three: = CRUSADER\n:four: = ARCHON\n:five: = LEGEND\n:six: = ANCIENT\n:seven: = DIVINE\n:eight: = IMMORTAL", false)
            .WithColor(DiscordColor.Yellow);

            builder.Footer = new EmbedFooter() { Text = "" };

            DiscordEmbed embed = builder.Build();
            var message = await channel.SendMessageAsync("", false, embed);

            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":one:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":two:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":three:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":four:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":five:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":six:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":seven:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":eight:"));
            //create embed + reactions
            JsonCommunicator comm = new JsonCommunicator();
            comm.OverwriteFile("Rank", message.Id.ToString());

            return message;
        }

        public async Task GenerateRankProgress(CommandContext context)
        {
            var channel = context.Channel;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            .WithTitle("Rank Progress")
            .AddField("React with your rank progress.", "Ex. If you're Ancient 1, react with :one:.", false)
            .WithColor(DiscordColor.Yellow);

            builder.Footer = new EmbedFooter() { Text = "" };

            DiscordEmbed embed = builder.Build();
            var message = await channel.SendMessageAsync("", false, embed);

            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":one:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":two:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":three:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":four:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":five:"));
            //create embed + reactions
            JsonCommunicator comm = new JsonCommunicator();
            comm.OverwriteFile("RankProgress", message.Id.ToString());

            //return message;
        }
    }




}
