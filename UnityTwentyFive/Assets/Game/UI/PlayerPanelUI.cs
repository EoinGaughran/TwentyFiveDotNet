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

    private readonly Dictionary<int, PlayerUI> playerUIs = new();

    public void RenderPlayers(IReadOnlyList<Player> players)
    {
        playerUIs.Clear();

        humanUI.Bind(players[0]);
        playerUIs[players[0].Id] = humanUI;

        foreach (Transform child in opponentContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 1; i < players.Count; i++)
        {
            GameObject opponentGO = Instantiate(opponentPrefab, opponentContainer);
            PlayerUI ui = opponentGO.GetComponent<PlayerUI>();

            ui.Bind(players[i]);

            playerUIs[players[i].Id] = ui;
        }
    }

    public void AllowCardPlay()
    {
        humanUI.AllowCardPlay();
    }

    public void RemoveCardFromPlayer(int playerID, int cardID)
    {
        if (!playerUIs.TryGetValue(playerID, out PlayerUI ui))
        {
            Debug.LogWarning($"No UI found for player ID {playerID}");
            return;
        }

        ui.RemoveCardFromHand(cardID);
    }

    public void AddCardToPlayerHand(int playerID, CardUI cardUI)
    {
        PlayerUI ui = GetPlayerUI(playerID);

        ui.AddCardToHand(cardUI);
    }
    public PlayerUI GetPlayerUI(int playerID)
    {
        if (!playerUIs.TryGetValue(playerID, out PlayerUI ui))
        {
            Debug.LogError($"No UI found for player ID {playerID}");

            throw new KeyNotFoundException(
                $"No PlayerUI exists for player ID {playerID}");
        }

        return ui;
    }

    public void MoveCardToPlayedCards(int playerID, CardUI card)
    {
        PlayerUI ui = GetPlayerUI(playerID);

        ui.AddCardToPlayedCards(card);
    }

    public void PrintPlayersScores(IReadOnlyList<PlayerScoreViewData> players)
    {
        foreach (var player in players)
        {
            PlayerUI ui = GetPlayerUI(player.PlayerId);

            ui.RenderText(player.PlayerName, player.Score);
        }
    }
}