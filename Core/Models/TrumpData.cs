using System.Collections.Generic;
using TwentyFiveDotNet.Core.Models;

namespace Core.Models
{
    public class TrumpData
    {
        public readonly Dictionary<Card, int> _trumpCards;
        public readonly Card _trumpCard;
        public TrumpData(Card trumpCard, Dictionary<Card, int> trumpCards)
        {
            _trumpCards = trumpCards;
            _trumpCard = trumpCard;
        }
    }
}
