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
        public List<Card> Hand { get; set; }
        public Card TableAreaCard { get; private set; }

        public Player (String name)
        {
            Name = name;
            Points = 0;
            Hand = new List<Card>();
            TableAreaCard = null;
        }
        public void PlayCard(int index)
        {
            // Where the logic for playing a card should be
        }

        public void SetCardToPlayerTableArea(Card card)
        {
            TableAreaCard = card;
        }
        public void RemovePlayerTableArea()
        {
            TableAreaCard = null;
        }

        public void SetPlayableCards(Card TrumpCard, Card LedCard)
        {
            bool isThereLedSuit = false;
            List<Card> LegalCards = new List<Card>(Hand);

            foreach (var card in LegalCards) card.Playable = false;

            //remove illegal cards
            if (LedCard.Suit.Equals(TrumpCard.Suit))
            {
                foreach (var card in LegalCards)
                {
                    if (card.Suit.Equals(TrumpCard.Suit))
                    {
                        if (!card.Renegable || (card.Renegable && (card.Score < LedCard.Score)))
                        {
                            card.Playable = true;
                            isThereLedSuit = true;
                        }

                        if (card.Renegable && (card.Score > LedCard.Score))
                        {
                            card.Playable = true;
                            Console.WriteLine($"{card} can renege.");
                        }
                    }
                }
            }
            else
            {
                foreach (var card in LegalCards)
                {
                    if (card.Suit.Equals(LedCard.Suit))
                    {
                        isThereLedSuit = true;
                        card.Playable = true;
                    }
                    else if (card.Suit.Equals(TrumpCard.Suit)) card.Playable = true;
                }

                if (isThereLedSuit) Hand = LegalCards;
            }

            if (!isThereLedSuit) foreach (var card in LegalCards) card.Playable = true;

            Console.Write("The legal cards are: ");
            foreach (var card in Hand)
            {
                if (card.Playable)
                {
                    Console.Write($"{card}, ");
                }
                /*else if (!card.Playable)
                {
                    Console.Write($"{card} (not legal), ");
                }*/
            }

            Console.WriteLine();
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
