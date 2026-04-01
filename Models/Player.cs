using System.Collections.Generic;

namespace TwentyFiveDotNet.Models
{

    public abstract class Player
    {
        protected Player()
        {}

        public string Name { get; set; }
        public int Points { get; set; }
        public List<Card> Hand { get; set; } = new();
        public Card TableAreaCard { get; set; }

        public override string ToString()
        {
            return Name;
        }
        
        public abstract Card ChooseCard(List<Card> legalCards, Card TrumpCard, Card LedCard);
        public abstract Card LeadCard();
        public abstract Card StealTrump(Card TrumpCard, Card LedCard);
    }
}
