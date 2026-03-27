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

        public string ToStringWords()
        {
            return $"{Rank} of {Suit}";
        }

        public override string ToString()
        {
            string symbol = "symbol null";
            string number = "number null";

            if (Suit == Suits.Hearts) symbol = "♥";
            if (Suit == Suits.Clubs) symbol = "♣";
            if (Suit == Suits.Diamonds) symbol = "♦";
            if (Suit == Suits.Spades) symbol = "♠";

            if (Rank == Ranks.King) number = "K";
            if (Rank == Ranks.Queen) number = "Q";
            if (Rank == Ranks.Jack) number = "J";
            if (Rank == Ranks.Ten) number = "10";
            if (Rank == Ranks.Nine) number = "9";
            if (Rank == Ranks.Eight) number = "8";
            if (Rank == Ranks.Seven) number = "7";
            if (Rank == Ranks.Six) number = "6";
            if (Rank == Ranks.Five) number = "5";
            if (Rank == Ranks.Four) number = "4";
            if (Rank == Ranks.Three) number = "3";
            if (Rank == Ranks.Two) number = "2";
            if (Rank == Ranks.Ace) number = "A";

            return $"{number}{symbol}";
        }

        public string ToUnicodeCardUnicodeSymbols()
        {
            string symbol = "symbol null";

            if (Suit == Suits.Hearts) symbol = "♡";
            if (Suit == Suits.Clubs) symbol = "♧";
            if (Suit == Suits.Diamonds) symbol = "♢";
            if (Suit == Suits.Spades) symbol = "♤";

            return $"{Rank}{symbol}";
        }

        public string GetSuitSymbolUnicoded()
        {
            string symbol = "symbol null";

            if (Suit == Suits.Hearts) symbol = "♥";
            if (Suit == Suits.Clubs) symbol = "♣";
            if (Suit == Suits.Diamonds) symbol = "♦";
            if (Suit == Suits.Spades) symbol = "♠";

            return $"{symbol}";
        }
    }
}
