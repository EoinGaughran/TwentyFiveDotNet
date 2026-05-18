using System;
using System.Collections;
using System.Collections.Generic;
using Core.Models;
using TwentyFiveDotNet.Core.Config;
using TwentyFiveDotNet.Core.Game;
using TwentyFiveDotNet.Core.Interfaces;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class GameUI : MonoBehaviour, IGameInteraction
{
    private GameManager _manager;
    private RuntimeSettings _runtimeSettings;

    [SerializeField] private Main _main;
    [SerializeField] private PhaseUI _phaseUI;
    [SerializeField] private PlayerPanelUI _playerPanelUI;
    [SerializeField] private TablePanelUI _tablePanelUI;
    [SerializeField] private ConsoleLogUI _consoleLogUI;

    [SerializeField] private float eventDelaySeconds = 1f;

    private readonly Queue<Action> uiQueue = new();
    private bool isPlayingQueue;

    private PlayerDecisionType currentDecisionType;
    private IReadOnlyList<Card> currentOptions;

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
        _manager.OnGamePhaseChange += HandleGamePhaseChange;
        _manager.OnGameOver += HandleGameOver;
        _manager.OnNewGame += HandleNewGame;
        _manager.OnProgramClosed += HandleProgramClosed;
    }

    public bool PlayAgain() => false;

    private void HandleStateSnapshot(GameState gameState)
    {
        EnqueueUI(() =>
        {
            _phaseUI.Render(gameState.CurrentPhase);
            _playerPanelUI.RenderPlayers(gameState.Players);
        });
    }

    private void HandleDealCompleted(GameState gameState)
    {
        EnqueueUI(() =>
        {
            _playerPanelUI.RenderPlayers(gameState.Players);
            _tablePanelUI.RenderDeckCount(gameState.Deck.Cards.Count);

            _consoleLogUI.AppendText($"{gameState.Players.Count} players added to game." +
                $"\n{gameState.Deck} created." +
                $"\n{gameState.Deck.Cards.Count} cards remain in {gameState.Deck}");
        });
    }

    private void HandleTrumpResolved(TrumpData trumpData, Player player, Deck deck)
    {
        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"Dealer {player.Name} flipped the trump card. It's the {trumpData._trumpCard}");

            _tablePanelUI.RenderStatusCard(trumpData._trumpCard, StatusCardType.TrumpCard);

            _tablePanelUI.RenderDeckCount(deck.Cards.Count);
        });
    }

    private void HandleRolesSelected(Player dealer, Player leader)
    {
        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"{dealer} has been selected as the dealer." +
                $"\n{leader} is leading the trick.");
        });
    }

    private void HandleCardDiscarded(Card discardedCard, Player cardPlayer)
    {
        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"{cardPlayer.Name} discarded {discardedCard}.");

            _playerPanelUI.RemoveCardFromPlayer(cardPlayer, discardedCard);
        });
    }

    private void HandlePlayerSteal(Card trumpCard, Player stealingPlayer)
    {
        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"{stealingPlayer.Name} stole {trumpCard}.");

            _playerPanelUI.AddCardToPlayerHand(stealingPlayer, trumpCard);
        });
    }

    private void HandlePlayerTurnStarted(Player player)
    {
        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"It's {player.Name}'s turn.");
        });
    }

    private void HandleCardPlayed(CardPlayedEvent cardPlayedEvent)
    {
        EnqueueUI(() =>
        {
            if (cardPlayedEvent.IsLeader)
            {
                _consoleLogUI.AppendText($"{cardPlayedEvent.Player} led with the {cardPlayedEvent.PlayedCard}." +
                    $"\n Suit {cardPlayedEvent.PlayedCard.GetSuitSymbolUnicoded()} is leading.");

                _tablePanelUI.RenderStatusCard(cardPlayedEvent.PlayedCard, StatusCardType.LedCard);
            }
            else
            {
                _consoleLogUI.AppendText($"{cardPlayedEvent.Player.Name} played {cardPlayedEvent.PlayedCard}");
            }

            _playerPanelUI.RemoveCardFromPlayer(cardPlayedEvent.Player, cardPlayedEvent.PlayedCard);
            _playerPanelUI.RefreshPlayedCards(cardPlayedEvent.Player);
        });
    }

    private void HandlePlayerInput(
    Player player,
    PlayerDecisionType decisionType,
    IReadOnlyList<Card> options)
    {
        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"PlayerDecisionType: {decisionType}");

            currentDecisionType = decisionType;
            currentOptions = options;

            switch (decisionType)
            {
                case PlayerDecisionType.FlipTrump:
                    _consoleLogUI.AppendText($"{player.Name}, please flip over the trump card.");
                    _tablePanelUI.AllowTrumpFlip();
                    break;

                case PlayerDecisionType.LeadCard:
                    _consoleLogUI.AppendText($"{player.Name}, please lead a card.");
                    _playerPanelUI.HumanUI.AllowCardPlay(player);
                    break;

                case PlayerDecisionType.StealTrump:
                    _consoleLogUI.AppendText($"{player.Name}, please discard a card to steal the trump card.");
                    _playerPanelUI.HumanUI.AllowCardPlay(player);
                    break;

                case PlayerDecisionType.PlayCard:
                    if (options == null)
                        throw new InvalidOperationException("Options was null");

                    _consoleLogUI.AppendText($"{player.Name}, please play a card.");
                    _playerPanelUI.HumanUI.ShowPlayableCards(options);
                    _playerPanelUI.HumanUI.AllowCardPlay(player);
                    break;
            }
        });
    }

    public void SubmitSelectedCard()
    {
        EnqueueUI(() =>
        {
            var card = _playerPanelUI.HumanUI.GetSelectedCard();

            if (card == null)
            {
                _consoleLogUI.AppendText("No card selected.");
                return;
            }

            _consoleLogUI.AppendText($"Selected card was {card}.");

            _manager.SubmitPlayerAction(card);

            currentDecisionType = default;
            currentOptions = null;

            _playerPanelUI.HumanUI.ResetPlayableCards();
        });
    }

    public void SubmitFlipTrump()
    {
        EnqueueUI(() =>
        {
            _manager.SubmitPlayerAction(null);
            
            _consoleLogUI.AppendText($"Trump Flip Submitted.");
            
            currentDecisionType = default;
            currentOptions = null;
        });
        
    }

    //Scoring
    private void HandleScoreChanged(IReadOnlyList<Player> players)
    {
        EnqueueUI(() =>
        {
            _playerPanelUI.PrintPlayersScores(players);
        });
        
    }

    private void HandleTrickNewWinner(Card winningCard, Player player, bool isDealer)
    {
        EnqueueUI(() =>
        {
            if (isDealer)
            {
                _consoleLogUI.AppendText($"{player.Name} got their dealer's trick.");
            }
            
            _consoleLogUI.AppendText($"{player.Name} is currently winning with the {winningCard}.");
            
            _tablePanelUI.RenderStatusCard(winningCard, StatusCardType.WinningCard);
        });

    }

    private void HandleTrickScored(Card winningCard, Player player)
    {
        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"{player.Name} won with the {winningCard}" +
            $"\n{player.Name} has received the trick worth 5 points.");
        });
        
    }

    private void HandleNewTrick(Player leader, int trickNumber)
    {
        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"Trick {trickNumber} begins." +
                $"\n{leader} will lead the trick.");

            _tablePanelUI.DestroyStatusCard(StatusCardType.WinningCard);
            _tablePanelUI.DestroyStatusCard(StatusCardType.LedCard);

        });
    }

    //Game State 
    private void HandleGamePhaseChange(GamePhase gamePhase)
    {
        EnqueueUI(() =>
        {
            _phaseUI.Render(gamePhase);

            if (_runtimeSettings.TestStateMode)
            {
                _main.SetAutoAdvance(false);
            }

            _consoleLogUI.AppendText($"GamePhase: {gamePhase}");
        });
    }

    private void HandleGameOver(Player winner)
    {
        EnqueueUI(() =>
        {
            _consoleLogUI.AppendText($"{winner} wins!" +
                $"\nGame Over!");
        });
    }

    private void HandleNewGame()
    {
        _consoleLogUI.AppendText($"The deck has been removed." +
            $"\nPlayers hands have been cleared." +
            $"\nPlayers scores have been cleared." +
            $"\nThe list of played cards has been cleared.");
    }

    private void HandleProgramClosed()
    {
        _consoleLogUI.AppendText($"The game has ended. The program will now close.");
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
        _manager.OnGamePhaseChange -= HandleGamePhaseChange;
        _manager.OnGameOver -= HandleGameOver;
        _manager.OnNewGame -= HandleNewGame;
        _manager.OnProgramClosed -= HandleProgramClosed;
    }
}