using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwentyFiveDotNet.Models
{
    public class Player
    {
        public string Name { get; set; }
        public int Points { get; set; }
        public List<Card> Hand { get; set; } = new List<Card>();

        public void PlayCard(int index)
        {
            // Where the logic for playing a card should be
        }

        public void SetPlayableCards(Card TrumpCard, Card LedCard)
        {
            bool isThereLedSuit = false;
            List<Card> LegalCards = new List<Card>();

            //remove illegal cards
            if (LedCard.Suit.Equals(TrumpCard.Suit))
            {
                for (int i = 0; i < Hand.Count; i++)
                {
                    if (Hand.ElementAt(i).Suit.Equals(TrumpCard.Suit))
                    {
                        isThereLedSuit = true;
                    }
                }

                if (isThereLedSuit)
                {
                    for (int i = 0; i < Hand.Count; i++)
                    {
                        if (!(Hand.ElementAt(i).Suit.Equals(TrumpCard.Suit)))
                        {
                            Hand.ElementAt(i).Playable = false;
                        }
                        else LegalCards.Add(Hand.ElementAt(i));
                    }
                }
                else LegalCards = Hand;
            }

            else
            {
                for (int i = 0; i < Hand.Count; i++)
                {
                    if (Hand.ElementAt(i).Suit.Equals(LedCard.Suit))
                    {
                        isThereLedSuit = true;
                    }
                }

                if (isThereLedSuit)
                {
                    for (int i = 0; i < Hand.Count; i++)
                    {
                        if (!(Hand.ElementAt(i).Suit.Equals(LedCard.Suit) || Hand.ElementAt(i).Suit.Equals(TrumpCard.Suit)))
                        {
                            Hand.ElementAt(i).Playable = false;
                        }
                        else LegalCards.Add(Hand.ElementAt(i));
                    }
                }
                else LegalCards = Hand;
            }

            Console.Write("The legal cards to play are: ");
            Console.WriteLine(String.Join(", ", LegalCards));
        }

        public void ResetPlayableCards()
        {
            for (int i = 0; i < Hand.Count; i++)
            {
                Hand.ElementAt((int)i).Playable = true;
            }
        }

        public void PrintPlayableCards()
        {
            Console.Write("The playable cards are: ");

            for (int i = 0; i < Hand.Count; i++)
            {
                if (Hand.ElementAt((int)i).Playable)
                {
                    Console.Write(Hand.ElementAt(i));
                }
            }

            Console.WriteLine();
        }

        public bool HasWon()
        {
            // Logic to determine if this player has won
            return false;
        }
    }
}
