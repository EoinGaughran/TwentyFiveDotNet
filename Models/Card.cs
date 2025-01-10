using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwentyFiveDotNet.Models
{
    public class Card
    {
        public Suits Suit { get; set; }
        public Ranks Rank { get; set; }
        public int Score { get; set; }
        public bool Playable { get; set; }

        public void AdjustForTrump(Suits trumpSuit)
        {
            if (Suit == trumpSuit)
            {
                Score += 15;
                if (Rank == Ranks.Ace) Score += 50;
                else if (Rank == Ranks.Jack) Score += 200;
                else if (Rank == Ranks.Five) Score += 300;
            }
            else if (Suit == Suits.Hearts && Rank == Ranks.Ace)
            {
                Score += 100;
            }
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }
}
