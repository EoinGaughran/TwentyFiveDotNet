using System.Collections.Generic;

namespace TwentyFiveDotNet.Core.Models
{
    public class GameSnapshot
    {
        public IReadOnlyList<Player> Players { get; set; } = new List<Player>();
        public IReadOnlyList<PlayedCard> PlayedCards { get; set; } = new List<PlayedCard>();

        public int DeckCount { get; set; }

        public GamePhase CurrentPhase { get; set; }
        public int TrickNumber { get; set; }

        public Player? Dealer { get; set; }
        public Player? Leader { get; set; }
        public Player? CurrentPlayer { get; set; }

        public Card? TrumpCard { get; set; }
        public Card? LedCard { get; set; }
        public Card? TrickWinningCard { get; set; }
        public Player? TrickWinningPlayer { get; set; }

        public bool TrumpStolen { get; set; }
    }
}