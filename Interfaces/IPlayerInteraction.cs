using System.Collections.Generic;
using TwentyFiveDotNet.Models;

namespace TwentyFiveDotNet.Interfaces
{
    public interface IPlayerInteraction
    {
        void ShowMessage(string message);
        void ShowCards(IEnumerable<Card> cards);
        void ShowLegalCards(IEnumerable<Card> cards, IEnumerable<Card> legalCards);
        int RequestCardChoice(int max);
        void WaitForPlayer(string playerName);
        void FlipTrumpCard(Card card, string playerName);
    }
}
