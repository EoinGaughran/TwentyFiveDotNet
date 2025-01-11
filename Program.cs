using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using TwentyFiveDotNet.Game;
using TwentyFiveDotNet.Models;

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
        LeadTurn,
        PlayerTurn,
        NewRound,
        PlayAgain,
        EndGame
    }

    class Program
    {

        static void Main(string[] args)
        {
            GameManager manager = new GameManager();
            Random r = new Random();

            while (manager.CurrentState != GameState.EndGame)
            {

                switch (manager.CurrentState)
                {
                    case GameState.NotStarted:

                    case GameState.Initialize:

                        manager.InitializeGame();
                        manager.AssignRandomDealer(r.Next(0, manager.TotalPlayers));
                        manager.ChangeGameState(GameState.DealCards);

                        break;

                    case GameState.DealCards:

                        manager.DealCards();
                        manager.Deck.AdjustForTrump(manager.TrumpCard);
                        manager.NextPlayer();
                        manager.ChangeGameState(GameState.LeadTurn);

                        break;
                        
                    case GameState.LeadTurn:

                        manager.LeadTurn();
                        manager.ChangeGameState(GameState.PlayerTurn);
                        break;

                    case GameState.PlayerTurn:
                        // Handle the player's turn logic
                        // Example: Check if the player exceeded the score limit

                        for (int i = 1; i < manager.TotalPlayers; i++)
                        {
                            manager.NextPlayer();
                            manager.PlayerTurn();
                        }

                        manager.Scoring();

                        if (manager.IsGameOver())
                        {
                            Console.WriteLine("Game Over!");
                            Console.WriteLine();
                            manager.ChangeGameState(GameState.PlayAgain);
                        }

                        else if (manager.ArePlayersOutOfCards())
                        {
                            manager.ChangeGameState(GameState.NewRound);
                        }
                        else manager.ChangeGameState(GameState.LeadTurn);

                        break;

                    case GameState.NewRound:

                        manager.NewRound();
                        manager.ChangeGameState(GameState.DealCards);

                        break;

                    case GameState.PlayAgain:

                        Console.WriteLine("Would you like to play again? (Y/N)");
                        var response = Console.ReadLine();

                        if (response == "y" || response == "Y")
                        {
                            manager.ChangeGameState(GameState.Initialize);
                            Console.WriteLine("You chose to play a new game.");
                            Console.WriteLine();
                        }
                        else if (response == "n" || response == "N")
                        {
                            Console.WriteLine("You chose to not play a new game.");
                            Console.WriteLine();
                            manager.ChangeGameState(GameState.EndGame);
                        }
                        else
                        {
                            Console.WriteLine("Invalid response, try again.");
                            Console.WriteLine();
                        }

                        break;

                    case GameState.EndGame:
                        // Display results and end the loop
                        
                        break;
                }
            }

            /*foreach (var player in players)
            {
                player.Hand.Add("Ace of Spades");
                Console.WriteLine($"{player.Name} has been dealt their cards.");
            }*/
        }
    }
}
