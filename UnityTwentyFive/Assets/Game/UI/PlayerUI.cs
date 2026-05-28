using System.Collections.Generic;
using System.Linq;
using TMPro;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;
using UnityEngine.Events;

public class PlayerUI : MonoBehaviour
{
    [Header("Parents")]
    [SerializeField] private Transform cardHandParent;
    [SerializeField] private Transform cardsPlayedParent;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI nameTextUI;
    [SerializeField] private TextMeshProUGUI scoreTextUI;

    [Header("Events")]
    public UnityEvent<bool> OnCardSelected;

    private int playerID;
    private string playerName;
    private CardUI selectedCard;
    private bool cardPlayAllowed;

    private readonly Dictionary<int, CardUI> handCardUIs = new();
    private readonly Dictionary<int, CardUI> playedCardUIs = new();

    public int PlayerID => playerID;
    public Transform HandParent => cardHandParent;
    public Transform PlayedCardsParent => cardsPlayedParent;

    public void Bind(Player player)
    {
        playerID = player.Id;
        playerName = player.Name;

        cardPlayAllowed = false;
        selectedCard = null;

        RenderText(player.Name, player.Points);
        ClearHand();
        ClearPlayedCards();
    }

    public void RenderText(string name, int points)
    {
        playerName = name;
        nameTextUI.text = name;
        scoreTextUI.text = "Score: " + points;
    }

    public void AddCardToHand(CardUI cardUI)
    {
        if (cardUI == null)
        {
            Debug.LogError("Tried to add null CardUI to hand.");
            return;
        }

        int cardID = cardUI.GetCardID();

        cardUI.transform.SetParent(cardHandParent, false);
        cardUI.OnCardClicked -= HandleCardClicked;
        cardUI.OnCardClicked += HandleCardClicked;

        handCardUIs[cardID] = cardUI;
        playedCardUIs.Remove(cardID);
    }

    public bool RemoveCardFromHand(int cardID)
    {
        if (!handCardUIs.TryGetValue(cardID, out CardUI cardUI))
        {
            Debug.LogWarning($"No CardUI found for card ID {cardID} in {playerName}'s hand.");
            return false;
        }

        if (selectedCard == cardUI)
            ClearSelectedCard();

        cardUI.OnCardClicked -= HandleCardClicked;
        handCardUIs.Remove(cardID);

        return true;
    }

    public bool AddCardToPlayedCards(CardUI cardUI)
    {
        if (cardUI == null)
        {
            Debug.LogError("Tried to add null CardUI to played cards.");
            return false;
        }

        int cardID = cardUI.GetCardID();

        cardUI.OnCardClicked -= HandleCardClicked;
        playedCardUIs[cardID] = cardUI;

        RectTransform cardRect = cardUI.GetComponent<RectTransform>();

        cardRect.SetParent(cardsPlayedParent, true);
        SetupCardRect(cardRect, false);

        int pCount = playedCardUIs.Count;
        float offsetAmount = 4;
        float offset = pCount * offsetAmount;
        Vector2 destination = new(Vector2.zero.x + offset, Vector2.zero.y + offset);

        cardUI.AnimateTo(destination, 0.25f);

        return true;
    }
    public void SetupCardRect(RectTransform rect, bool resetPosition)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;

        if (resetPosition)
            rect.anchoredPosition = Vector2.zero;
    }

    private void LayoutPlayedCards()
    {
        float offset = 1f;

        foreach (CardUI cardUI in playedCardUIs.Values)
        {
            SetupCenteredOffset(cardUI.transform as RectTransform, offset);
            offset += 3f;
        }
    }

    private void SetupCenteredOffset(RectTransform rect, float offset)
    {
        if (rect == null) return;

        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(offset, -offset);
        rect.localScale = Vector3.one;
    }

    public void AllowCardPlay()
    {
        cardPlayAllowed = true;
    }

    public void DisableCardPlay()
    {
        cardPlayAllowed = false;
        ClearSelectedCard();
    }

    public bool CanHumanPlayCards()
    {
        return cardPlayAllowed;
    }

    public int? GetSelectedCardID()
    {
        return selectedCard != null ? selectedCard.GetCardID() : null;
    }

    public CardUI GetSelectedCardUI()
    {
        return selectedCard;
    }

    private void HandleCardClicked(CardUI cardUI)
    {
        if (!cardPlayAllowed)
        {
            Debug.Log($"Its not your turn.");
            return;
        }
        
        if (cardUI == null)
        {
            Debug.Log($"The UI for clicked card {cardUI} is null.");
            return;
        }

        if (!handCardUIs.ContainsKey(cardUI.GetCardID()))
        {
            Debug.Log($"The clicked card does not exist in your hand.");
            return;
        }
        if (!cardUI.IsPlayable())
        {
            Debug.Log($"This card cannot be played.");
            return;
        }

        if (selectedCard == cardUI)
        {
            ClearSelectedCard();
            return;
        }

        if (selectedCard != null)
            selectedCard.SetSelected(false);

        selectedCard = cardUI;
        selectedCard.SetSelected(true);

        Debug.Log($"Card pressed, CardID: {selectedCard.GetCardID()}");
        OnCardSelected?.Invoke(true);
    }

    private void ClearSelectedCard()
    {
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        selectedCard = null;
        OnCardSelected?.Invoke(false);
    }

    public void ShowPlayableCards(IReadOnlyList<int> playableCardIDs)
    {
        foreach (var kvp in handCardUIs)
        {
            int cardID = kvp.Key;
            CardUI cardUI = kvp.Value;

            bool playable = playableCardIDs.Contains(cardID);
            cardUI.SetPlayable(playable);
        }
    }

    public void ResetPlayableCards()
    {
        foreach (CardUI cardUI in handCardUIs.Values)
            cardUI.SetPlayable(true);
    }

    public void ClearHand()
    {
        foreach (CardUI cardUI in handCardUIs.Values)
        {
            if (cardUI != null)
                cardUI.OnCardClicked -= HandleCardClicked;
        }

        handCardUIs.Clear();
        ClearSelectedCard();
    }

    public void ClearPlayedCards()
    {
        playedCardUIs.Clear();
    }
}