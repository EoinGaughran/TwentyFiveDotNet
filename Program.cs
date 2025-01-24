using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using TwentyFiveDotNet.Game;
using TwentyFiveDotNet.Models;
using TwentyFiveDotNet.Utilities;
using TwentyFiveDotNet.Config;

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
            if (args.Contains("--dev"))
            {
                GameConfig.DevMode = true;
                CustomConsole.DevWriteLineNoDelay("Running in DEV mode.");
            }

            GameManager manager = new GameManager();

            while (manager.CurrentState != GameState.EndGame)
            {

                switch (manager.CurrentState)
                {
                    case GameState.NotStarted:

                    case GameState.Initialize:

                        CustomConsole.WriteLine("Welcome to the card game 25.");
                        CustomConsole.WriteLine($"The game is for {GameConfig.MinPlayers} - {GameConfig.MaxPlayers} players.");
                        CustomConsole.WriteLine("How many total players would you like?");

                        int rTotalPlayers;

                        while (!int.TryParse(Console.ReadLine(), out rTotalPlayers) || rTotalPlayers < GameConfig.MinPlayers || rTotalPlayers > GameConfig.MaxPlayers)
                        {
                            CustomConsole.WriteLine($"Please choose a number between {GameConfig.MinPlayers} - {GameConfig.MaxPlayers} inclusive.");
                        }

                        CustomConsole.WriteLine($"How many Human players would you like? 0 - {rTotalPlayers}");

                        int rTotalHumans;

                        while (!int.TryParse(Console.ReadLine(), out rTotalHumans) || rTotalHumans < 0 || rTotalHumans > rTotalPlayers)
                        {
                            CustomConsole.WriteLine($"Please choose a number between 0 and {rTotalPlayers} inclusive.");
                        }

                        CustomConsole.DevWriteLineNoDelay("Initializing Game.");

                        manager.InitializeGame(rTotalPlayers, rTotalHumans);

                        if(rTotalHumans > 1)
                        {
                            GameConfig.HidePlayerHands = true;
                            CustomConsole.DevWriteLineNoDelay($"Amount of humans is greater than 1, enabling hidden player hands mode.");
                        }
                        else GameConfig.HidePlayerHands = false;

                        for ( int i = 0; i < rTotalHumans; i++)
                        {
                            CustomConsole.Write($"Player {i+1} enter your name: ");
                            var readName = Console.ReadLine();
                            manager.Players.Add(new PlayerHuman(readName));
                        }

                        manager.InitializeCPUs();

                        CustomConsole.DevWriteLineNoDelay($"{manager.Players.Count} players have been created.");
                        General.PrintListOfPlayers(manager.Players);
                        CustomConsole.WriteLine();

                        CustomConsole.Write("Selecting dealer: ");
                        manager.AssignRandomDealer();
                        CustomConsole.WriteLine(manager.Dealer.Name);

                        CustomConsole.DevWriteLineNoDelay($"Creating new deck.");
                        manager.NewDeck();
                        CustomConsole.DevWriteLineNoDelay($"A deck of {manager.Deck.cards.Count} cards has been created.");

                        manager.Deck.Shuffle();
                        CustomConsole.WriteLine("The deck has been shuffled.");

                        manager.ChangeGameState(GameState.DealCards);
                        break;

                    case GameState.DealCards:

                        CustomConsole.WriteLine("Dealing Cards.");
                        manager.DealCards();

                        General.PrintPlayersHands(manager.Players);

                        CustomConsole.DevWriteLineNoDelay($"{manager.Deck.cards.Count} cards remain in the deck.");
                        CustomConsole.WriteLine();

                        manager.AssignTrumpSuit();
                        CustomConsole.WriteLine($"The trump card is: {manager.TrumpCard}");
                        CustomConsole.WriteLine($"The trump suit is: {manager.TrumpCard.GetSuitSymbolUnicoded()}.");

                        CustomConsole.DevWriteLineNoDelay($"{manager.Deck.cards.Count} cards remain in the deck.");

                        CustomConsole.WriteLine();
                        
                        manager.Deck.AdjustForTrump(manager.TrumpCard);
                        General.PrintTrumpScores(manager.Deck.cards, manager.Deck.DealtCards);

                        if (manager.TrumpCard.Rank == Ranks.Ace)
                        {
                            CustomConsole.WriteLine("The Trump card is an Ace so the Dealer gets to steal it.");
                            CustomConsole.WriteLine($"Dealer {manager.Dealer.Name} place down your worst card.");

                            if (!GameConfig.DevMode && GameConfig.HidePlayerHands) manager.CurrentPlayer.IsPlayerReady();

                            manager.CurrentPlayer.SelectWorstCard();

                            manager.CurrentPlayer.Hand.Remove(manager.CurrentPlayer.WorstCard);
                            CustomConsole.WriteLine($"{manager.CurrentPlayer} placed down their worst card");
                            CustomConsole.DevWriteLineNoDelay($"Worst card: {manager.CurrentPlayer.WorstCard}");

                            manager.CurrentPlayer.Hand.Add(manager.TrumpCard);
                            CustomConsole.WriteLine($"{manager.CurrentPlayer} stole the trump card {manager.TrumpCard}.");

                            manager.PlayerStoleTheTrump();

                            manager.NextPlayer();
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
                            CustomConsole.WriteLine($"{manager.CurrentPlayer} has the Ace of Trumps and so gets to steal.");

                            if (!GameConfig.DevMode && GameConfig.HidePlayerHands) manager.CurrentPlayer.IsPlayerReady();

                            manager.CurrentPlayer.SelectWorstCard();

                            manager.CurrentPlayer.Hand.Remove(manager.CurrentPlayer.WorstCard);
                            CustomConsole.WriteLine($"{manager.CurrentPlayer} placed down their worst card");
                            CustomConsole.DevWriteLineNoDelay($"Worst card: {manager.CurrentPlayer.WorstCard}");

                            manager.CurrentPlayer.Hand.Add(manager.TrumpCard);
                            CustomConsole.WriteLine($"{manager.CurrentPlayer} stole the trump card {manager.TrumpCard}.");

                            manager.PlayerStoleTheTrump();

                            manager.ChangeToPlayer(manager.Dealer);
                            manager.ChangeGameState(GameState.LeadTurn);
                        }

                        if (manager.CurrentPlayer == manager.Dealer)
                        {
                            if (!manager.HasPlayerStolen())
                            {
                                CustomConsole.WriteLine("Nobody has the Ace of Trumps so nobody steals.");
                            }

                            CustomConsole.WriteLine();

                            manager.NextPlayer();
                            manager.ChangeGameState(GameState.LeadTurn);
                        }

                        break;

                    case GameState.LeadTurn:

                        CustomConsole.WriteLine($"Press any key when you are ready to start the round.");
                        CustomConsole.WaitForKeyPress();
                        CustomConsole.WriteLine();

                        manager.ResetCardsPlayed();
                        manager.SetLeader(manager.CurrentPlayer);
                        CustomConsole.WriteLine($"{manager.Leader.Name} is now leading the trick.");
                        CustomConsole.WriteLine($"{manager.TrumpCard.GetSuitSymbolUnicoded()} are trumps.");

                        manager.CurrentPlayer.ResetPlayableCards();

                        if (!GameConfig.DevMode && GameConfig.HidePlayerHands) manager.CurrentPlayer.IsPlayerReady();

                        manager.CurrentPlayer.LeadCard();
                        manager.SetLedCard(manager.CurrentPlayer.ChosenCard);
                        manager.SetWinner(manager.CurrentPlayer);
                        manager.UpdatePlayedCards();

                        if(!GameConfig.DevMode && GameConfig.HidePlayerHands) CustomConsole.Clear();
                        General.PrintPlayedCards(manager.PlayedCards);
                        CustomConsole.WriteLine($"{manager.TrumpCard.GetSuitSymbolUnicoded()} are trumps, {manager.LedCard.GetSuitSymbolUnicoded()} were led.");
                        
                        CustomConsole.DevWriteLineNoDelay($"{manager.CurrentPlayer.Name} now has {manager.CurrentPlayer.Hand.Count} cards.");
                        CustomConsole.WriteLine();

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
                            manager.CurrentPlayer.SetPlayableCards(manager.TrumpCard, manager.LedCard);


                            CustomConsole.DevWriteNoDelay($"{CustomConsole.DevPrefix} {manager.CurrentPlayer.Name} has in their hand: ");
                            CustomConsole.DevWriteLineNoDelayNoPrefix(String.Join(", ", manager.CurrentPlayer.Hand));

                            CustomConsole.DevWriteNoDelay($"{CustomConsole.DevPrefix} The legal cards to play are: ");
                            if(GameConfig.DevMode) General.PrintLegalCards(manager.CurrentPlayer.Hand);
                            CustomConsole.DevWriteLine();

                            if(!GameConfig.DevMode && GameConfig.HidePlayerHands) manager.CurrentPlayer.IsPlayerReady();

                            manager.CurrentPlayer.PlayerTurn();
                            manager.UpdatePlayedCards();

                            manager.CurrentPlayer.ResetPlayableCards();

                            manager.UpdateWinner();
                            
                            if (!GameConfig.DevMode && GameConfig.HidePlayerHands)
                            {
                                CustomConsole.Clear();
                                General.PrintPlayedCards(manager.PlayedCards);
                                CustomConsole.WriteLine();
                                CustomConsole.WriteLineNoDelay($"{manager.TrumpCard.GetSuitSymbolUnicoded()} are trumps, {manager.LedCard.GetSuitSymbolUnicoded()} were led.");
                            }
                            else
                            {
                                CustomConsole.WriteLine($"{manager.CurrentPlayer.Name} played {manager.CurrentPlayer.ChosenCard}.");
                            }

                            CustomConsole.DevWriteLineNoDelay($"{manager.CurrentPlayer.Name} now has {manager.CurrentPlayer.Hand.Count} cards.");
                            CustomConsole.DevWriteLine();

                            CustomConsole.WriteLine($"{manager.WinningPlayer.Name} is currently winning with the {manager.WinningCard}.");
                            CustomConsole.WriteLine();
                        }

                        break;

                    case GameState.Scoring:

                        manager.Scoring();
                        CustomConsole.WriteLine($"{manager.WinningPlayer.Name} wins the trick with the {manager.WinningCard} and receives 5 points.");
                        CustomConsole.WriteLine();
                        General.PrintPlayersScores(manager.Players);

                        if (manager.IsGameOver())
                        {
                            CustomConsole.WriteLine("Game Over!");
                            CustomConsole.WriteLine($"{manager.WinningPlayer.Name} wins.");
                            CustomConsole.WriteLine();
                            manager.ChangeGameState(GameState.PlayAgain);
                        }

                        else if (manager.ArePlayersOutOfCards())
                        {
                            manager.ChangeGameState(GameState.NewRound);
                        }
                        else
                        {
                            manager.WinnerBecomesLeader();
                            CustomConsole.WriteLine($"{manager.WinningPlayer.Name} leads the next trick.");
                            CustomConsole.WriteLine();

                            manager.ChangeGameState(GameState.LeadTurn);

                        }

                        break;

                    case GameState.NewRound:

                        manager.RotateDealer();
                        manager.NewDeck();

                        CustomConsole.DevWriteLineNoDelay($"New Deck has been created, it has {manager.Deck.cards.Count} cards.");

                        manager.Deck.Shuffle();
                        CustomConsole.WriteLine($"Deck has been shuffled.");

                        CustomConsole.WriteLine($"The dealer position has rotated clockwise to: {manager.Dealer.Name}");

                        manager.ChangeGameState(GameState.DealCards);
                        break;

                    case GameState.PlayAgain:

                        CustomConsole.WriteLine("Would you like to play again? (Y/N)");
                        var charResponse = Console.ReadLine();

                        if (charResponse == "y" || charResponse == "Y")
                        {
                            manager.ChangeGameState(GameState.Initialize);
                            CustomConsole.Clear();
                            CustomConsole.WriteLine("You chose to play a new game.");
                            CustomConsole.WriteLine();
                        }
                        else if (charResponse == "n" || charResponse == "N")
                        {
                            CustomConsole.WriteLine("You chose to not play a new game.");
                            CustomConsole.WriteLine();
                            manager.ChangeGameState(GameState.EndGame);
                        }
                        else
                        {
                            CustomConsole.WriteLine("Invalid response, try again.");
                            CustomConsole.WriteLine();
                        }

                        break;

                    case GameState.EndGame:
                        // Display results and end the loop
                        
                        break;
                }
            }
        }
    }
}
