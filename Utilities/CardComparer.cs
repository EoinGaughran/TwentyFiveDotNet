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
        public static Player DetermineWinner(List<Player> players, Card trumpCard)
        {
            Player winner = null;
            Card highestCard = null;

            foreach (var player in players)
            {
                foreach (var card in player.Hand)
                {
                    if (highestCard == null || CompareCards(card, highestCard, trumpCard) > 0)
                    {
                        highestCard = card;
                        winner = player;
                    }
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
            //pick best card
            Card bestCard = Set.ElementAt(0);

            if (Set.Count > 1)
            {
                for (int i = 1; i < Set.Count; i++)
                {
                    if (Set.ElementAt(i).Playable)
                    {
                        if (Set.ElementAt(i).Score > bestCard.Score) bestCard = Set.ElementAt(i);
                    }
                }
            }
            return bestCard;
        }
    }
}
