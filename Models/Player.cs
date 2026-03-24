using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Config;
using TwentyFiveDotNet.Utilities;

namespace TwentyFiveDotNet.Models
{

    public abstract class Player
    {
        protected GameConfig Config { get; }

        protected Player(GameConfig config)
        {
            Config = config;
        }

        public string Name { get; set; }
        public int Points { get; set; }
        public List<Card> Hand { get; set; } = new();
        public Card TableAreaCard { get; set; }

        public override string ToString()
        {
            return Name;
        }
        
        public abstract Card ChooseCard();
        public abstract Card StealTrump();
    }
}
