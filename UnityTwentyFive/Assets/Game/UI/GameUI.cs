using System.Collections.Generic;
using TMPro;
using TwentyFiveDotNet.Core.Config;
using TwentyFiveDotNet.Core.Game;
using TwentyFiveDotNet.Core.Interfaces;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour, IGameInteraction
{
    private GameManager _manager;

    public PlayerUI humanUI;
    public Transform opponentContainer;
    public GameObject opponentPrefab;
    public TextMeshProUGUI CurrentPhase;
    public RuntimeSettings _runtimeSettings;

    public Player human;

    [SerializeReference] public Button PlayCard;

    [SerializeField] private Main main;


    private List<PlayerUI> opponentUIs = new();
    public void Init(GameManager manager, RuntimeSettings runtimeSettings)
    {
        Debug.Log("GameUI.Init called");

        _manager = manager;
        _runtimeSettings = runtimeSettings;

        _manager.OnStateSnapshot += OnStateSnapshot;
        _manager.OnGamePhaseChange += OnPhaseChange;
    }

    public bool PlayAgain() => false;

    private void OnStateSnapshot(GameState gameState)
    {
        Debug.Log("Snapshot received");
        CurrentPhase.text = "CurrentPhase: " + gameState.CurrentPhase.ToString();

        // --- HUMAN ---
        human = gameState.Players[0];
        humanUI.Bind(human);

        // --- CLEAR OLD OPPONENTS ---
        foreach (Transform child in opponentContainer)
        {
            Destroy(child.gameObject);
        }

        opponentUIs.Clear();

        // --- CREATE OPPONENTS ---
        for (int i = 1; i < gameState.Players.Count; i++)
        {
            GameObject opponentGO = Instantiate(opponentPrefab, opponentContainer);

            PlayerUI ui = opponentGO.GetComponent<PlayerUI>();
            ui.Bind(gameState.Players[i]);

            opponentUIs.Add(ui);
        }
    }

    private void OnPhaseChange(GamePhase gamePhase)
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

    void OnDestroy()
{
    if (_manager != null)
        _manager.OnStateSnapshot -= OnStateSnapshot;
        _manager.OnGamePhaseChange -= OnPhaseChange;
    }
}
