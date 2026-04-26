using TwentyFiveDotNet.Core.Models;

namespace TwentyFiveDotNet.Core.Config
{
    public class RuntimeSettings
    {
        public bool HidePlayerHands { get; set; }
        public bool DevMode { get; set; }
        public GameMode GameMode { get; set; }
        public int Delay { get; set; }

    }
}
