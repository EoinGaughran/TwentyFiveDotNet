using System;
using System.Collections.Generic;
using System.Linq;
using TwentyFiveDotNet.Core.Config;
using TwentyFiveDotNet.Core.Models;

namespace TwentyFiveDotNet.Core.Game
{
    public class RulesEngine
    {
        private readonly GameConfig _config;

        public RulesEngine(GameConfig config)
        {
            _config = config;
        }

        public int[] GetDealPattern()
        {
            return _config.DealPattern;
        }
        public bool IsCardBetter(Card ChosenCard, Card WinningCard, Card LedCard, Card TrumpCard)
        {
            if (ChosenCard.Suit == LedCard.Suit || IsTrump(ChosenCard, TrumpCard))
            {
                if (GetCardScore(ChosenCard, TrumpCard) > GetCardScore(WinningCard, TrumpCard))
                {
                    return true;
                }
            }
            return false;
        }

        public void Scoring(Player WinningPlayer)
        {
            WinningPlayer.AddPoints(_config.PointsPerTrick);
        }

        public IReadOnlyList<Card> GetPlayableCards(IReadOnlyList<Card> hand, Card trumpCard, Card ledCard)
        {
            bool ledIsTrump = IsTrump(ledCard, trumpCard);

            var trumps = hand.Where(c => IsTrump(c, trumpCard)).ToList();

            // =========================
            // CASE 1: LED IS TRUMP
            // =========================
            if (ledIsTrump)
            {
                if (!trumps.Any())
                    return hand;

                bool hasNonRenegeTrump = trumps.Any(c => !CanRenege(c, ledCard, trumpCard));

                if (hasNonRenegeTrump)
                    return trumps;

                return hand;
            }

            // =========================
            // CASE 2: LED IS NOT TRUMP
            // =========================
            var matchingSuit = hand.Where(c => c.Suit == ledCard.Suit).ToList();

            if (matchingSuit.Any())
            {
                return hand
                    .Where(c => c.Suit == ledCard.Suit || IsTrump(c, trumpCard))
                    .ToList();
            }

            return hand;
        }

        private bool CanRenege(Card card, Card ledCard, Card trumpCard)
        {
            if (!IsTrump(card, trumpCard))
                return false;

            bool isSpecial =
                (card.Suit == Card.Suits.Hearts && card.Rank == Card.Ranks.Ace) ||
                (IsTrump(card, trumpCard) &&
                    (card.Rank == Card.Ranks.Ace ||
                     card.Rank == Card.Ranks.Jack ||
                     card.Rank == Card.Ranks.Five));

            if (!isSpecial)
                return false;

            return GetCardScore(card, trumpCard) > GetCardScore(ledCard, trumpCard);
        }
        public int GetCardScore(Card card, Card trumpCard)
        {
            int score = GetBaseScore(card);

            if (IsTrump(card, trumpCard))
            {
                score += GetTrumpBonus(card);
            }

            // Special case
            if (card.Suit == Card.Suits.Hearts && card.Rank == Card.Ranks.Ace)
            {
                score += 5; // move to config later
            }

            return score;
        }

        public int GetBaseScore(Card card)
        {
            if (_config.CardRules.RedSuits.Contains(card.Suit))
            {
                return _config.CardRules.BaseScoresRed.TryGetValue(card.Rank, out var score)
                    ? score
                    : 0;
            }

            if (_config.CardRules.BlackSuits.Contains(card.Suit))
            {
                return _config.CardRules.BaseScoresBlack.TryGetValue(card.Rank, out var score)
                    ? score
                    : 0;
            }

            throw new Exception($"No base score defined for {card.Rank}");

        }

        public int GetTrumpBonus(Card card)
        {
            if (IsAceOfHearts(card))
                return _config.CardRules.AceOfHeartsBonus;

            if (_config.CardRules.TrumpBonus.TryGetValue(card.Rank, out var bonus))
                return bonus;

            return _config.CardRules.DefaultTrumpBonus;
        }
        private bool IsAceOfHearts(Card card)
        {
            return card.Rank == Card.Ranks.Ace && card.Suit == Card.Suits.Hearts;
        }

        public bool CanPlayerSteal(List<Card> Hand, Card trumpCard)
        {
            foreach (Card card in Hand)
            {
                if (card.Suit == trumpCard.Suit && card.Rank == Card.Ranks.Ace)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsTrump(Card card, Card trumpCard)
        {
            if (card.Suit == trumpCard.Suit)
                return true;

            // special rule
            if (card.Suit == Card.Suits.Hearts && card.Rank == Card.Ranks.Ace)
                return true;

            return false;
        }

        public bool IsTrumpCardStealable(Card TrumpCard)
        {
            if (TrumpCard.Rank == Card.Ranks.Ace) return true;

            return false;
        }

        public List<Card> GetTrumpCardsSorted(IEnumerable<Card> cards, Card trumpCard)
        {
            return cards
                .Where(c => IsTrump(c, trumpCard))
                .OrderByDescending(c => GetCardScore(c, trumpCard))
                .ToList();
        }

        public Card GetBestCard(IReadOnlyList<Card> Set, Card TrumpCard, Card LedCard)
        {
            Card bestCard = Set[0];

            if (Set.Count > 1)
            {
                var legalCards = GetPlayableCards(Set, TrumpCard, LedCard);

                foreach (var card in legalCards)
                {
                    if (GetCardScore(card, TrumpCard) > GetCardScore(bestCard, TrumpCard)) bestCard = card;

                }
            }
            return bestCard;
        }

        public Card GetWorstCard(List<Card> Set, Card TrumpCard)
        {
            Card worstCard = Set[0];

            if (Set.Count > 1)
            {
                foreach (var card in Set)
                {
                    if (GetCardScore(card, TrumpCard) < GetCardScore(worstCard, TrumpCard)) worstCard = card;
                }
            }
            return worstCard;
        }

        public bool IsGameOver(Player WinningPlayer)
        {
            if (WinningPlayer.Points >= _config.MaxPoints)
            {
                return true;
            }
            else return false;
        }
    }
}
