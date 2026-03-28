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
            WinningPlayer.Points += _config.PointsPerTrick;
        }

        public List<Card> GetPlayableCards(List<Card> hand, Card trumpCard, Card ledCard)
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
                (card.Suit == Suits.Hearts && card.Rank == Ranks.Ace) ||
                (IsTrump(card, trumpCard) &&
                    (card.Rank == Ranks.Ace ||
                     card.Rank == Ranks.Jack ||
                     card.Rank == Ranks.Five));

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
            if (card.Suit == Suits.Hearts && card.Rank == Ranks.Ace)
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
            return card.Rank == Ranks.Ace && card.Suit == Suits.Hearts;
        }

        public bool CanPlayerSteal(List<Card> Hand, Card trumpCard)
        {
            foreach (Card card in Hand)
            {
                if (IsTrump(card, trumpCard) && card.Rank == Ranks.Ace)
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
            if (card.Suit == Suits.Hearts && card.Rank == Ranks.Ace)
                return true;

            return false;
        }

        public bool IsTrumpCardStealable(Card TrumpCard)
        {
            if (TrumpCard.Rank == Ranks.Ace) return true;

            return false;
        }

        public List<Card> GetTrumpCardsSorted(IEnumerable<Card> cards, Card trumpCard)
        {
            return cards
                .Where(c => IsTrump(c, trumpCard))
                .OrderByDescending(c => GetCardScore(c, trumpCard))
                .ToList();
        }

        public Player DetermineWinner(Dictionary<Player, Card> PlayedCards, Card trumpCard)
        {
            Player winner = null;
            Card highestCard = null;

            foreach (var entry in PlayedCards)
            {
                if (highestCard == null || CompareCards(entry.Value, highestCard, trumpCard) > 0)
                {
                    highestCard = entry.Value;
                    winner = entry.Key;
                }
            }

            return winner;
        }

        private int CompareCards(Card card1, Card card2, Card trumpCard)
        {
            if (IsTrump(card1, trumpCard) && !IsTrump(card2, trumpCard)) return 1;
            if (IsTrump(card2, trumpCard) && !IsTrump(card1, trumpCard)) return -1;

            return GetCardScore(card1, trumpCard).CompareTo(GetCardScore(card1, trumpCard));
        }

        public Card GetBestCard(List<Card> Set, Card TrumpCard, Card LedCard)
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

        public Card GetWorstLegalCard(List<Card> Set, Card TrumpCard, Card LedCard)
        {
            Card worstCard = Set[0];

            if (Set.Count > 1)
            {
                var legalCards = GetPlayableCards(Set, TrumpCard, LedCard);

                foreach (var card in legalCards)
                {
                    if (GetCardScore(card, TrumpCard) < GetCardScore(worstCard, TrumpCard)) worstCard = card;
                }
            }
            return worstCard;
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

        public bool HasWon()
        {
            // Logic to determine if this player has won
            return false;
        }
    }
}
