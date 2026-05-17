using System.Collections.Generic;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class PlayerPanelUI : MonoBehaviour
{
    [SerializeField] private PlayerUI humanUI;
    [SerializeField] private Transform opponentContainer;
    [SerializeField] private GameObject opponentPrefab;

    private readonly List<PlayerUI> opponentUIs = new();

    public Player Human { get; private set; }
    public PlayerUI HumanUI => humanUI;

    private readonly Dictionary<Player, PlayerUI> playerUIs = new();

    public void RenderPlayers(IReadOnlyList<Player> players)
    {
        playerUIs.Clear();

        humanUI.Bind(players[0]);
        playerUIs[players[0]] = humanUI;

        foreach (Transform child in opponentContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 1; i < players.Count; i++)
        {
            GameObject opponentGO = Instantiate(opponentPrefab, opponentContainer);
            PlayerUI ui = opponentGO.GetComponent<PlayerUI>();

            ui.Bind(players[i]);
            playerUIs[players[i]] = ui;
        }
    }

    public void RemoveCardFromPlayer(Player player, Card card)
    {
        if (!playerUIs.TryGetValue(player, out PlayerUI ui))
        {
            Debug.LogWarning($"No UI found for player {player.Name}");
            return;
        }

        ui.RemoveCardFromHand(card);
    }

    public void AddCardToPlayerHand(Player player, Card card)
    {
        if (!playerUIs.TryGetValue(player, out PlayerUI ui))
        {
            Debug.LogWarning($"No UI found for player {player.Name}");
            return;
        }

        ui.AddCardToHand(card);
    }

    public void RefreshPlayedCards(Player player)
    {
        if (!playerUIs.TryGetValue(player, out PlayerUI ui))
        {
            Debug.LogWarning($"No UI found for player {player.Name}");
            return;
        }

        ui.RenderPlayedCards();
    }

    public void PrintPlayersScores(IReadOnlyList<Player> players)
    {
        foreach (Player player in players)
        {
            if (!playerUIs.TryGetValue(player, out PlayerUI ui))
            {
                Debug.LogWarning($"No UI found for player {player.Name}");
                return;
            }

            ui.RenderText();
        }
    }
}