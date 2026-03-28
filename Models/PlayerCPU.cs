using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Config;
using TwentyFiveDotNet.Game;
using TwentyFiveDotNet.Utilities;

namespace TwentyFiveDotNet.Models
{
    public class PlayerCPU : Player
    {
        private readonly RulesEngine _rules;
        public PlayerCPU(string name, RulesEngine rules)
        {
            Name = name;
            _rules = rules;
        }

        public override Card StealTrump(Card TrumpCard, Card LedCard)
        {
            return _rules.GetWorstCard(Hand, TrumpCard);
        }

        public override Card ChooseCard(List<Card> legalCards, Card TrumpCard, Card LedCard)
        {
            return _rules.GetBestCard(Hand, TrumpCard, LedCard);
        }

        public override Card LeadCard()
        {
            return Hand[0]; // Create new best card function
        }
    }
}
