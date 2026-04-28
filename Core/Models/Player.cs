using System.Collections.Generic;

namespace TwentyFiveDotNet.Core.Models
{
    public abstract class Player
    {
        protected Player()
        {}

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Points { get; set; }
        public List<Card> Hand { get; set; } = new();

        public override string ToString()
        {
            return Name;
        }

        public Player(string name)
        {
            Name = name;
            Hand = new List<Card>();
            Points = 0;
        }
    }
}
