using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace bot_2
{
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
    }

    public struct ReactMessageConfigJson
    {
        [JsonProperty("positions")]
        public ulong Positions { get; private set; }

        [JsonProperty("favorite_position")]
        public ulong FavoritePosition { get; private set; }

        [JsonProperty("preferred_region")]
        public ulong Region { get; private set; }

        [JsonProperty("player_drafter")]
        public ulong PlayerDrafter { get; private set; }

        [JsonProperty("dota_drafter")]
        public ulong DotaDrafter { get; private set; }

        [JsonProperty("herald")]
        public ulong Herald { get; private set; }

        [JsonProperty("guardian")]
        public ulong Guardian { get; private set; }

        [JsonProperty("crusader")]
        public ulong Crusader { get; private set; }

        [JsonProperty("archon")]
        public ulong Archon { get; private set; }

        [JsonProperty("legend")]
        public ulong Legend { get; private set; }

        [JsonProperty("ancient")]
        public ulong Ancient { get; private set; }

        [JsonProperty("divine")]
        public ulong Divine { get; private set; }

        [JsonProperty("immortal")]
        public ulong Immortal { get; private set; }
    }

    public struct ChannelConfigJson
    {
        [JsonProperty("queue_channel")]
        public ulong QueueChannel { get; private set; }

        [JsonProperty("queue_message")]
        public ulong QueueMessage { get; private set; }

        [JsonProperty("leaderboard_channel")]
        public ulong LeaderboardChannel { get; private set; }

        [JsonProperty("commands_channel")]
        public ulong CommandsChannel { get; private set; }

        [JsonProperty("error_channel")]
        public ulong ErrorChannel { get; private set; }

        [JsonProperty("bet_channel")]
        public ulong BetChannel { get; private set; }

        [JsonProperty("game_history_channel")]
        public ulong GameHistoryChannel { get; private set; }

        [JsonProperty("general_channel")]
        public ulong GeneralChannel { get; private set; }

        [JsonProperty("commands_playback_channel")]
        public ulong CommandsPlaybackChannel { get; private set; }

        [JsonProperty("admin_commands_channel")]
        public ulong AdminCommandsChannel { get; private set; }

        [JsonProperty("registration_channel")]
        public ulong RegistrationChannel { get; private set; }
    }
}
