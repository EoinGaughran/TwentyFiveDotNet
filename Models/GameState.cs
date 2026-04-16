using System.Collections.Generic;

namespace TwentyFiveDotNet.Models
{
    public class GameState
    {
        public List<Player> Players;
        public List<Card> Deck;
        public List<Card> TrickPile;

        public int CurrentPlayerIndex;
    }
}
