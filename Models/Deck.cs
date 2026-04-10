using System;
using System.Collections.Generic;

namespace TwentyFiveDotNet.Models
{
    public class Deck
    {
        private static readonly Random rng = new Random();

        private readonly List<Card> cards;
        private readonly List<Card> dealtCards;
        public IReadOnlyList<Card> Cards => cards;
        public IReadOnlyList<Card> DealtCards => dealtCards;

        public Deck()
        {
            cards = [];
            dealtCards = [];
        }

        public override string ToString()
        {
            return "DECK OF CARDS";
        }

        public void Add52CardsToDeck()
        {
            foreach (Card.Suits suit in Enum.GetValues(typeof(Card.Suits)))
            {
                foreach (Card.Ranks rank in Enum.GetValues(typeof(Card.Ranks)))
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
            dealtCards.Add(card);
            cards.Remove(card);
            return card;
        }

        public void AddCardToDeck(Card card)
        {
            cards.Add(card);
        }
    }
}
