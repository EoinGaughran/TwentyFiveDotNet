using System;
using System.Collections.Generic;

namespace TwentyFiveDotNet.Core.Models
{
    public class GameState
    {
        // Core setup
        public List<Player> Players { get; set; } = new List<Player>();
        public Deck? Deck { get; set; }

        // Turn structure
        public int CurrentPlayerIndex { get; set; }
        public GamePhase CurrentPhase { get; set; } = GamePhase.Initialize;

        // Cards in play
        public List<PlayedCard> PlayedCards { get; set; } = new List<PlayedCard>();

        // Trump & trick info
        public Card? TrumpCard { get; set; }
        public Card? LedCard { get; set; }
        public Card? RoundWinningCard { get; set; }
        public Player? RoundWinningPlayer { get; set; }
        public Player? PendingPlayer { get; set; }
        public List<Card>? PendingOptions { get; set; }
        public PlayerDecisionType? PendingDecisionType { get; set; }

        public bool TrumpStolen { get; set; } = false;

        public List<Player> GetPlayersOrThrow()
        {
            if(Players.Count == 0)
                throw new InvalidOperationException("No Players were set");

            return Players;
        }
        public Deck GetDeckOrThrow()
        {
            return Deck ?? throw new InvalidOperationException("No deck was set");
        }

        public Card GetTrumpCardOrThrow()
        {
            return TrumpCard ?? throw new InvalidOperationException("Trump not set");
        }

        public Card GetLedCardOrThrow()
        {
            return LedCard ?? throw new InvalidOperationException("LedCard not set");
        }

        public Card GetRoundWinningCardOrThrow()
        {
            return RoundWinningCard ?? throw new InvalidOperationException("RoundWinningCard not set");
        }

        public Player GetRoundWinningPlayerOrThrow()
        {
            return RoundWinningPlayer ?? throw new InvalidOperationException("RoundWinningPlayer not set");
        }

        public Player GetPendingPlayerOrThrow()
        {
            return PendingPlayer ?? throw new InvalidOperationException("PendingPlayer not set");
        }

        public List<Card> GetPendingOptionsOrThrow()
        {
            return PendingOptions ?? throw new InvalidOperationException("PendingOptions not set");
        }
    }
}