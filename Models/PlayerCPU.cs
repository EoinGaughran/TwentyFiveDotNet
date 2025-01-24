using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Utilities;

namespace TwentyFiveDotNet.Models
{
    public class PlayerCPU : Player
    {
        public PlayerCPU(string name)
        {
            Name = name;
        }

        public override void LeadCard()
        {
            ChosenCard = Hand[0];
            Hand.Remove(ChosenCard);
        }
        public override void PlayerTurn()
        {
            ChosenCard = CardComparer.GetBestCard(Hand);
            Hand.Remove(ChosenCard);
        }

        public override Card SelectWorstCard()
        {
            WorstCard = CardComparer.GetWorstCard(Hand);
            return WorstCard;
        }

        public override void IsPlayerReady()
        {
            //do nothing
        }
    }
}
