using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwentyFiveDotNet.Config
{
    public static class GameConfig
    {
        public static bool DevMode { get; set; } = false;
        public static string GameTitle { get; set; } = "Twenty Five (25)";
        public static int MaxHand { get; set; } = 5;
        public static int MaxPlayers { get; private set; } = 10;
        public static int MinPlayers { get; private set; } = 3;
        public static bool HidePlayerHands { get; set; } = false;
        public static int _delayInMilliseconds { get; private set; } = 500;

        // Optional: Load configuration from a file or arguments
        public static void LoadFromArgs(string[] args)
        {
            if (args.Contains("--dev"))
            {
                DevMode = true;
            }
        }
    }
}
