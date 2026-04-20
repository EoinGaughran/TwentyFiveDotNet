using System.Collections.Generic;

namespace TwentyFiveDotNet.Models
{
    public class GameState
    {
        public List<Player> Players;
        public Deck Deck;
        public List<Card> TrickPile;

        public int CurrentPlayerIndex;

        public bool IsPreconfigured { get; set; } = false;
    }
}
