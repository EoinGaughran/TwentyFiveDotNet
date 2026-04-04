using System;
using System.Collections.Generic;

namespace TwentyFiveDotNet.Models
{
    public class Deck
    {
        private static readonly Random rng = new Random();

        private readonly List<Card> cards;
        public IReadOnlyList<Card> Cards => cards;

        public Deck()
        {
            cards = new List<Card>();
        }

        public override string ToString()
        {
            return "DECK OF CARDS";
        }

        public void Add52CardsToDeck()
        {
            foreach (Suits suit in Enum.GetValues(typeof(Suits)))
            {
                foreach (Ranks rank in Enum.GetValues(typeof(Ranks)))
                {
                    cards.Add(new Card
                    {
                        Suit = suit,
                        Rank = rank,
                    });
                }
            }
        }

        public void Shuffle()
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }

        public Card Draw()
        {
            if (cards.Count == 0)
                throw new InvalidOperationException("Deck is empty");

            var card = cards[0];
            cards.RemoveAt(0);
            return card;
        }

        public void AddCardToDeck(Card card)
        {
            cards.Add(card);
        }
    }
}
