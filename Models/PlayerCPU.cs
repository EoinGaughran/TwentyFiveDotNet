using System.Collections.Generic;
using TwentyFiveDotNet.Game;
using static TwentyFiveDotNet.Game.GameManager;

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

        public Card Decide(
            PlayerDecisionType type,
            List<Card> options,
            Card trump, Card led)
        {
            return type switch
            {
                PlayerDecisionType.LeadCard => (Card) LeadCard(),
                PlayerDecisionType.PlayCard => (Card) ChooseCard(options, trump, led),
                PlayerDecisionType.StealTrump => (Card) StealTrump(trump, led),
                _ => null,
            };
        }

        public Card StealTrump(Card TrumpCard, Card LedCard)
        {
            return _rules.GetWorstCard(Hand, TrumpCard);
        }

        public Card ChooseCard(List<Card> legalCards, Card TrumpCard, Card LedCard)
        {
            return _rules.GetBestCard(Hand, TrumpCard, LedCard);
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
