using System.Collections.Generic;
using TMPro;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;
using UnityEngine.Events;

public class PlayerUI : MonoBehaviour
{
    public Transform cardHandParent;
    public Transform cardsPlayedParent;
    public GameObject cardPrefab;

    public TextMeshProUGUI NameTextUI;
    public TextMeshProUGUI ScoreTextUI;

    private Player player;
    private CardUI selectedCard;

    public UnityEvent<bool> OnCardSelected;

    public void Bind(Player player)
    {
        this.player = player;

        RenderPlayer();
    }

    void RenderPlayer()
    {
        UpdateText();
        RenderCardGroup(player.Hand, cardHandParent);
        RenderPlayedCards(player.PlayedCards, cardsPlayedParent);
    }

    public void UpdateText()
    {
        NameTextUI.text = player.Name;
        ScoreTextUI.text = "Score: " + player.Points;
    }

    private void RenderCardGroup(IReadOnlyList<Card> cardGroup, Transform cardParent)
    {
        // Clear existing cards
        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var card in cardGroup)
        {
            if (IsCardNull(card))
                return;

            GameObject cardHandGO = Instantiate(cardPrefab, cardParent);

            CardUI cardUI = cardHandGO.GetComponent<CardUI>();
            cardUI.Setup(card, player.Id);

            cardUI.OnCardClicked += HandleCardClicked;
        }
    }

    private void RenderPlayedCards(IReadOnlyList<Card> cardGroup, Transform cardParent)
    {
        // Clear existing cards
        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }

        float offset = 1f;

        foreach (var card in cardGroup)
        {
            if (IsCardNull(card))
                return;

            GameObject cardHandGO = Instantiate(cardPrefab, cardParent, false);

            RectTransform rect = cardHandGO.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.anchoredPosition = new Vector2(offset, -offset);
            rect.localScale = Vector3.one;

            CardUI cardUI = cardHandGO.GetComponent<CardUI>();
            cardUI.Setup(card, player.Id);

            offset += 3f;
        }
    }

    public void HandleCardClicked(CardUI card, int playerID)
    {
        if (playerID != 0) return;

        if(selectedCard != null)
            selectedCard.SetSelected(false);

        if(selectedCard == card)
        {
            selectedCard = null;
        }
        else
        {
            selectedCard = card;
            selectedCard.SetSelected(true);
            Debug.Log($"Card pressed: {selectedCard.GetCard()}");
        }

        OnCardSelected?.Invoke(selectedCard != null);
    }

    public bool IsCardUINull(CardUI card)
    {
        if (card == null)
        {
            Debug.LogError("cardUI is NULL");
            return true;
        }
        return false;
    }

    public static bool IsCardNull(Card card)
    {
        if (card == null)
        {
            Debug.LogError("card is NULL");
            return true;
        }
        return false;
    }

    public static bool AreAnyCardsNull(IReadOnlyList<Card> cardGroup)
    {
        foreach (var card in cardGroup)
        {
            if (IsCardNull(card))
                return true;
        }
        return false;
    }

    public Card GetSelectedCard() => selectedCard.GetCard();

    public CardUI GetSelectedCardUI() => selectedCard.GetCardUI();

    public void RemoveCardFromHand(CardUI card)
    {
        if (IsCardUINull(card))
            return;


    }
}
