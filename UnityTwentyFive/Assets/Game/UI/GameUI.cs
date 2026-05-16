using System;
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

    [SerializeField] private Main main;
    [SerializeField] private PhaseUI phaseUI;
    [SerializeField] private PlayerPanelUI playerPanelUI;
    [SerializeField] private TablePanelUI tablePanelUI;
    [SerializeField] private ConsoleLogUI ConsoleLogUI;

    private PlayerDecisionType currentDecisionType;
    private IReadOnlyList<Card> currentOptions;

    public void Init(GameManager manager, RuntimeSettings runtimeSettings)
    {
        _manager = manager;
        _runtimeSettings = runtimeSettings;

        _manager.OnStateSnapshot += HandleStateSnapshot;
        _manager.OnGamePhaseChange += HandlePhaseChange;
        _manager.OnDealingCompleted += HandleDealCompleted;
        _manager.OnTrumpResolved += HandleTrumpResolved;
        _manager.OnRolesSelected += HandleRolesSelected;
        _manager.OnCardDiscarded += HandleCardDiscarded;
        _manager.OnPlayerSteal += HandlePlayerSteal;
        _manager.OnPlayerTurnStarted += HandlePlayerTurnStarted;
        _manager.OnCardPlayed += HandleCardPlayed;
        _manager.OnPlayerInputRequest += HandlePlayerInput;
    }

    public bool PlayAgain() => false;

    private void HandleStateSnapshot(GameState gameState)
    {
        phaseUI.Render(gameState.CurrentPhase);
        playerPanelUI.RenderPlayers(gameState.Players);
    }

    private void HandlePhaseChange(GamePhase gamePhase)
    {
        phaseUI.Render(gamePhase);

        if (_runtimeSettings.TestStateMode)
        {
            main.SetAutoAdvance(false);
        }

        ConsoleLogUI.AppendText($"GamePhase: {gamePhase}");
    }

    private void HandleDealCompleted(GameState gameState)
    {
        playerPanelUI.RenderPlayers(gameState.Players);
        tablePanelUI.RenderDeckCount(gameState.Deck.Cards.Count);

        ConsoleLogUI.AppendText($"{gameState.Players.Count} players added to game.");
        ConsoleLogUI.AppendText($"{gameState.Deck} created.");
        ConsoleLogUI.AppendText($"{gameState.Deck.Cards.Count} cards remain in {gameState.Deck}");
    }

    private void HandleTrumpResolved(TrumpData trumpData, Player player)
    {
        Debug.Log($"Dealer {player.Name} flipped the trump card. It's the {trumpData._trumpCard}");

        ConsoleLogUI.AppendText($"Dealer {player.Name} flipped the trump card. It's the {trumpData._trumpCard}");

        tablePanelUI.RenderStatusCard(trumpData._trumpCard, StatusCardType.TrumpCard);
    }

    private void HandleRolesSelected(Player dealer, Player leader)
    {
        Debug.Log($"{dealer} has been selected as the dealer.");
        Debug.Log($"{leader} is leading the trick.");

        ConsoleLogUI.AppendText($"{dealer} has been selected as the dealer.");
        ConsoleLogUI.AppendText($"{leader} is leading the trick.");
    }

    private void HandleCardDiscarded(Card discardedCard, Player cardPlayer)
    {
        Debug.Log($"{cardPlayer.Name} discarded {discardedCard}.");

        ConsoleLogUI.AppendText($"{cardPlayer.Name} discarded {discardedCard}.");

        playerPanelUI.RemoveCardFromPlayer(cardPlayer, discardedCard);
    }

    private void HandlePlayerSteal(Card trumpCard, Player stealingPlayer)
    {
        Debug.Log($"{stealingPlayer.Name} stole {trumpCard}.");

        ConsoleLogUI.AppendText($"{stealingPlayer.Name} stole {trumpCard}.");

        playerPanelUI.AddCardToPlayerHand(stealingPlayer, trumpCard);
    }

    private void HandlePlayerTurnStarted(Player player)
    {
        Debug.Log($"It's {player.Name}'s turn.");

        ConsoleLogUI.AppendText($"It's {player.Name}'s turn.");
    }

    private void HandleCardPlayed(CardPlayedEvent cardPlayedEvent)
    {
        if (cardPlayedEvent.IsLeader)
        {
            Debug.Log($"{cardPlayedEvent.Player} led with the {cardPlayedEvent.PlayedCard}.");

            ConsoleLogUI.AppendText($"{cardPlayedEvent.Player} led with the {cardPlayedEvent.PlayedCard}." +
                $"\n Suit {cardPlayedEvent.PlayedCard.GetSuitSymbolUnicoded()} is leading.");

            tablePanelUI.RenderStatusCard(cardPlayedEvent.PlayedCard, StatusCardType.LedCard);
        }
        else
        {
            Debug.Log($"{cardPlayedEvent.Player.Name} played the {cardPlayedEvent.PlayedCard}");
            ConsoleLogUI.AppendText($"{cardPlayedEvent.Player.Name} played {cardPlayedEvent.PlayedCard}");
        }

        playerPanelUI.RemoveCardFromPlayer(cardPlayedEvent.Player, cardPlayedEvent.PlayedCard);
        playerPanelUI.RefreshPlayedCards(cardPlayedEvent.Player);
    }

    private void HandlePlayerInput(
    Player player,
    PlayerDecisionType decisionType,
    IReadOnlyList<Card> options)
    {
        ConsoleLogUI.AppendText($"PlayerDecisionType: {decisionType}");

        currentDecisionType = decisionType;
        currentOptions = options;

        switch (decisionType)
        {
            case PlayerDecisionType.FlipTrump:
                ConsoleLogUI.AppendText($"{player.Name}, please flip over the trump card.");
                tablePanelUI.AllowTrumpFlip();
                break;

            case PlayerDecisionType.LeadCard:
                ConsoleLogUI.AppendText($"{player.Name}, please lead a card.");
                playerPanelUI.HumanUI.AllowCardPlay(player);
                break;

            case PlayerDecisionType.StealTrump:
                ConsoleLogUI.AppendText($"{player.Name}, please discard a card to steal the trump card.");
                playerPanelUI.HumanUI.AllowCardPlay(player);
                break;

            case PlayerDecisionType.PlayCard:
                if (options == null)
                    throw new InvalidOperationException("Options was null");

                ConsoleLogUI.AppendText($"{player.Name}, please play a card.");
                playerPanelUI.HumanUI.ShowPlayableCards(options);
                playerPanelUI.HumanUI.AllowCardPlay(player);
                break;
        }
    }

    public void SubmitSelectedCard()
    {
        var card = playerPanelUI.HumanUI.GetSelectedCard();

        if (card == null)
        {
            Debug.Log("No card selected.");
            return;
        } 

        Debug.Log($"Selected card was {card}.");

        _manager.SubmitPlayerAction(card);

        currentDecisionType = default;
        currentOptions = null;

        playerPanelUI.HumanUI.ResetPlayableCards();
    }

    public void SubmitFlipTrump()
    {
        _manager.SubmitPlayerAction(null);

        Debug.Log($"Trump Flip Submitted.");

        currentDecisionType = default;
        currentOptions = null;
    }

    private void OnDestroy()
    {
        if (_manager == null) return;

        _manager.OnStateSnapshot -= HandleStateSnapshot;
        _manager.OnGamePhaseChange -= HandlePhaseChange;
        _manager.OnDealingCompleted -= HandleDealCompleted;
        _manager.OnTrumpResolved -= HandleTrumpResolved;
        _manager.OnRolesSelected -= HandleRolesSelected;
        _manager.OnCardDiscarded -= HandleCardDiscarded;
        _manager.OnPlayerSteal -= HandlePlayerSteal;
        _manager.OnPlayerTurnStarted -= HandlePlayerTurnStarted;
        _manager.OnCardPlayed -= HandleCardPlayed;
        _manager.OnPlayerInputRequest -= HandlePlayerInput;
    }
}