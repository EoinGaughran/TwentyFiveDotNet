using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Utilities;

namespace TwentyFiveDotNet.Models
{
    public class PlayerHuman : Player
    {
        public PlayerHuman(string name)
        {
            Name = name;
        }

        public override void LeadCard()
        {
            PlayerTurn();
        }
        public override void PlayerTurn()
        {
            ChosenCard = SelectCard();
            Hand.Remove(ChosenCard);
        }

        private Card SelectCard()
        {
            var j = 0;

            CardConsolePrompt();

            while (!int.TryParse(Console.ReadLine(), out j) || j < 1 || j > Hand.Count + 1)
            {
                CardConsolePrompt();
            }

            return Hand[j - 1];
        }

        private void CardConsolePrompt()
        {
            Console.Write($"Enter a number to pick a card: ");

            int i = 0;
            foreach (var card in Hand)
            {
                Console.Write($"{++i} ({card}), ");
            }

            Console.WriteLine();
        }

        public override Card SelectWorstCard()
        {
            WorstCard = SelectCard();
            return WorstCard;
        }
    }
}
