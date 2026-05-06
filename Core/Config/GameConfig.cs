using System;
using System.IO;
using Newtonsoft.Json;

namespace TwentyFiveDotNet.Core.Config
{
    public class GameConfig
    {
        public bool DevMode { get; set; }
        public bool TestStateMode { get; set; }
        public bool SnapshotOnLaunch { get; set; }
        public string GameTitle { get; set; } = "TwentyFive";
        public int MaxHand { get; set; }
        public int MaxPlayers { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPoints { get; set; }
        public int PointsPerTrick { get; set; }
        public bool HidePlayerHands { get; set; }
        public int DelayInMilliseconds { get; set; }
        public int[] DealPattern { get; set; } = { 3, 2 };
        public CardRulesConfig CardRules { get; set; } = null!;
    }

    public class ConfigLoader
    {
        public static GameConfig LoadGameConfig(string filePath)
        {
            string json = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<GameConfig>(json)
                ?? throw new InvalidOperationException("Failed to deserialize GameConfig");
        }

        public static GameConfig LoadJsonText(string jsonText)
        {
            return JsonConvert.DeserializeObject<GameConfig>(jsonText)
                ?? throw new InvalidOperationException("Failed to deserialize GameConfig");
        }
    }
}
