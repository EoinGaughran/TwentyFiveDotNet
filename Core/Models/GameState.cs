namespace TwentyFiveDotNet.Core.Models
{
    public class GameState
    {
        // Core setup
        public List<Player> Players { get; set; } = new();
        public Deck Deck { get; set; }

        // Turn structure
        public int CurrentPlayerIndex { get; set; }
        public int DealerIndex { get; set; }
        public int LeaderIndex { get; set; }
        public GamePhase CurrentPhase { get; set; } = GamePhase.Initialize;

        // Cards in play
        public List<(Player player, Card card)> PlayedCards { get; set; } = new();

        // Trump & trick info
        public Card TrumpCard { get; set; }
        public Card LedCard { get; set; }
        public Card RoundWinningCard { get; set; }
        public Player RoundWinningPlayer { get; set; }
        public Player PendingPlayer { get; set; }
        public List<Card> PendingOptions { get; set; }
        public PlayerDecisionType? PendingDecisionType { get; set; }

        public bool TrumpStolen { get; set; } = false;
    }
}