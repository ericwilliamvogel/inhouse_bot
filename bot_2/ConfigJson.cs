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
    }
}
