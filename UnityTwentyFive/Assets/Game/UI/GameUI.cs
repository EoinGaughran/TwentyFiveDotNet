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
    [SerializeField] private DeckPanelUI deckPanelUI;
    [SerializeField] private TrumpPanelUI trumpPanelUI;
    [SerializeField] private ConsoleLogUI ConsoleLogUI;

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
    }

    public void PlayCardButtonPressed()
    {
        var card = playerPanelUI.HumanUI.GetSelectedCard();

        if (card == null)
        {
            Debug.Log("No card selected.");
            return;
        }

        Debug.Log($"Selected card was {card}.");
    }

    private void HandleDealCompleted(GameState gameState)
    {
        playerPanelUI.RenderPlayers(gameState.Players);
        deckPanelUI.RenderDeckCount(gameState.Deck.Cards.Count);

        ConsoleLogUI.AppendText($"{gameState.Players.Count} players added to game.");
        ConsoleLogUI.AppendText($"{gameState.Deck} created.");
        ConsoleLogUI.AppendText($"{gameState.Deck.Cards.Count} cards remain in {gameState.Deck}");
    }

    private void HandleTrumpResolved(TrumpData trumpData, Player player)
    {
        Debug.Log($"Dealer {player.Name} flipped the trump card. It's the {trumpData._trumpCard}");

        ConsoleLogUI.AppendText($"Dealer {player.Name} flipped the trump card. It's the {trumpData._trumpCard}");

        trumpPanelUI.RenderTrump(trumpData._trumpCard);
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

        playerPanelUI.RemoveCardFromPlayer(cardPlayer,discardedCard);
    }

    private void HandlePlayerSteal(Card trumpCard, Player stealingPlayer)
    {
        Debug.Log($"{stealingPlayer.Name} stole {trumpCard}.");

        ConsoleLogUI.AppendText($"{stealingPlayer.Name} stole {trumpCard}.");
    }

    private void HandlePlayerTurnStarted(Player player)
    {
        Debug.Log($"It's {player.Name}'s turn.");

        ConsoleLogUI.AppendText($"It's {player.Name}'s turn.");
    }

    private void HandleCardPlayed(CardPlayedEvent cardPlayedEvent)
    {
        Debug.Log($"{cardPlayedEvent.Player.Name} played {cardPlayedEvent.PlayedCard}");

        ConsoleLogUI.AppendText($"{cardPlayedEvent.Player.Name} played {cardPlayedEvent.PlayedCard}");
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
    }
}