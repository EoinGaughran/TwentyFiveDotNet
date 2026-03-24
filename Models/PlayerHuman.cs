using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Utilities;
using TwentyFiveDotNet.Config;
using TwentyFiveDotNet.Interfaces;

namespace TwentyFiveDotNet.Models
{
    public class PlayerHuman : Player
    {
        private readonly IPlayerInteraction _interaction;

        public PlayerHuman(GameConfig config, string name, IPlayerInteraction interaction)
            : base(config)
        {
            Name = name;
            _interaction = interaction;
        }

        public override Card ChooseCard()
        {
            _interaction.ShowCards(Hand);
            int choice = _interaction.RequestCardChoice(Hand.Count);
            return Hand[choice];          
        }

        public override Card StealTrump()
        {
            return ChooseCard();
        }
    }
}
