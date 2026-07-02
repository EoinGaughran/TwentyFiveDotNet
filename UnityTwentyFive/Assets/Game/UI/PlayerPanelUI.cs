using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;
using UnityEngine.UI;

public struct DealPacket
{
    public int PlayerId;
    public int CardCount;

    public DealPacket(int playerId, int cardCount)
    {
        PlayerId = playerId;
        CardCount = cardCount;
    }
}
public class PlayerPanelUI : MonoBehaviour
{
    [SerializeField] private PlayerUI humanUI;
    [SerializeField] private Transform opponentContainer;
    [SerializeField] private GameObject opponentPrefab;
    [SerializeField] private CardAnimator _animateCardManager;

    private readonly List<PlayerUI> opponentUIs = new();

    public Player Human { get; private set; }
    public PlayerUI HumanUI => humanUI;

    private readonly Dictionary<int, PlayerUI> playerUIs = new();

    public event Action<string> OnNotification;
    private void RegisterPlayerUI(PlayerUI playerUI)
    {
        playerUI.OnNotification += HandlePlayerNotification;
    }
    public void RenderPlayers(IReadOnlyList<Player> players)
    {
        playerUIs.Clear();

        foreach (Transform child in opponentContainer)
        {
            Destroy(child.gameObject);
        }

        Player humanPlayer = players.FirstOrDefault(p => p is PlayerHuman);

        if (humanPlayer != null)
        {
            Human = humanPlayer;
            humanUI.gameObject.SetActive(true);
            humanUI.Bind(humanPlayer);
            playerUIs[humanPlayer.Id] = humanUI;
            RegisterPlayerUI(humanUI);
        }
        else
        {
            Human = null;
            humanUI.gameObject.SetActive(false);
        }

        foreach (Player player in players)
        {
            if (player == humanPlayer)
                continue;

            GameObject opponentGO = Instantiate(opponentPrefab, opponentContainer);
            PlayerUI ui = opponentGO.GetComponent<PlayerUI>();

            ui.Bind(player);
            playerUIs[player.Id] = ui;
        }
    }

    private void HandlePlayerNotification(string message)
    {
        OnNotification?.Invoke(message);
    }

    public void AllowCardPlay()
    {
        humanUI.AllowCardPlay();
    }

    public bool RemoveCardFromPlayer(int playerID, int cardID)
    {
        if (!playerUIs.TryGetValue(playerID, out PlayerUI ui))
        {
            Debug.LogWarning($"No UI found for player ID {playerID}");
            return false;
        }

        if(ui.RemoveCardFromHand(cardID))
            return true;

        return false;
    }

    public void AddCardToPlayerHand(int playerID, CardUI cardUI)
    {
        PlayerUI ui = GetPlayerUI(playerID);

        if(playerID != 0)
            cardUI.SetCardSize(CardSize.Small);

        ui.SetUpCardInHand(cardUI);
        //cardUI.AnimateTo
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

    public bool MoveCardToPlayedCards(int playerID, CardUI cardUI)
    {
        PlayerUI ui = GetPlayerUI(playerID);

        if(!ui.AddCardToPlayedCards(cardUI))
            return false;

        int cardID = cardUI.GetCardID();

        if(!ui.RemoveCardFromHand(cardID))
            return false;

        return true;
    }

    public IEnumerator AnimateDealToPlayers(
    List<DealtPlayerCardUISnapshot> dealtCardAnimUIs,
    Vector3 startPosition,
    int leaderPlayerId,
    int dealerPlayerId)
    {
        Dictionary<int, DealtPlayerCardUISnapshot> snapshots = dealtCardAnimUIs.ToDictionary(x => x.PlayerId);

        List<int> playerIdsInTurnOrder = GetPlayerIdsStartingFromLeader(leaderPlayerId);

        foreach (int playerId in playerIdsInTurnOrder)
        {
            yield return DealCardsToPlayer(snapshots[playerId], startPosition);
        }
    }

    private IEnumerator DealCardsToPlayer(
    DealtPlayerCardUISnapshot player,
    Vector3 startPosition)
    {
        Debug.Log($"{this} called. player.CardUIs.Count: {player.CardUIs.Count}");

        for (int i = 0; i < player.CardUIs.Count; i++)
        {
            yield return _animateCardManager.AnimateMove(player.CardUIs[i], startPosition);

            yield return new WaitForSeconds(0.12f);
        }

        yield return new WaitForSeconds(0.25f);
    }

    private List<int> GetPlayerIdsStartingFromLeader(int leaderPlayerId)
    {
        var playerIds = playerUIs.Keys.ToList();

        int leaderIndex = playerIds.IndexOf(leaderPlayerId);

        if (leaderIndex < 0)
            throw new InvalidOperationException($"Leader player ID {leaderPlayerId} not found.");

        return playerIds
            .Skip(leaderIndex)
            .Concat(playerIds.Take(leaderIndex))
            .ToList();
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