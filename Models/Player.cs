using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Config;
using TwentyFiveDotNet.Utilities;

namespace TwentyFiveDotNet.Models
{
    public abstract class Player()
    {
        public string Name { get; set; }
        public int Points { get; set; }
        public List<Card> Hand { get; set; } = new List<Card>();
        public Card TableAreaCard { get; set; }
        public Card ChosenCard { get; set; }
        public Card WorstCard { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public abstract void PlayerTurn();
        public abstract void LeadCard();
        public abstract Card SelectWorstCard();
        public abstract void IsPlayerReady();

        public void SetCardToPlayerTableArea(Card card)
        {
            TableAreaCard = card;
        }
        public void RemovePlayerTableArea()
        {
            TableAreaCard = null;
        }
        public bool CanPlayerSteal(Suits TrumpSuit)
        {
            foreach(Card card in Hand)
            {
                if (card.Suit == TrumpSuit && card.Rank == Ranks.Ace)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetPlayableCards(Card TrumpCard, Card LedCard)
        {
            bool isThereLedSuit = false;

            foreach (var card in Hand)
            {
                card.Legal = IsCardLegal(card, LedCard, ref isThereLedSuit);
            }

            if (!isThereLedSuit)
            {
                foreach (var card in Hand)
                {
                    card.Legal = true;
                }
            }
        }

        private bool IsCardLegal(Card card, Card LedCard, ref bool isThereLedSuit)
        {
            if (LedCard.IsTrump)
            {
                if (card.IsTrump)
                {
                    if (!card.Renegable || (card.Renegable && (card.Score < LedCard.Score)))
                    {
                        isThereLedSuit = true;
                    }

                    return true;
                }

            }
            else
            {
                if (card.Suit == LedCard.Suit)
                {
                    isThereLedSuit = true;
                    return true;
                }
                if (card.IsTrump)
                {
                    return true;
                }
            }

            return false;
        }

        public void ResetPlayableCards()
        {
            foreach (var card in Hand)
            {
                card.Legal = true;
            }
        } 
        public void Steal()
        {
            if (!GameConfig.DevMode && GameConfig.HidePlayerHands) IsPlayerReady();
            Hand.Remove(SelectWorstCard());
        }

        public bool HasWon()
        {
            // Logic to determine if this player has won
            return false;
        }
    }
}
