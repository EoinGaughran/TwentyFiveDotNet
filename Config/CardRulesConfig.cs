using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwentyFiveDotNet.Config
{
    public class CardRulesConfig
    {
        public Dictionary<Ranks, int> BaseScores { get; set; }
        public Dictionary<Ranks, int> TrumpBonus { get; set; }
        public int DefaultTrumpBonus { get; set; }
    }
}
