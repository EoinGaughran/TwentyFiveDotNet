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
        
    }
}
