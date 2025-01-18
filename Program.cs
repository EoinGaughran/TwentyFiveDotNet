using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using TwentyFiveDotNet.Game;
using TwentyFiveDotNet.Models;
using TwentyFiveDotNet.Utilities;

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
            GameManager manager = new GameManager();

            while (manager.CurrentState != GameState.EndGame)
            {

                switch (manager.CurrentState)
                {
                    case GameState.NotStarted:

                    case GameState.Initialize:

                        Console.WriteLine("Welcome to the card game 25.");
                        Console.WriteLine($"The game is for {manager.MinPlayers} - {manager.MaxPlayers} players.");
                        Console.WriteLine("How many total players would you like?");

                        int rTotalPlayers;
                        while (!int.TryParse(Console.ReadLine(), out rTotalPlayers) || rTotalPlayers < manager.MinPlayers || rTotalPlayers > manager.MaxPlayers)
                        {
                            Console.WriteLine($"Please choose a number between {manager.MinPlayers} - {manager.MaxPlayers} inclusive.");
                        }

                        Console.WriteLine($"How many Human players would you like? 0 - {rTotalPlayers}");

                        int rTotalHumans;

                        while (!int.TryParse(Console.ReadLine(), out rTotalHumans) || rTotalHumans < 0 || rTotalHumans > rTotalPlayers)
                        {
                            Console.WriteLine($"Please choose a number between 0 and {rTotalPlayers} inclusive.");
                        }

                        Console.WriteLine("Initializing Game.");
                        manager.InitializeGame(rTotalPlayers, rTotalHumans);

                        for ( int i = 0; i < rTotalHumans; i++)
                        {
                            Console.Write($"Player {i+1} enter you name: ");
                            var readName = Console.ReadLine();
                            manager.Players.Add(new PlayerHuman(readName));
                        }

                        manager.InitializeCPUs();

                        Console.WriteLine($"{manager.Players.Count} players have been created.");
                        General.PrintListOfPlayers(manager.Players);
                        Console.WriteLine();

                        Console.Write("Selecting dealer: ");
                        manager.AssignRandomDealer();
                        Console.WriteLine(manager.Dealer.Name);

                        Console.WriteLine($"Creating new deck.");
                        manager.NewDeck();
                        Console.WriteLine($"A deck of {manager.Deck.cards.Count} cards has been created.");
                        manager.Deck.Shuffle();
                        Console.WriteLine("The deck has been shuffled.");

                        manager.ChangeGameState(GameState.DealCards);
                        break;

                    case GameState.DealCards:

                        Console.WriteLine("Dealing Cards.");
                        manager.DealCards();
                        General.PrintPlayersHands(manager.Players);
                        Console.WriteLine($"{manager.Deck.cards.Count} cards remain in the deck.");
                        Console.WriteLine();

                        manager.AssignTrumpSuit();
                        Console.WriteLine($"The trump card is: {manager.TrumpCard}");
                        Console.WriteLine($"The trump suit is: {manager.TrumpCard.Suit}.");
                        Console.WriteLine($"{manager.Deck.cards.Count} cards remain in the deck.");
                        Console.WriteLine();
                        
                        manager.Deck.AdjustForTrump(manager.TrumpCard);
                        General.PrintTrumpScores(manager.Deck.cards, manager.Deck.DealtCards);
                        Console.WriteLine();

                        if (manager.TrumpCard.Rank == Ranks.Ace)
                        {
                            Console.WriteLine("The Trump card is an Ace so the Dealer gets to steal it.");
                            Console.WriteLine("Dealer place down your worst card.");

                            manager.Dealer.SelectWorstCard();
                            manager.Dealer.Hand.Remove(manager.Dealer.WorstCard);
                            manager.Dealer.Hand.Add(manager.TrumpCard);
                            Console.WriteLine($"{manager.CurrentPlayer} places down his worst card {manager.CurrentPlayer.WorstCard} and steals the {manager.TrumpCard}");
                            Console.WriteLine();

                            manager.ChangeGameState(GameState.LeadTurn);
                        }
                        else
                        {
                            manager.ChangeGameState(GameState.Stealing);
                        }

                        break;

                    case GameState.Stealing:

                        manager.NextPlayer();

                        if (manager.CurrentPlayer.CanPlayerSteal(manager.TrumpCard.Suit))
                        {
                            Console.WriteLine($"{manager.CurrentPlayer} has the Ace of Trumps and so gets to steal.");

                            manager.CurrentPlayer.SelectWorstCard();
                            manager.CurrentPlayer.Hand.Remove(manager.CurrentPlayer.WorstCard);
                            manager.CurrentPlayer.Hand.Add(manager.TrumpCard);
                            
                            Console.WriteLine($"{manager.CurrentPlayer} places down his worst card {manager.CurrentPlayer.WorstCard} and steals the {manager.TrumpCard}.");

                            manager.PlayerStoleTheTrump();

                            manager.ChangeToPlayer(manager.Dealer);
                            manager.ChangeGameState(GameState.LeadTurn);
                        }

                        if (manager.CurrentPlayer == manager.Dealer)
                        {
                            if (!manager.HasPlayerStolen())
                            {
                                Console.WriteLine("Nobody has the Ace of Trumps.");
                            }

                            Console.WriteLine();

                            manager.NextPlayer();
                            manager.ChangeGameState(GameState.LeadTurn);
                        }

                        break;

                    case GameState.LeadTurn:

                        manager.SetLeader(manager.CurrentPlayer);
                        Console.WriteLine($"{manager.Leader.Name} is now leading the trick.");
                        Console.WriteLine($"{manager.TrumpCard.Suit} are trumps.");

                        manager.CurrentPlayer.ResetPlayableCards();
                        manager.CurrentPlayer.LeadCard();
                        manager.SetLedCard(manager.CurrentPlayer.ChosenCard);
                        manager.SetWinner(manager.CurrentPlayer);

                        Console.WriteLine($"{manager.CurrentPlayer.Name} plays {manager.CurrentPlayer.ChosenCard}.");
                        Console.WriteLine($"{manager.LedCard.Suit} is the suit led.");
                        Console.WriteLine($"{manager.CurrentPlayer.Name} now has {manager.CurrentPlayer.Hand.Count} cards.");
                        Console.WriteLine();

                        manager.ChangeGameState(GameState.PlayerTurn);
                        break;

                    case GameState.PlayerTurn:

                        manager.NextPlayer();

                        if (manager.CurrentPlayer == manager.Leader)
                        {
                            manager.ChangeGameState(GameState.Scoring);
                        }
                        else
                        {
                            Console.Write($"{manager.CurrentPlayer.Name} has in their hand: ");
                            Console.WriteLine(String.Join(", ", manager.CurrentPlayer.Hand));

                            manager.CurrentPlayer.SetPlayableCards(manager.TrumpCard, manager.LedCard);
                            Console.Write("The legal cards to play are: ");
                            General.PrintLegalCards(manager.CurrentPlayer.Hand);
                            Console.WriteLine();
                            Console.WriteLine($"{manager.TrumpCard.Suit} are trumps, {manager.LedCard.Suit} were led.");

                            manager.CurrentPlayer.PlayerTurn();
                            Console.WriteLine($"{manager.CurrentPlayer.Name} plays {manager.CurrentPlayer.ChosenCard}.");

                            manager.CurrentPlayer.ResetPlayableCards();

                            manager.UpdateWinner();
                            Console.WriteLine($"{manager.CurrentPlayer.Name} now has {manager.CurrentPlayer.Hand.Count} cards.");
                            Console.WriteLine($"{manager.WinningPlayer.Name} is currently winning with the {manager.WinningCard}.");
                            Console.WriteLine();
                        }

                        break;

                    case GameState.Scoring:

                        manager.Scoring();
                        Console.WriteLine($"{manager.WinningPlayer.Name} wins the trick with the {manager.WinningCard} and receives 5 points.");
                        Console.WriteLine();
                        General.PrintPlayersScores(manager.Players);

                        if (manager.IsGameOver())
                        {
                            Console.WriteLine("Game Over!");
                            Console.WriteLine($"{manager.WinningPlayer.Name} wins.");
                            Console.WriteLine();
                            manager.ChangeGameState(GameState.PlayAgain);
                        }

                        else if (manager.ArePlayersOutOfCards())
                        {
                            manager.ChangeGameState(GameState.NewRound);
                        }
                        else
                        {
                            manager.WinnerBecomesLeader();
                            Console.WriteLine($"{manager.WinningPlayer.Name} leads the next trick.");
                            Console.WriteLine();

                            manager.ChangeGameState(GameState.LeadTurn);

                        }

                        break;

                    case GameState.NewRound:

                        manager.RotateDealer();
                        manager.NewDeck();

                        Console.WriteLine($"The dealer position has rotated clockwise to: {manager.Dealer.Name}");

                        manager.ChangeGameState(GameState.DealCards);
                        break;

                    case GameState.PlayAgain:

                        Console.WriteLine("Would you like to play again? (Y/N)");
                        var charResponse = Console.ReadLine();

                        if (charResponse == "y" || charResponse == "Y")
                        {
                            manager.ChangeGameState(GameState.Initialize);
                            Console.WriteLine("You chose to play a new game.");
                            Console.WriteLine();
                        }
                        else if (charResponse == "n" || charResponse == "N")
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
