using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Utilities;

namespace TwentyFiveDotNet.Models
{
    public class Player(string name)
    {
        public string Name { get; set; } = name;
        public int Points { get; set; } = 0;
        public List<Card> Hand { get; set; } = new List<Card>();
        public Card TableAreaCard { get; private set; } = null;
        public Card ChosenCard { get; private set; }

        public void PlayFirstCard()
        {
            ChosenCard = Hand[0];
            Hand.Remove(ChosenCard);
        }
        public void PlayBestCard()
        {
            ChosenCard = CardComparer.GetBestCard(Hand);
            Hand.Remove(ChosenCard);
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

            foreach (var card in LegalCards) card.Legal = false;

            //remove illegal cards
            if (LedCard.Suit.Equals(TrumpCard.Suit))
            {
                foreach (var card in LegalCards)
                {
                    if (card.Suit.Equals(TrumpCard.Suit))
                    {
                        if (!card.Renegable || (card.Renegable && (card.Score < LedCard.Score)))
                        {
                            card.Legal = true;
                            isThereLedSuit = true;
                        }

                        if (card.Renegable && (card.Score > LedCard.Score))
                        {
                            card.Legal = true;
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
                        card.Legal = true;
                    }
                    else if (card.Suit.Equals(TrumpCard.Suit)) card.Legal = true;
                }

                if (isThereLedSuit) Hand = LegalCards;
            }

            if (!isThereLedSuit) foreach (var card in LegalCards) card.Legal = true;
        }

        public void ResetPlayableCards()
        {
            foreach (var card in Hand)
            {
                card.Legal = true;
            }
        }

        public bool HasWon()
        {
            // Logic to determine if this player has won
            return false;
        }
    }
}
