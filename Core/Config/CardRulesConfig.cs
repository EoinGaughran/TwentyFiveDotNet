using System;
using System.Collections.Generic;
using static TwentyFiveDotNet.Core.Models.Card;

namespace TwentyFiveDotNet.Core.Config
{
    public class CardRulesConfig
    {
        public List<Suits> RedSuits { get; set; } = null!;
        public List<Suits> BlackSuits { get; set; } = null!;
        public Dictionary<Ranks, int> BaseScoresRed { get; set; } = null!;
        public Dictionary<Ranks, int> BaseScoresBlack { get; set; } = null!;
        public Dictionary<Ranks, int> TrumpBonus { get; set; } = null!;
        public int DefaultTrumpBonus { get; set; }
        public int AceOfHeartsBonus { get; set; }

        public void Validate()
        {
            if (RedSuits == null || RedSuits.Count == 0)
                throw new Exception("RedSuits missing or empty");

            if (BlackSuits == null || BlackSuits.Count == 0)
                throw new Exception("BlackSuits missing or empty");

            if (BaseScoresRed == null)
                throw new Exception("BaseScoresRed missing");

            if (BaseScoresBlack == null)
                throw new Exception("BaseScoresBlack missing");

            if (TrumpBonus == null)
                throw new Exception("TrumpBonus missing");
        }
    }
}
