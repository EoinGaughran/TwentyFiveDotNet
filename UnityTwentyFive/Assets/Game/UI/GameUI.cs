using System.Collections.Generic;
using TwentyFiveDotNet.Core.Game;
using TwentyFiveDotNet.Core.Interfaces;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class GameUI : MonoBehaviour, IGameInteraction
{
    private GameManager _manager;

    public PlayerUI humanUI;
    public Transform opponentContainer;
    public GameObject opponentPrefab;

    private List<PlayerUI> opponentUIs = new();
    public void Init(GameManager manager)
    {
        Debug.Log("GameUI.Init called");

        _manager = manager;

        _manager.OnStateSnapshot += OnStateSnapshot;
    }

    public bool PlayAgain() => false;

    private void OnStateSnapshot(GameState gameState)
    {
        Debug.Log("Snapshot received");

        // --- HUMAN ---
        Player human = gameState.Players[0];
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

    void OnDestroy()
{
    if (_manager != null)
        _manager.OnStateSnapshot -= OnStateSnapshot;
}
}
