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
using System.Runtime;

namespace TwentyFiveDotNet
{
    public enum Suits
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades,
    }

    public enum Ranks
    {
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
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
        NewGame,
        EndGame
    }

    class Program
    {
        public static readonly String UIPrefix = "[Program] ";

        static void Main(string[] args)
        {
            string filePath = "Config/GameConfig.json";
            GameConfig config = ConfigLoader.LoadGameConfig(filePath);


            ConsoleSettings consoleSettings = new ConsoleSettings();
            consoleSettings.DevMode = config.DevMode;
            consoleSettings.Delay = config.DelayInMilliseconds;

            IPlayerInteraction iPlayerInteraction = new ConsoleInteraction(consoleSettings);
            IGameInteraction iGameInteraction = new ConsoleGameInteraction(consoleSettings);

            // Debug values
            if (config != null)
            {
                Console.WriteLine(UIPrefix + $"DevMode: {config.DevMode}, Instance: {config.DevMode}");
                Console.WriteLine(UIPrefix + $"MaxPlayers: {config.MaxPlayers}, Instance: {config.MaxPlayers}");
                Console.WriteLine(UIPrefix + $"GameTitle: {config.GameTitle}, Instance: {config.GameTitle}");
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

            CustomConsole.WriteLine(UIPrefix + "Welcome to the card game 25.", consoleSettings);
            CustomConsole.WriteLine(UIPrefix + $"The game is for {config.MinPlayers} - {config.MaxPlayers} players.", consoleSettings);
            CustomConsole.Write(UIPrefix + "How many total players would you like?: ", consoleSettings);

            int rTotalPlayers;

            while (!int.TryParse(Console.ReadLine(), out rTotalPlayers) || rTotalPlayers < config.MinPlayers || rTotalPlayers > config.MaxPlayers)
            {
                CustomConsole.WriteLine(UIPrefix + $"Please choose a number between {config.MinPlayers} - {config.MaxPlayers} inclusive.", consoleSettings);
            }

            CustomConsole.Write(UIPrefix + $"How many Human players would you like? 0 - {rTotalPlayers}: ", consoleSettings);

            int rTotalHumans;

            while (!int.TryParse(Console.ReadLine(), out rTotalHumans) || rTotalHumans < 0 || rTotalHumans > rTotalPlayers)
            {
                CustomConsole.WriteLine(UIPrefix + $"Please choose a number between 0 and {rTotalPlayers} inclusive.", consoleSettings);
            }

            //CustomConsole.DevWriteLineNoDelay("Initializing Game.", consoleSettings);

            if (rTotalHumans > 1)
            {
                config.HidePlayerHands = true;
                CustomConsole.DevWriteLineNoDelay(UIPrefix + $"Amount of humans is greater than 1, enabling hidden player hands mode.", consoleSettings);
            }
            else config.HidePlayerHands = false;

            List<Player> Players = new List<Player>();

            for (int i = 0; i < rTotalHumans; i++)
            {
                CustomConsole.Write($"Player {i + 1} enter your name: ", consoleSettings);
                var readName = Console.ReadLine();
                Players.Add(new PlayerHuman(readName, iPlayerInteraction));
            }

            RulesEngine rules = new RulesEngine(config);

            for (int i = rTotalHumans; i < rTotalPlayers; i++)
            {
                Players.Add(new PlayerCPU($"CPU Player {i + 1}", rules));
            }

            GameManager manager = new GameManager(config, rules, Players, iGameInteraction);
            var gameUI = new ConsoleGameInteraction(consoleSettings);

            manager.OnDealingStarted += gameUI.HandleDealingStarted;
            manager.OnCardsDealtToPlayer += gameUI.HandleCardsDealt;
            manager.OnTrumpCardRevealed += gameUI.HandleTrumpCard;
            manager.ScoreChanged += gameUI.UpdateScores;
            manager.OnMessage += gameUI.ShowMessage;

            manager.StartGame();
        }
    }
}
