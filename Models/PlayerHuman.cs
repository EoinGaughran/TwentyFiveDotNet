using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Utilities;
using TwentyFiveDotNet.Config;

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
            CustomConsole.Write("Your legal cards to play are: ");
            General.PrintLegalCards(Hand);
            CustomConsole.WriteLine();
            ChosenCard = SelectCard();
            Hand.Remove(ChosenCard);
        }

        private Card SelectCard()
        {
            var j = 0;

            CardConsolePrompt();

            while (!int.TryParse(Console.ReadLine(), out j) || j < 1 || j > Hand.Count )
            {
                CardConsolePrompt();
            }

            return Hand[j - 1];
        }

        private void CardConsolePrompt()
        {
            CustomConsole.Write($"Enter a number to pick a card: ");

            int i = 0;
            foreach (var card in Hand)
            {
                Console.Write($"{++i} ({card}), ");
            }

            CustomConsole.WriteLine();
        }

        public override Card SelectWorstCard()
        {
            WorstCard = SelectCard();
            if (!GameConfig.DevMode) CustomConsole.Clear();
            return WorstCard;
        }
        public override void IsPlayerReady()
        {
            CustomConsole.WriteLine($"{this.Name}, when you are ready, press any key to start your turn. Other players look away.");
            CustomConsole.WaitForKeyPress();
        }
    }
}
