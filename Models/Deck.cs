using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwentyFiveDotNet.Models
{
    public class Deck
    {
        public List<Card> cards;
        public List<Card> DealtCards;

        public Deck()
        {
            cards = new List<Card>();
            DealtCards = new List<Card>();

            foreach (Suits suit in Enum.GetValues(typeof(Suits)))
            {
                foreach (Ranks rank in Enum.GetValues(typeof(Ranks)))
                {
                    cards.Add(new Card
                    {
                        Suit = suit,
                        Rank = rank,
                        Score = GetBaseScore(suit, rank),
                        Playable = true,
                        Renegable = false,
                    });
                }
            }

            int GetBaseScore(Suits suit, Ranks rank)
            {
                // Implement the base scoring logic here
                if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.King) return 12;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Queen) return 11;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Jack) return 10;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Ten) return 9;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Nine) return 8;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Eight) return 7;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Seven) return 6;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Six) return 5;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Five) return 4;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Four) return 3;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Three) return 2;
                else if ((suit == Suits.Hearts || suit == Suits.Diamonds) && rank == Ranks.Two) return 1;
                else if ((suit == Suits.Diamonds) && rank == Ranks.Ace) return 0;

                else if ((suit == Suits.Hearts) && rank == Ranks.Ace) return 100;

                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.King) return 12;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Queen) return 11;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Jack) return 10;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Ten) return 0;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Nine) return 1;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Eight) return 2;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Seven) return 3;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Six) return 4;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Five) return 5;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Four) return 6;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Three) return 7;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Two) return 8;
                else if ((suit == Suits.Clubs || suit == Suits.Spades) && rank == Ranks.Ace) return 9;

                return 0;
            }
        }

        public void Shuffle()
        {
            Random rng = new Random();
            cards = cards.OrderBy(x => rng.Next()).ToList();
        }

        public Card Draw()
        {
            Card card = cards[0];
            cards.RemoveAt(0);
            DealtCards.Add(card);
            return card;
        }

        public void AdjustForTrump(Card TrumpCard)
        {
            foreach (var card in cards)
            {
                ScanTrumpList(card, TrumpCard);
            }

            foreach (var card in DealtCards)
            {
                ScanTrumpList(card, TrumpCard);
            }

            Console.WriteLine();
        }

        private void ScanTrumpList(Card card, Card TrumpCard)
        {
            if (card.Suit == TrumpCard.Suit)
            {
                card.AdjustForTrump(TrumpCard.Suit);
            }
        }
    }
}
