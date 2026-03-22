using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Config;
using TwentyFiveDotNet.Utilities;

namespace TwentyFiveDotNet.Models
{
    public class PlayerCPU : Player
    {
        public PlayerCPU(GameConfig config, string name) : base(config)
        {
            Name = name;
        }

        public override void PlayerTurn()
        {
            ChosenCard = CardComparer.GetBestCard(Hand);
            Hand.Remove(ChosenCard);
        }
    }
}
