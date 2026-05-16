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

    [Header("Prefabs")]
    [SerializeField] private GameObject cardPrefab;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI nameTextUI;
    [SerializeField] private TextMeshProUGUI scoreTextUI;

    [Header("Events")]
    public UnityEvent<bool> OnCardSelected;

    private Player player;
    private CardUI selectedCard;

    private readonly Dictionary<Card, CardUI> handCardUIs = new();
    private readonly Dictionary<Card, CardUI> playedCardUIs = new();

    public void Bind(Player newPlayer)
    {
        player = newPlayer;
        Render();
    }

    public void Render()
    {
        if (player == null)
        {
            Debug.LogError("PlayerUI.Render called before Bind.");
            return;
        }

        RenderText();
        RenderHand();
        RenderPlayedCards();
    }

    public void RenderText()
    {
        nameTextUI.text = player.Name;
        scoreTextUI.text = "Score: " + player.Points;
    }

    private void RenderHand()
    {
        ClearCardParent(cardHandParent, handCardUIs);

        foreach (Card card in player.Hand)
        {
            if (card == null)
            {
                Debug.LogError("Null card found in player hand.");
                continue;
            }

            CardUI cardUI = CreateCardUI(card, cardHandParent);
            cardUI.OnCardClicked += HandleCardClicked;

            handCardUIs[card] = cardUI;
        }
    }

    public void RenderPlayedCards()
    {
        ClearCardParent(cardsPlayedParent, playedCardUIs);

        float offset = 1f;

        foreach (Card card in player.PlayedCards)
        {
            if (card == null)
            {
                Debug.LogError("Null card found in played cards.");
                continue;
            }

            CardUI cardUI = CreateCardUI(card, cardsPlayedParent);
            SetupCenteredOffset(cardUI.transform as RectTransform, offset);

            playedCardUIs[card] = cardUI;

            offset += 3f;
        }
    }

    private CardUI CreateCardUI(Card card, Transform parent)
    {
        GameObject cardGO = Instantiate(cardPrefab, parent, false);

        CardUI cardUI = cardGO.GetComponent<CardUI>();

        if (cardUI == null)
        {
            Debug.LogError("Card prefab is missing CardUI component.");
            return null;
        }

        cardUI.Setup(card, player.Id);
        return cardUI;
    }

    private void SetupCenteredOffset(RectTransform rect, float offset)
    {
        if (rect == null) return;

        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(offset, -offset);
        rect.localScale = Vector3.one;
    }

    private void ClearCardParent(Transform parent, Dictionary<Card, CardUI> lookup)
    {
        foreach (CardUI cardUI in lookup.Values)
        {
            if (cardUI != null)
            {
                cardUI.OnCardClicked -= HandleCardClicked;
            }
        }

        lookup.Clear();

        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        selectedCard = null;
        OnCardSelected?.Invoke(false);
    }

    public void RemoveCardFromHand(Card card)
    {
        if (card == null)
        {
            Debug.LogError("Tried to remove null card from hand.");
            return;
        }

        if (!handCardUIs.TryGetValue(card, out CardUI cardUI))
        {
            Debug.LogWarning($"No CardUI found for card {card} in {player.Name}'s hand.");
            return;
        }

        if (selectedCard == cardUI)
        {
            selectedCard = null;
            OnCardSelected?.Invoke(false);
        }

        cardUI.OnCardClicked -= HandleCardClicked;
        handCardUIs.Remove(card);

        Destroy(cardUI.gameObject);
    }

    public void AddCardToHand(Card card)
    {
        if (card == null)
        {
            Debug.LogError("Tried to add null card to hand.");
            return;
        }

        CardUI cardUI = CreateCardUI(card, cardHandParent);
        cardUI.OnCardClicked += HandleCardClicked;

        handCardUIs[card] = cardUI;
    }

    public void HandleCardClicked(CardUI cardUI, int playerID)
    {
        if (playerID != 0) return;
        if (cardUI == null) return;

        if (selectedCard != null)
        {
            selectedCard.SetSelected(false);
        }

        if (selectedCard == cardUI)
        {
            selectedCard = null;
        }
        else
        {
            selectedCard = cardUI;
            selectedCard.SetSelected(true);

            Debug.Log($"Card pressed: {selectedCard.GetCard()}");
        }

        OnCardSelected?.Invoke(selectedCard != null);
    }

    public Card GetSelectedCard()
    {
        return selectedCard != null ? selectedCard.GetCard() : null;
    }

    public CardUI GetSelectedCardUI()
    {
        return selectedCard;
    }

    public void AllowCardPlay(Player player)
    {
        //TO DO
    }

    public void ShowPlayableCards(IReadOnlyList<Card> playableCards)
    {
        foreach (var kvp in handCardUIs)
        {
            Card card = kvp.Key;
            CardUI cardUI = kvp.Value;

            bool playable = playableCards.Contains(card);

            cardUI.SetPlayable(playable);
        }
    }
    public void ResetPlayableCards()
    {
        foreach (var kvp in handCardUIs)
        {
            CardUI cardUI = kvp.Value;
            cardUI.SetPlayable(true);
        }
    }
}