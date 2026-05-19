using System;
using System.Collections.Generic;
using TwentyFiveDotNet.Core.Game;

namespace TwentyFiveDotNet.Core.Models
{
    public class PlayerCPU : Player
    {
        private readonly RulesEngine _rules;
        public PlayerCPU(string name, RulesEngine rules)
        {
            Name = name;
            _rules = rules;
        }

        public Card? Decide(
            PlayerDecisionType type,
            IReadOnlyList<Card>? options,
            Card? trump, Card? led)
        {
            return type switch
            {
                PlayerDecisionType.LeadCard => LeadCard(),

                PlayerDecisionType.PlayCard =>
                options is not null &&
                trump is not null &&
                led is not null
                    ? ChooseCard(options, trump, led)
                    : throw new ArgumentException("PlayCard requires options, trump, and led"),

                PlayerDecisionType.StealTrump => trump is not null
                    ? StealTrump(trump)
                    : throw new ArgumentException("StealTrump requires trump"),

                PlayerDecisionType.FlipTrump => null,

                _ => null,
            };
        }

        public Card StealTrump(Card TrumpCard)
        {
            return _rules.GetWorstCard(_hand, TrumpCard);
        }

        public Card ChooseCard(IReadOnlyList<Card> legalCards, Card TrumpCard, Card LedCard)
        {
            return _rules.GetBestCard(legalCards, TrumpCard, LedCard);
        }

        public Card LeadCard()
        {
            return Hand[0]; // Create new best card function
        }
        public void PlayerFlipTrumpCard(Card card)
        {

        }
    }
}
