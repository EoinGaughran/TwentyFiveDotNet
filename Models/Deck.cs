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
        public List<Card> Trumps;

        //load from file later
        private int AceHeartsScoreBuff = 100;

        public Deck()
        {
            cards = new List<Card>();
            DealtCards = new List<Card>();
            Trumps = new List<Card>();

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
    }
}
