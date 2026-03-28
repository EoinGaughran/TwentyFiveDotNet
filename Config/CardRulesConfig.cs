using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwentyFiveDotNet.Config
{
    public class CardRulesConfig
    {
        public List<Suits> RedSuits { get; set; }
        public List<Suits> BlackSuits { get; set; }
        public Dictionary<Ranks, int> BaseScoresRed { get; set; }
        public Dictionary<Ranks, int> BaseScoresBlack { get; set; }
        public Dictionary<Ranks, int> TrumpBonus { get; set; }
        public int DefaultTrumpBonus { get; set; }
        public int AceOfHeartsBonus { get; set; }
    }
}
