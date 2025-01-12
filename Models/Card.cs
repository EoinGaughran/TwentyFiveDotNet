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
        public bool Trump { get; set; }
        public bool Legal { get; set; }
        public bool Renegable { get; set; }

        //load from file later
        private int StandardScoreBuff = 15;
        private int AceScoreBuff = 50;
        private int AceHeartsScoreBuff = 100;
        private int JackScoreBuff = 200;
        private int FiveScoreBuff = 300;

        public void AdjustForTrump(Suits trumpSuit)
        {
            if (Suit == trumpSuit)
            {
                Score += StandardScoreBuff;

                if (Rank == Ranks.Ace)
                {
                    Score += AceScoreBuff;
                }
                else if (Rank == Ranks.Jack)
                {
                    Score += JackScoreBuff;
                    Renegable = true;
                }
                else if (Rank == Ranks.Five)
                {
                    Score += FiveScoreBuff;
                    Renegable = true;
                }

                Trump = true;
            }

            if (Suit == Suits.Hearts && Rank == Ranks.Ace)
            {
                Score += AceHeartsScoreBuff;
                Trump = true;
                Renegable = true;
            }
        }

        public void SetPlayable(bool legal)
        {
            Legal = legal;
        }

        public void SetTrump(bool trumpStatus)
        {
            Trump = trumpStatus;
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }
}
