using System.Collections.Generic;
using TwentyFiveDotNet.Core.Game;
using TwentyFiveDotNet.Core.Interfaces;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class GameUI : MonoBehaviour, IGameInteraction
{
    private GameManager _manager;

    private Dictionary<int, PlayerUI> _players = new();

    [SerializeField] private GameObject playerPrefab;
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

        foreach (var player in gameState.Players)
        {
            if (!_players.ContainsKey(player.Id))
            {
                CreatePlayerUI(player);
            }

            _players[player.Id].UpdateFrom(player);
        }
    }

    private void CreatePlayerUI(Player player)
    {
        Debug.Log($"Creating UI for player {player.Id}");

        var obj = Instantiate(playerPrefab);
        var ui = obj.GetComponent<PlayerUI>();

        ui.Init(player.Id);

        _players[player.Id] = ui;
    }

    void OnDestroy()
{
    if (_manager != null)
        _manager.OnStateSnapshot -= OnStateSnapshot;
}
}
