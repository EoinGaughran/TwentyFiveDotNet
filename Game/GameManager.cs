using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TwentyFiveDotNet.Config;
using TwentyFiveDotNet.Interfaces;
using TwentyFiveDotNet.Models;
using TwentyFiveDotNet.Utilities;

namespace TwentyFiveDotNet.Game
{
    internal class GameManager
    {
        private readonly GameConfig _config;
        private readonly RulesEngine _rules;

        private List<Player> _players { get; set; }
        private Dictionary<Player, Card> PlayedCards { get; set; }
        private Deck Deck { get; set; }
        private GameState CurrentState { get; set; }
        private Player Dealer { get; set; }
        private Player Leader { get; set; }
        private Player CurrentPlayer { get; set; }
        private Player WinningPlayer { get; set; }
        private int DealerPlayerNumber { get; set; }
        private int CurrentPlayerNumber { get; set; }
        private int WinningPlayerNumber { get; set; }
        private Card TrumpCard { get; set; }
        private Card LedCard { get; set; }
        private Card WinningCard { get; set; }
        private bool Steal {  get; set; }

        //load from file later
        private readonly int TwoCards = 2;
        private readonly int ThreeCards = 3;
        
        public GameManager(GameConfig config, RulesEngine rules, List<Player> players)
        {
            _config = config;
            _rules = rules;
            _players = players;

            // Initialize the properties in the constructor
            CurrentState = GameState.NotStarted;
        }

        private void AssignRandomDealer()
        {
            Random r = new Random();
            DealerPlayerNumber = r.Next(0, _players.Count);
            Dealer = _players[DealerPlayerNumber];
        }

        private void RotateDealer()
        {
            DealerPlayerNumber = NextPlayerNumber(DealerPlayerNumber);
            Dealer = _players[DealerPlayerNumber];
        }

        private void NewDeck()
        {
            Deck = new Deck();
        }

        private void ResetCardsPlayed()
        {
            PlayedCards = new Dictionary<Player, Card>();
        }

        private void UpdatePlayedCards()
        {
            PlayedCards.Add(CurrentPlayer, CurrentPlayer.ChosenCard);
        }

        private void DealCards()
        {
            ChangeToPlayer(DealerPlayerNumber);

            for (int i = 0; i < _players.Count; i++)
            {
                NextPlayer();
                GivePlayerCards(TwoCards);
            }

            for (int i = 0; i < _players.Count; i++)
            {
                NextPlayer();
                GivePlayerCards(ThreeCards);
            }
        }

        private void AssignTrumpSuit()
        {
            TrumpCard = Deck.Draw();
            Steal = false;
        }

        private void GivePlayerCards(int cards)
        {
            for (int i = 0; i < cards; i++)
            {
                CurrentPlayer.Hand.Add(Deck.Draw());
            }
        }

        private int NextPlayerNumber(int PlayerNumber)
        {
            return (PlayerNumber + 1) % _players.Count; // Move to the indexer to the next player
        }

        private void NextPlayer()
        {
            CurrentPlayerNumber = NextPlayerNumber(CurrentPlayerNumber);
            CurrentPlayer = _players[CurrentPlayerNumber];
        }

        private void ChangeToPlayer(int playerNumber)
        {
            CurrentPlayerNumber = playerNumber;
            CurrentPlayer = _players[playerNumber];
        }

        private void ChangeToPlayer(Player player)
        {
            CurrentPlayerNumber = _players.IndexOf(player);
            CurrentPlayer = player;
        }
        private void SetLeader(Player player)
        {
            Leader = player;
        }

        private void SetLedCard(Card card)
        {
            LedCard = card;
        }
        private void SetWinner()
        {
            WinningCard = CurrentPlayer.ChosenCard;
            WinningPlayer = CurrentPlayer;
            WinningPlayerNumber = _players.IndexOf(CurrentPlayer);
        }

        private void WinnerBecomesLeader()
        {
            CurrentPlayer = WinningPlayer;
            CurrentPlayerNumber = WinningPlayerNumber;
        }

        private void PlayerStoleTheTrump()
        {
            Steal = true;
        }

        private bool IsGameOver()
        {
            if (WinningPlayer.Points >= _config.MaxPoints)
            {
                return true;
            }
            else return false;
        }

        private bool ArePlayersOutOfCards()
        {
            if (_players[0].Hand.Count == 0) return true;
            else return false;
        }

        private bool HasPlayerStolen()
        {
            if (Steal)
            {
                return true;
            }
            else return false;
        }

        private void ChangeGameState(GameState state)
        {
            CurrentState = state;
        }

        public void StartGame()
        {
            while (CurrentState != GameState.EndGame)
            {
                ConsoleSettings consoleSettings = new ConsoleSettings();
                consoleSettings.DevMode = _config.DevMode;
                consoleSettings.Delay = _config.DelayInMilliseconds;

                switch (CurrentState)
                {
                    case GameState.NotStarted:

                    case GameState.Initialize:

                        CustomConsole.DevWriteLineNoDelay($"{_players.Count} players have been created.", consoleSettings);
                        CustomConsole.PrintListOfPlayers(_players, consoleSettings);
                        CustomConsole.WriteLine();

                        CustomConsole.Write("Selecting dealer: ", consoleSettings);
                        AssignRandomDealer();
                        CustomConsole.WriteLine(Dealer.Name, consoleSettings);

                        CustomConsole.DevWriteLineNoDelay($"Creating new deck.", consoleSettings);
                        NewDeck();
                        CustomConsole.DevWriteLineNoDelay($"A deck of {Deck.cards.Count} cards has been created.", consoleSettings);

                        Deck.Shuffle();
                        CustomConsole.WriteLine("The deck has been shuffled.", consoleSettings);

                        ChangeGameState(GameState.DealCards);
                        break;

                    case GameState.DealCards:

                        CustomConsole.WriteLine("Dealing Cards.", consoleSettings);
                        DealCards();

                        CustomConsole.PrintPlayersHands(_players, consoleSettings);

                        CustomConsole.DevWriteLineNoDelay($"{Deck.cards.Count} cards remain in the deck.", consoleSettings);
                        CustomConsole.WriteLine();

                        AssignTrumpSuit();
                        CustomConsole.WriteLine($"The trump card is: {TrumpCard}", consoleSettings);
                        CustomConsole.WriteLine($"The trump suit is: {TrumpCard.GetSuitSymbolUnicoded()}.", consoleSettings);

                        CustomConsole.DevWriteLineNoDelay($"{Deck.cards.Count} cards remain in the deck.", consoleSettings);

                        CustomConsole.WriteLine();

                        Deck.AdjustForTrump(TrumpCard);
                        CustomConsole.PrintTrumpScores(Deck.cards, Deck.DealtCards, consoleSettings);

                        if (TrumpCard.Rank == Ranks.Ace)
                        {
                            CustomConsole.WriteLine("The Trump card is an Ace so the Dealer gets to steal it.", consoleSettings);
                            CustomConsole.WriteLine($"Dealer {Dealer.Name} place down your worst card.", consoleSettings); ;

                            //if (!_config.DevMode && _config.HidePlayerHands) CurrentPlayer.IsPlayerReady();

                            //CurrentPlayer.GetWorstCard(_rules.GetWorstCard);

                            CustomConsole.WriteLine($"{CurrentPlayer} placed down their worst card", consoleSettings);
                            CustomConsole.DevWriteLineNoDelay($"Worst card: {_rules.GetWorstCard(CurrentPlayer.Hand)}", consoleSettings);
                            CurrentPlayer.Hand.Remove(_rules.GetWorstCard(CurrentPlayer.Hand));

                            CurrentPlayer.Hand.Add(TrumpCard);
                            CustomConsole.WriteLine($"{CurrentPlayer} stole the trump card {TrumpCard}.", consoleSettings);

                            PlayerStoleTheTrump();

                            NextPlayer();
                            ChangeGameState(GameState.LeadTurn);
                        }
                        else
                        {
                            ChangeGameState(GameState.Stealing);
                        }

                        break;

                    case GameState.Stealing:

                        NextPlayer();

                        if (_rules.CanPlayerSteal(CurrentPlayer.Hand, TrumpCard.Suit))
                        {
                            CustomConsole.WriteLine($"{CurrentPlayer} has the Ace of Trumps and so gets to steal.", consoleSettings);

                            //if (!_config.DevMode && _config.HidePlayerHands) CurrentPlayer.IsPlayerReady();

                            //CurrentPlayer.SelectWorstCard();

                            CustomConsole.WriteLine($"{CurrentPlayer} placed down their worst card", consoleSettings);
                            CustomConsole.DevWriteLineNoDelay($"Worst card: {_rules.GetWorstCard(CurrentPlayer.Hand)}", consoleSettings);
                            CurrentPlayer.Hand.Remove(_rules.GetWorstCard(CurrentPlayer.Hand));

                            CurrentPlayer.Hand.Add(TrumpCard);
                            CustomConsole.WriteLine($"{CurrentPlayer} stole the trump card {TrumpCard}.", consoleSettings);

                            PlayerStoleTheTrump();

                            ChangeToPlayer(Dealer);
                            ChangeGameState(GameState.LeadTurn);
                        }

                        if (CurrentPlayer == Dealer)
                        {
                            if (!HasPlayerStolen())
                            {
                                CustomConsole.WriteLine("Nobody has the Ace of Trumps so nobody steals.", consoleSettings);
                            }

                            CustomConsole.WriteLine();

                            NextPlayer();
                            ChangeGameState(GameState.LeadTurn);
                        }

                        break;

                    case GameState.LeadTurn:

                        CustomConsole.WriteLine($"Press any key when you are ready to start the round.", consoleSettings);
                        CustomConsole.WaitForKeyPress();
                        CustomConsole.WriteLine();

                        ResetCardsPlayed();
                        SetLeader(CurrentPlayer);
                        CustomConsole.WriteLine($"{Leader.Name} is now leading the trick.", consoleSettings);
                        CustomConsole.WriteLine($"{TrumpCard.GetSuitSymbolUnicoded()} are trumps.", consoleSettings);

                        _rules.ResetPlayableCards(CurrentPlayer.Hand);

                        //if (!_config.DevMode && _config.HidePlayerHands) CurrentPlayer.IsPlayerReady();

                        CurrentPlayer.PlayerTurn();
                        SetLedCard(CurrentPlayer.ChosenCard);
                        SetWinner();
                        UpdatePlayedCards();

                        if (!_config.DevMode && _config.HidePlayerHands) CustomConsole.Clear();
                        CustomConsole.PrintPlayedCards(PlayedCards);
                        CustomConsole.WriteLine($"{TrumpCard.GetSuitSymbolUnicoded()} are trumps, {LedCard.GetSuitSymbolUnicoded()} were led.", consoleSettings);

                        CustomConsole.DevWriteLineNoDelay($"{CurrentPlayer.Name} now has {CurrentPlayer.Hand.Count} cards.", consoleSettings);
                        CustomConsole.WriteLine();

                        ChangeGameState(GameState.PlayerTurn);
                        break;

                    case GameState.PlayerTurn:

                        NextPlayer();

                        if (CurrentPlayer == Leader)
                        {
                            ChangeGameState(GameState.Scoring);
                        }
                        else
                        {
                            _rules.SetPlayableCards(CurrentPlayer.Hand, TrumpCard, LedCard);

                            CustomConsole.DevWriteNoDelay($"{CustomConsole.DevPrefix} {CurrentPlayer.Name} has in their hand: ", consoleSettings);
                            CustomConsole.DevWriteLineNoDelayNoPrefix(String.Join(", ", CurrentPlayer.Hand), consoleSettings);

                            CustomConsole.DevWriteNoDelay($"{CustomConsole.DevPrefix} The legal cards to play are: ", consoleSettings); ;
                            if (_config.DevMode) CustomConsole.PrintLegalCards(CurrentPlayer.Hand, consoleSettings);
                            CustomConsole.DevWriteLine(consoleSettings);

                            //if (!_config.DevMode && _config.HidePlayerHands) CurrentPlayer.IsPlayerReady();

                            CurrentPlayer.PlayerTurn();
                            UpdatePlayedCards();

                            _rules.ResetPlayableCards(CurrentPlayer.Hand);

                            if(_rules.IsCardBetter(CurrentPlayer.ChosenCard, WinningCard, LedCard)) SetWinner();

                            if (!_config.DevMode && _config.HidePlayerHands)
                            {
                                CustomConsole.Clear();
                                CustomConsole.PrintPlayedCards(PlayedCards);
                                CustomConsole.WriteLine();
                                CustomConsole.WriteLineNoDelay($"{TrumpCard.GetSuitSymbolUnicoded()} are trumps, {LedCard.GetSuitSymbolUnicoded()} were led.");
                            }
                            else
                            {
                                CustomConsole.WriteLine($"{CurrentPlayer.Name} played {CurrentPlayer.ChosenCard}.", consoleSettings);
                            }

                            CustomConsole.DevWriteLineNoDelay($"{CurrentPlayer.Name} now has {CurrentPlayer.Hand.Count} cards.", consoleSettings);
                            CustomConsole.DevWriteLine(consoleSettings);

                            CustomConsole.WriteLine($"{WinningPlayer.Name} is currently winning with the {WinningCard}.", consoleSettings);
                            CustomConsole.WriteLine();
                        }

                        break;

                    case GameState.Scoring:

                        _rules.Scoring(WinningPlayer);
                        CustomConsole.WriteLine($"{WinningPlayer.Name} wins the trick with the {WinningCard} and receives 5 points.", consoleSettings);
                        CustomConsole.WriteLine();
                        CustomConsole.PrintPlayersScores(_players, consoleSettings);

                        if (IsGameOver())
                        {
                            CustomConsole.WriteLine("Game Over!", consoleSettings);
                            CustomConsole.WriteLine($"{WinningPlayer.Name} wins.", consoleSettings);
                            CustomConsole.WriteLine();
                            ChangeGameState(GameState.PlayAgain);
                        }

                        else if (ArePlayersOutOfCards())
                        {
                            ChangeGameState(GameState.NewRound);
                        }
                        else
                        {
                            WinnerBecomesLeader();
                            CustomConsole.WriteLine($"{WinningPlayer.Name} leads the next trick.", consoleSettings);
                            CustomConsole.WriteLine();

                            ChangeGameState(GameState.LeadTurn);

                        }

                        break;

                    case GameState.NewRound:

                        RotateDealer();
                        NewDeck();

                        CustomConsole.DevWriteLineNoDelay($"New Deck has been created, it has {Deck.cards.Count} cards.", consoleSettings);

                        Deck.Shuffle();
                        CustomConsole.WriteLine($"Deck has been shuffled.", consoleSettings);

                        CustomConsole.WriteLine($"The dealer position has rotated clockwise to: {Dealer.Name}", consoleSettings);

                        ChangeGameState(GameState.DealCards);
                        break;

                    case GameState.PlayAgain:

                        CustomConsole.WriteLine("Would you like to play again? (Y/N)", consoleSettings);
                        var charResponse = Console.ReadLine();

                        if (charResponse == "y" || charResponse == "Y")
                        {
                            ChangeGameState(GameState.Initialize);
                            CustomConsole.Clear();
                            CustomConsole.WriteLine("You chose to play a new game.", consoleSettings);
                            CustomConsole.WriteLine();
                        }
                        else if (charResponse == "n" || charResponse == "N")
                        {
                            CustomConsole.WriteLine("You chose to not play a new game.", consoleSettings);
                            CustomConsole.WriteLine();
                            ChangeGameState(GameState.EndGame);
                        }
                        else
                        {
                            CustomConsole.WriteLine("Invalid response, try again.", consoleSettings);
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
