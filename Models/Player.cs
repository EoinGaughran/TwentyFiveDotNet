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
                foreach (var card in Hand)
                {
                    if (card.Suit.Equals(TrumpCard.Suit))
                    {
                        isThereLedSuit = true;
                    }
                }

                if (isThereLedSuit)
                {
                    foreach (var card in Hand)
                    {
                        if (card.Renegable && card.Score > LedCard.Score)
                        {
                            LegalCards.Add(card);
                            Console.WriteLine($"{card} can Renege.");
                        }
                        else if (card.Suit.Equals(TrumpCard.Suit))
                        {
                            LegalCards.Add(card);
                        }
                        else card.Playable = false;
                    }
                }
                else LegalCards = Hand;
            }

            else
            {
                foreach (var card in Hand)
                {
                    if (card.Suit.Equals(LedCard.Suit))
                    {
                        isThereLedSuit = true;
                    }
                }

                if (isThereLedSuit)
                {
                    foreach (var card in Hand)
                    {
                        if (!(card.Equals(LedCard.Suit) || card.Suit.Equals(TrumpCard.Suit)))
                        {
                            card.Playable = false;
                        }
                        else LegalCards.Add(card);
                    }
                }
                else LegalCards = Hand;
            }

            Console.Write("The legal cards to play are: ");
            Console.WriteLine(String.Join(", ", LegalCards));
        }

        public void ResetPlayableCards()
        {
            foreach (var card in Hand)
            {
                card.Playable = true;
            }
        }

        public void PrintPlayableCards()
        {
            Console.Write("The playable cards are: ");

            foreach (var card in Hand)
            {
                if (card.Playable)
                {
                    Console.Write(card);
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
