using TwentyFiveDotNet.ConsoleUI;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Config
{
    public class RuntimeSettings
    {
        public bool HidePlayerHands { get; set; }
        public bool DevMode { get; set; }
        public GameMode GameMode { get; set; }
        public int Delay { get; set; }

    }
}
