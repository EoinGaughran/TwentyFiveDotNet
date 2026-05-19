using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Models;
using TwentyFiveDotNet.Core.Config;
using TwentyFiveDotNet.Core.Game;
using TwentyFiveDotNet.Core.Interfaces;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameUI : MonoBehaviour, IGameInteraction
{
    private GameManager _manager;
    private RuntimeSettings _runtimeSettings;

    [SerializeField] private Main _main;
    [SerializeField] private PhaseUI _phaseUI;
    [SerializeField] private PlayerPanelUI _playerPanelUI;
    [SerializeField] private TablePanelUI _tablePanelUI;
    [SerializeField] private ConsoleLogUI _consoleLogUI;
    [SerializeField] private CardUIFactory _cardUIFactory;

    [SerializeField] private float eventDelaySeconds = 1f;

    private readonly Queue<Action> uiQueue = new();
    private bool isPlayingQueue;

    private PlayerDecisionType currentDecisionType;
    private IReadOnlyList<int> currentOptions;

    public void Init(GameManager manager, RuntimeSettings runtimeSettings)
    {
        _manager = manager;
        _runtimeSettings = runtimeSettings;

        _manager.OnStateSnapshot += HandleStateSnapshot;
        _manager.OnDealingCompleted += HandleDealCompleted;
        _manager.OnTrumpResolved += HandleTrumpResolved;
        _manager.OnRolesSelected += HandleRolesSelected;
        _manager.OnCardDiscarded += HandleCardDiscarded;
        _manager.OnPlayerSteal += HandlePlayerSteal;
        _manager.OnPlayerTurnStarted += HandlePlayerTurnStarted;
        _manager.OnCardPlayed += HandleCardPlayed;
        _manager.OnPlayerInputRequest += HandlePlayerInput;
        _manager.OnScoreChanged += HandleScoreChanged;
        _manager.OnTrickNewWinner += HandleTrickNewWinner;
        _manager.OnTrickScored += HandleTrickScored;
        _manager.OnNewTrick += HandleNewTrick;
        _manager.OnHandEnd += HandleHandEnd;
        _manager.OnGamePhaseChange += HandleGamePhaseChange;
        _manager.OnGameOver += HandleGameOver;
        _manager.OnNewGame += HandleNewGame;
        _manager.OnProgramClosed += HandleProgramClosed;
    }

    public bool PlayAgain() => false;

    private void HandleStateSnapshot(GameState gS)
    {
        var gamePhase = gS.CurrentPhase.ToString();
        var players = gS.Players.ToList();

        EnqueueUI(() =>
        {
            _phaseUI.Render(gamePhase);
            _playerPanelUI.RenderPlayers(players);
        });
    }

    private void HandleDealCompleted(GameState gameState)
    {
        var players = gameState.Players.ToList();

        var deckCount = gameState.Deck.Cards.Count;
        var playerCount = players.Count;
        var deckName = gameState.Deck.ToString();

        EnqueueUI(() =>
        {
            _playerPanelUI.RenderPlayers(players);

            foreach (var player in players)
            {
                PlayerUI playerUI = _playerPanelUI.GetPlayerUI(player.Id);

                foreach (var card in player.Hand)
                {
                    CardUI cardUI =
                        _cardUIFactory.CreateCardUI(card);

                    playerUI.AddCardToHand(cardUI);
                }
            }

            _tablePanelUI.RenderDeckCount(deckCount);

            _consoleLogUI.AppendText(
                $"{playerCount} players added to game." +
                $"\n{deckName} created." +
                $"\n{deckCount} cards remain in {deckName}");
        });
    }

    private void HandleTrumpResolved(TrumpData trumpData, Player player, Deck deck)
    {
        var playerName = player.Name;
        var trumpCard = trumpData._trumpCard;
        var deckCardCount = deck.Cards.Count;
        var deckName = deck.ToString();

        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"Dealer {playerName} flipped the trump card. It's the {trumpCard}." +
                $"\n{deckCardCount} cards remain in {deckName}.");

            //_tablePanelUI.RenderStatusCard(trumpCard, StatusCardType.TrumpCard);

            CardUI cardUI = _cardUIFactory.CreateCardUI(trumpCard);
            _tablePanelUI.AddCardToStatusSlot(cardUI, StatusCardType.TrumpCard);

            _tablePanelUI.RenderDeckCount(deckCardCount);
        });
    }

    private void HandleRolesSelected(Player dealer, Player leader)
    {
        var dealerName = dealer.Name;
        var leaderName = leader.Name;

        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"{dealerName} has been selected as the dealer." +
                $"\n{leaderName} is leading the trick.");
        });
    }

    private void HandleCardDiscarded(Card discardedCard, Player cardPlayer)
    {
        var cardPlayerName = cardPlayer.Name;
        var cardPlayerID = cardPlayer.Id;
        var discardedCardValue = discardedCard.ToString();
        var discardedCardID = discardedCard.Id;

        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"{cardPlayerName} discarded {discardedCardValue}.");

            _playerPanelUI.RemoveCardFromPlayer(cardPlayerID, discardedCardID);
        });
    }

    private void HandlePlayerSteal(Card trumpCard, Player stealingPlayer)
    {
        var stealingPlayerName = stealingPlayer.Name;
        var stealingPlayerID = stealingPlayer.Id;
        var trumpCardName = trumpCard.ToString();
        var trumpCardID = trumpCard.Id;

        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"{stealingPlayerName} stole {trumpCardName}.");

            if (_cardUIFactory.TryGetCardUI(trumpCardID, out var trumpCardUI))
            {
                _playerPanelUI.AddCardToPlayerHand(stealingPlayerID, trumpCardUI);
            }
            else
            {
                Debug.LogError($"No CardUI found for card ID {trumpCardID}");
            }

        });
    }

    private void HandlePlayerTurnStarted(Player player)
    {
        var playerName = player.Name;

        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"It's {playerName}'s turn.");
        });
    }

    private void HandleCardPlayed(CardPlayedEvent e)
    {
        var playerName = e.Player.Name;
        var playerID = e.Player.Id;
        var playedCardName = e.PlayedCard.ToString();
        var playedCardID = e.PlayedCard.Id;
        var ledSuit = e.PlayedCard.GetSuitSymbolUnicoded();
        var isLeader = e.IsLeader;
        var playedCard = e.PlayedCard;
        var playerScore = e.Player.Points;

        EnqueueUI(() =>
        {
            if (isLeader)
            {
                _consoleLogUI.AppendText($"{playerName} led with the {playedCardName}." +
                    $"\n Suit {ledSuit} is leading.");

                _tablePanelUI.RenderStatusCard(playedCard, StatusCardType.LedCard);
            }
            else
            {
                _consoleLogUI.AppendText($"{playerName} played {playedCard}");
            }

            _playerPanelUI.RemoveCardFromPlayer(playerID, playedCardID);

            if (_cardUIFactory.TryGetCardUI(playedCardID, out var playedCardUI))
            {
                _playerPanelUI.MoveCardToPlayedCards(playerID, playedCardUI);
            }
            else
            {
                Debug.LogError($"No CardUI found for card ID {playedCardUI}");
            }
        });
    }

    private void HandlePlayerInput(
    Player p,
    PlayerDecisionType dT,
    IReadOnlyList<Card> o)
    {
        var playerName = p.Name;
        var decisionTypeName = dT.ToString();
        var decisionType = dT;
        var options = o?.Select(c => c.Id).ToList();


        EnqueueUI(() =>
        {
            _playerPanelUI.HumanUI.AllowCardPlay();

            _consoleLogUI.AppendText($"PlayerDecisionType: {decisionTypeName}");

            currentDecisionType = decisionType;
            currentOptions = options;

            switch (decisionType)
            {
                case PlayerDecisionType.FlipTrump:
                    _consoleLogUI.AppendText($"{playerName}, please flip over the trump card.");
                    _tablePanelUI.AllowTrumpFlip();
                    break;

                case PlayerDecisionType.LeadCard:
                    _consoleLogUI.AppendText($"{playerName}, please lead a card.");
                    _playerPanelUI.HumanUI.AllowCardPlay();
                    break;

                case PlayerDecisionType.StealTrump:
                    _consoleLogUI.AppendText($"{playerName}, please discard a card to steal the trump card.");
                    _playerPanelUI.HumanUI.AllowCardPlay();
                    break;

                case PlayerDecisionType.PlayCard:
                    if (options == null)
                        throw new InvalidOperationException("Options was null");

                    _consoleLogUI.AppendText($"{playerName}, please play a card.");
                    _playerPanelUI.HumanUI.ShowPlayableCards(options);
                    _playerPanelUI.HumanUI.AllowCardPlay();
                    break;
            }
        });
    }

    public void SubmitSelectedCard()
    {
        if (_playerPanelUI.HumanUI.CanHumanPlayCards())
        {
            var cardID = _playerPanelUI.HumanUI.GetSelectedCardID();

            if (cardID == null)
            {
                _consoleLogUI.AppendText("No card selected.");
                return;
            }

            _consoleLogUI.AppendText($"Selected card was {cardID}.");

            _manager.SubmitPlayerAction(cardID);

            currentDecisionType = default;
            currentOptions = null;

            _playerPanelUI.HumanUI.ResetPlayableCards();
        }
        else
        {
            Debug.Log("You cannot play cards, its not your turn.");
        }
    }

    public void SubmitFlipTrump()
    {
        _manager.SubmitPlayerAction(null);
            
        _consoleLogUI.AppendText($"Trump Flip Submitted.");
            
        currentDecisionType = default;
        currentOptions = null;
    }

    //Scoring
    private void HandleScoreChanged(IReadOnlyList<Player> players)
    {
        var scoreData = players
        .Select(p => new PlayerScoreViewData(
            p.Id,
            p.Name,
            p.Points))
        .ToList();

        EnqueueUI(() =>
        {
            _playerPanelUI.PrintPlayersScores(scoreData);
        });
        
    }

    private void HandleTrickNewWinner(Card winningCard, Player player, bool isDealer)
    {
        var playerName = player.Name;
        var winningCardName = winningCard.ToString();

        EnqueueUI(() =>
        {
            if (isDealer)
            {
                _consoleLogUI.AppendText($"{playerName} got their dealer's trick.");
            }
            
            _consoleLogUI.AppendText($"{playerName} is currently winning with the {winningCardName}.");
            
            _tablePanelUI.RenderStatusCard(winningCard, StatusCardType.WinningCard);
        });

    }

    private void HandleTrickScored(Card winningCard, Player player)
    {
        var playerName = player.Name;
        var winningCardName = winningCard.ToString();

        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"{playerName} won with the {winningCardName}" +
            $"\n{playerName} has received the trick worth 5 points.");
        });
        
    }

    private void HandleNewTrick(Player leader, int trickNumber)
    {
        var leaderName = leader.Name;
        var _trickNumber = trickNumber;

        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"Trick {_trickNumber} begins." +
                $"\n{leaderName} will lead the trick.");

            _tablePanelUI.DestroyStatusCard(StatusCardType.WinningCard);
            _tablePanelUI.DestroyStatusCard(StatusCardType.LedCard);

        });
    }

    private void HandleHandEnd()
    {
        EnqueueUI(() =>
        {
            _cardUIFactory.ClearAllCardUIs();
        });
    }

    //Game State 
    private void HandleGamePhaseChange(GamePhase gamePhase)
    {
        var _gamePhase = gamePhase.ToString();

        EnqueueUI(() =>
        {
            _phaseUI.Render(_gamePhase);

            if (_runtimeSettings.TestStateMode)
            {
                _main.SetAutoAdvance(false);
            }

            _consoleLogUI.AppendText($"GamePhase: {_gamePhase}");
        });
    }

    private void HandleGameOver(Player winner)
    {
        var playerName = winner.Name;

        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"{playerName} wins!" +
                $"\nGame Over!");
        });
    }

    private void HandleNewGame()
    {
        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"The deck has been removed." +
                $"\nPlayers hands have been cleared." +
                $"\nPlayers scores have been cleared." +
                $"\nThe list of played cards has been cleared.");
        });
    }

    private void HandleProgramClosed()
    {
        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"The game has ended. The program will now close.");
        });
    }

    private void EnqueueUI(Action action)
    {
        uiQueue.Enqueue(action);

        if (!isPlayingQueue)
            StartCoroutine(PlayUIQueue());
    }

    private IEnumerator PlayUIQueue()
    {
        isPlayingQueue = true;

        while (uiQueue.Count > 0)
        {
            var action = uiQueue.Dequeue();
            action.Invoke();

            yield return new WaitForSeconds(eventDelaySeconds);
        }

        isPlayingQueue = false;
    }

    private void OnDestroy()
    {
        if (_manager == null) return;

        _manager.OnStateSnapshot -= HandleStateSnapshot;
        _manager.OnDealingCompleted -= HandleDealCompleted;
        _manager.OnTrumpResolved -= HandleTrumpResolved;
        _manager.OnRolesSelected -= HandleRolesSelected;
        _manager.OnCardDiscarded -= HandleCardDiscarded;
        _manager.OnPlayerSteal -= HandlePlayerSteal;
        _manager.OnPlayerTurnStarted -= HandlePlayerTurnStarted;
        _manager.OnCardPlayed -= HandleCardPlayed;
        _manager.OnPlayerInputRequest -= HandlePlayerInput;
        _manager.OnScoreChanged -= HandleScoreChanged;
        _manager.OnTrickNewWinner -= HandleTrickNewWinner;
        _manager.OnTrickScored -= HandleTrickScored;
        _manager.OnNewTrick -= HandleNewTrick;
        _manager.OnHandEnd -= HandleHandEnd;
        _manager.OnGamePhaseChange -= HandleGamePhaseChange;
        _manager.OnGameOver -= HandleGameOver;
        _manager.OnNewGame -= HandleNewGame;
        _manager.OnProgramClosed -= HandleProgramClosed;
    }
}