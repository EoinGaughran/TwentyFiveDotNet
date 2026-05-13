using System.Collections.Generic;
using Core.Models;
using TMPro;
using TwentyFiveDotNet.Core.Config;
using TwentyFiveDotNet.Core.Game;
using TwentyFiveDotNet.Core.Interfaces;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class GameUI : MonoBehaviour, IGameInteraction
{
    private GameManager _manager;

    public PlayerUI humanUI;
    public Transform opponentContainer;
    public GameObject opponentPrefab;
    public TextMeshProUGUI CurrentPhase;
    public RuntimeSettings _runtimeSettings;

    public Transform deckSlot;
    public GameObject deckPrefab;

    public Player human;

    [SerializeField] private Main main;

    private List<PlayerUI> opponentUIs = new();
    public void Init(GameManager manager, RuntimeSettings runtimeSettings)
    {
        Debug.Log("GameUI.Init called");

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
        Debug.Log("Snapshot received");
        CurrentPhase.text = "CurrentPhase: " + gameState.CurrentPhase.ToString();

        PlayerSetup(gameState.Players);
    }

    private void PlayerSetup(IReadOnlyList<Player> playerList)
    {
        // --- HUMAN ---
        human = playerList[0];
        humanUI.Bind(human);

        // --- CLEAR OLD OPPONENTS ---
        foreach (Transform child in opponentContainer)
        {
            Destroy(child.gameObject);
        }

        opponentUIs.Clear();

        // --- CREATE OPPONENTS ---
        for (int i = 1; i < playerList.Count; i++)
        {
            GameObject opponentGO = Instantiate(opponentPrefab, opponentContainer);

            PlayerUI ui = opponentGO.GetComponent<PlayerUI>();
            ui.Bind(playerList[i]);

            opponentUIs.Add(ui);
        }
    }

    private void HandlePhaseChange(GamePhase gamePhase)
    {
        CurrentPhase.text = "CurrenPhase: " + gamePhase.ToString();

        Debug.Log($"State changed: {gamePhase}");

        if (_runtimeSettings.TestStateMode)
        {
            main.SetAutoAdvance(false);
        }
    }

    public void PlayCardButtonPressed()
    {
        var card = humanUI.GetSelectedCard();

        if (card == null)
        {
            Debug.Log("PlayCardButtonPressed called. Card selected was null.");
            return;
        }

        Debug.Log($"PlayCardButtonPressed called. Card selected was {card}.");
        //humanUI.ha
    }

    public void HandleDealCompleted(GameState gameState)
    {
        PlayerSetup(gameState.Players);
        GameObject deckGO = Instantiate(deckPrefab, deckSlot);
        DeckUI deck = deckGO.GetComponent<DeckUI>();

        deck.Setup(gameState.Deck.Cards.Count);
    }

    private void HandleTrumpResolved(TrumpData trumpData, Player player)
    {
        Debug.Log($"Dealer {player.Name} flipped the trump card. It's the {trumpData._trumpCard}");

        foreach (var kvp in trumpData._trumpCards)
        {
            Debug.Log($"{kvp.Key} is worth: {kvp.Value}");
        }
    }

    private void HandleRolesSelected(Player dealer, Player leader)
    {
        Debug.Log($"{dealer} has been selected as the dealer.");
        Debug.Log($"{leader} is leading the trick.");
    }
    private void HandleCardDiscarded(Card discardedCard, Player cardPlayer)
    {
        Debug.Log($"{cardPlayer.Name} discarded a card. (It was a {discardedCard})");
    }
    private void HandlePlayerSteal(Card trumpCard, Player stealingPlayer)
    {
        Debug.Log($"{stealingPlayer.Name} stole the trump card {trumpCard}.");
    }
    private void HandlePlayerTurnStarted(Player player)
    {
        Debug.Log($"It's {player.Name}'s turn.");
    }
    private void HandleCardPlayed(CardPlayedEvent cardPlayedEvent)
    {
        if (cardPlayedEvent.IsLeader)
            Debug.Log($"{cardPlayedEvent.Player} led with the {cardPlayedEvent.PlayedCard}. Suit {cardPlayedEvent.PlayedCard.GetSuitSymbolUnicoded()} is leading.");
        else
            Debug.Log($"{cardPlayedEvent.Player.Name} played the {cardPlayedEvent.PlayedCard}");
    }

    void OnDestroy()
    {
        if (_manager != null)
        {
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
}