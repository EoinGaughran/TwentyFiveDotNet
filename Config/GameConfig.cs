using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TwentyFiveDotNet.Config
{
    public class GameConfig
    {

        public bool DevMode { get; set; }
        public string GameTitle { get; set; }
        public int MaxHand { get; set; }
        public int MaxPlayers { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPoints { get; set; }
        public int PointsPerTrick { get; set; }
        public bool HidePlayerHands { get; set; }
        public int DelayInMilliseconds { get; set; }
        public int [] DealPattern { get; set; }
        public CardRulesConfig CardRules { get; set; }

        private GameConfig() { }
    }

    public class ConfigLoader
    {
        public static GameConfig LoadGameConfig(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<GameConfig>(json);
        }
    }

    public class WinningRules
    {
        public bool WinOnExactScore { get; set; }
        public bool AllowTie { get; set; }
    }
}
