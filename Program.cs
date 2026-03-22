using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using TwentyFiveDotNet.Game;
using TwentyFiveDotNet.Models;
using TwentyFiveDotNet.Utilities;
using TwentyFiveDotNet.Config;
using System.IO;
using TwentyFiveDotNet.Interfaces;
using TwentyFiveDotNet.ConsoleUI;

namespace TwentyFiveDotNet
{
    public enum Suits
    {
        Hearts = 0,
        Diamonds = 1,
        Clubs = 2,
        Spades = 3,
    }

    public enum Ranks
    {
        Two = 0,
        Three = 1,
        Four = 2,
        Five = 3,
        Six = 4,
        Seven = 5,
        Eight = 6,
        Nine = 7,
        Ten = 8,
        Jack = 9,
        Queen = 10,
        King = 11,
        Ace = 12
    }

    public enum GameState
    {
        NotStarted,
        Initialize,
        DealCards,
        Stealing,
        LeadTurn,
        PlayerTurn,
        Scoring,
        NewRound,
        PlayAgain,
        EndGame
    }

    class Program
    {

        static void Main(string[] args)
        {
            string filePath = "Config/GameConfig.json";
            GameConfig config = ConfigLoader.LoadGameConfig(filePath);
            GameConfig.Load(config);

            ConsoleSettings consoleSettings = new ConsoleSettings();
            consoleSettings.DevMode = config.DevMode;
            consoleSettings.Delay = config.DelayInMilliseconds;

            IPlayerInteraction iPlayerInteraction = new ConsoleInteraction(consoleSettings);

            // Debug values
            if (config != null)
            {
                Console.WriteLine($"DevMode: {config.DevMode}, Instance: {config.DevMode}");
                Console.WriteLine($"MaxPlayers: {config.MaxPlayers}, Instance: {config.MaxPlayers}");
                Console.WriteLine($"GameTitle: {config.GameTitle}, Instance: {config.GameTitle}");
            }
            else
            {
                Console.WriteLine("Failed to load configuration.");
            }

            //RulesEngine rulesEngine = new(config);
            //rulesEngine.ApplyRules();

            if (args.Contains("--dev"))
            {
                config.DevMode = true;
                CustomConsole.DevWriteLineNoDelay("Running in DEV mode.", consoleSettings);
            }

            /*string json = File.ReadAllText(filePath);
            Console.WriteLine("Loaded JSON:");
            Console.WriteLine(json);*/

            CustomConsole.WriteLine("Welcome to the card game 25.", consoleSettings);
            CustomConsole.WriteLine($"The game is for {config.MinPlayers} - {config.MaxPlayers} players.", consoleSettings);
            CustomConsole.WriteLine("How many total players would you like?", consoleSettings);

            int rTotalPlayers;

            while (!int.TryParse(Console.ReadLine(), out rTotalPlayers) || rTotalPlayers < config.MinPlayers || rTotalPlayers > config.MaxPlayers)
            {
                CustomConsole.WriteLine($"Please choose a number between {config.MinPlayers} - {config.MaxPlayers} inclusive.", consoleSettings);
            }

            CustomConsole.WriteLine($"How many Human players would you like? 0 - {rTotalPlayers}", consoleSettings);

            int rTotalHumans;

            while (!int.TryParse(Console.ReadLine(), out rTotalHumans) || rTotalHumans < 0 || rTotalHumans > rTotalPlayers)
            {
                CustomConsole.WriteLine($"Please choose a number between 0 and {rTotalPlayers} inclusive.", consoleSettings);
            }

            CustomConsole.DevWriteLineNoDelay("Initializing Game.", consoleSettings);

            if (rTotalHumans > 1)
            {
                config.HidePlayerHands = true;
                CustomConsole.DevWriteLineNoDelay($"Amount of humans is greater than 1, enabling hidden player hands mode.", consoleSettings);
            }
            else config.HidePlayerHands = false;

            List<Player> Players = new List<Player>();

            for (int i = 0; i < rTotalHumans; i++)
            {
                CustomConsole.Write($"Player {i + 1} enter your name: ", consoleSettings);
                var readName = Console.ReadLine();
                Players.Add(new PlayerHuman(config, readName, iPlayerInteraction));
            }

            for (int i = rTotalHumans; i < rTotalPlayers; i++)
            {
                Players.Add(new PlayerCPU(config, $"CPU Player {i + 1}"));
            }

            RulesEngine rules = new RulesEngine(config);
            GameManager manager = new GameManager(config, rules, Players);

            manager.StartGame();
        }
    }
}
