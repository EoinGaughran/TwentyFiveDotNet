namespace TwentyFiveDotNet.Core.Models
{
    public class PlayedCard
    {
        public Player Player { get; set; }
        public Card Card { get; set; }

        public PlayedCard(Player player, Card card)
        {
            Player = player;
            Card = card;
        }
    }
}
