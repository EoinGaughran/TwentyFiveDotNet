using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Utilities
{
    public static class CardComparer
    {
        public static Player DetermineWinner(Dictionary<Player, Card> PlayedCards, Card trumpCard)
        {
            Player winner = null;
            Card highestCard = null;

            foreach (var entry in PlayedCards)
            {
                if (highestCard == null || CompareCards(entry.Value, highestCard, trumpCard) > 0)
                {
                    highestCard = entry.Value;
                    winner =entry.Key;
                }
            }

            return winner;
        }

        private static int CompareCards(Card card1, Card card2, Card trumpCard)
        {
            if (card1.Suit == trumpCard.Suit && card2.Suit != trumpCard.Suit) return 1;
            if (card2.Suit == trumpCard.Suit && card1.Suit != trumpCard.Suit) return -1;

            return card1.Score.CompareTo(card2.Score); // Assuming higher value wins
        }

        public static Card GetBestCard(List<Card> Set)
        {
            Card bestCard = Set[0];

            if (Set.Count > 1)
            {
                foreach (var card in Set)
                {
                    if (card.Legal)
                    {
                        if (card.Score > bestCard.Score) bestCard = card;
                    }
                }
            }
            return bestCard;
        }
    }
}
