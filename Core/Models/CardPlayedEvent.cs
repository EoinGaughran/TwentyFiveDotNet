using TwentyFiveDotNet.Core.Models;
using static TwentyFiveDotNet.Core.Models.Card;

namespace Core.Models
{
    public class CardPlayedEvent
    {
        public Player Player { get; }
        public Card PlayedCard { get; }
        public Suits TrumpSuit { get; }
        public bool IsLeader { get; }

        public CardPlayedEvent(Player player, Card playedCard, Suits trumpSuit, bool isLeader)
        {
            Player = player;
            PlayedCard = playedCard;
            TrumpSuit = trumpSuit;
            IsLeader = isLeader;
        }
    }
}
