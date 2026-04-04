using System.Collections.Generic;
using TwentyFiveDotNet.Interfaces;

namespace TwentyFiveDotNet.Models
{
    public class PlayerHuman : Player
    {
        private readonly IPlayerInteraction _interaction;

        public PlayerHuman(string name, IPlayerInteraction interaction)
        {
            Name = name;
            _interaction = interaction;
        }
        public override Card ChooseCard(List<Card> legalCards, Card TrumpCard, Card LedCard)
        {
            _interaction.ShowCards(Hand);
            _interaction.ShowLegalCards(Hand, legalCards);
            int choice = _interaction.RequestCardChoice(legalCards.Count);
            return legalCards[choice];          
        }
        public override Card StealTrump(Card TrumpCard, Card LedCard)
        {
            return ChooseCard(Hand, TrumpCard, LedCard);
        }
        public override Card LeadCard()
        {
            _interaction.ShowLegalCards(Hand, Hand);
            int choice = _interaction.RequestCardChoice(Hand.Count);
            return Hand[choice];
        }

        public override void PlayerFlipTrumpCard(Card card)
        {
            _interaction.FlipTrumpCard(card, Name);
        }
    }
}
