using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyFiveDotNet.Interfaces;
using TwentyFiveDotNet.Models;
using TwentyFiveDotNet.Utilities;

namespace TwentyFiveDotNet.ConsoleUI
{
    public class ConsoleInteraction : IPlayerInteraction
    {
        private readonly ConsoleSettings _settings;

        public ConsoleInteraction(ConsoleSettings settings)
        {
            _settings = settings;
        }

        public void ShowMessage(string message)
        {
            CustomConsole.WriteLine(message, _settings);
        }

        public void ShowCards(IEnumerable<Card> cards, IEnumerable<Card> legalCards)
        {
            Console.Write($"Players hand: ");
            int i = 1;
            foreach (var card in cards)
            {
                Console.Write($"{i++}: {card}, ");
            }
            Console.WriteLine();

            Console.Write($"Legal Cards to play: ");
            i = 1;
            foreach (var card in legalCards)
            {
                Console.Write($"{i++}: {card}, ");
            }
            Console.WriteLine();
        }

        public int RequestCardChoice(int max)
        {
            int choice;

            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > max)
            {
                Console.WriteLine("Invalid choice, try again.");
            }

            return choice - 1;
        }

        public void WaitForPlayer(string playerName)
        {
            CustomConsole.WriteLine($"{playerName}, press any key when ready.", _settings);
            Console.ReadKey(true);
        }
    }
}