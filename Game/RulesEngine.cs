using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Config;
using TwentyFiveDotNet.Models;
using TwentyFiveDotNet.Utilities;

namespace TwentyFiveDotNet.Game
{
    public class RulesEngine
    {
        private GameConfig _config;

        public RulesEngine(GameConfig config)
        {
            _config = config;
        }

        public bool IsCardBetter(Card ChosenCard, Card WinningCard, Card LedCard)
        {
            if (ChosenCard.Suit == LedCard.Suit || ChosenCard.IsTrump)
            {
                if (ChosenCard.Score > WinningCard.Score)
                {
                    return true;
                }
            }
            return false;
        }

        public Card GetWorstCard(List<Card> Hand)
        {
            return CardComparer.GetWorstCard(Hand);
        }

        public void Scoring(Player WinningPlayer)
        {
            WinningPlayer.Points += _config.PointsPerTrick;
        }

        public void SetPlayableCards(List<Card> Hand, Card TrumpCard, Card LedCard)
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

        public bool CanPlayerSteal(List<Card> Hand, Suits TrumpSuit)
        {
            foreach (Card card in Hand)
            {
                if (card.Suit == TrumpSuit && card.Rank == Ranks.Ace)
                {
                    return true;
                }
            }
            return false;
        }

        public void ResetPlayableCards(List<Card> Hand)
        {
            foreach (var card in Hand)
            {
                card.Legal = true;
            }
        }
        public void Steal(List<Card> Hand)
        {
            //if (!Config.DevMode && Config.HidePlayerHands) IsPlayerReady();
            Hand.Remove(GetWorstCard(Hand));
        }

        public bool HasWon()
        {
            // Logic to determine if this player has won
            return false;
        }
    }
}
