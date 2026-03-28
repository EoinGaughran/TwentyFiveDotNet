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
        public static readonly String UIPrefix = "[Player UI] ";

        public ConsoleInteraction(ConsoleSettings settings)
        {
            _settings = settings;
        }

        public void ShowMessage(string message)
        {
            CustomConsole.WriteLine(UIPrefix+message, _settings);
        }

        public void ShowCards(IEnumerable<Card> cards)
        {
            CustomConsole.Write(UIPrefix, _settings);
            Console.Write($"Players hand: ");
            int i = 1;
            foreach (var card in cards)
            {
                Console.Write($"{i++}: {card}, ");
            }
            Console.WriteLine();
        }

        public void ShowLegalCards(IEnumerable<Card> cards, IEnumerable<Card> legalCards)
        {
            CustomConsole.Write(UIPrefix, _settings);

            Console.Write($"Legal Cards to play: ");
            int i = 1;
            foreach (var card in legalCards)
            {
                Console.Write($"{i++}: {card}, ");
            }
            Console.WriteLine();
        }

        public int RequestCardChoice(int max)
        {
            CustomConsole.Write(UIPrefix+"Enter your choice: ", _settings);

            int choice;

            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > max)
            {
                Console.WriteLine("Invalid choice, try again.");
            }

            return choice - 1;
        }

        public void WaitForPlayer(string playerName)
        {
            CustomConsole.WriteLine($"UIPrefix+{playerName}, press any key when ready.", _settings);
            Console.ReadKey(true);
        }
    }
}