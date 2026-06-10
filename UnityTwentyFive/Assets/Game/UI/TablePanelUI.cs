using System;
using System.Collections.Generic;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;
public class TablePanelUI : MonoBehaviour
{
    [Header("Deck")]
    [SerializeField] private Transform deckSlot;
    [SerializeField] private GameObject deckPrefab;

    [Header("Status Card")]
    [SerializeField] private Transform trumpSlot;
    [SerializeField] private Transform ledCardSlot;
    [SerializeField] private Transform winningCardSlot;

    private readonly Dictionary<StatusCardType, CardUI> statusCardUIs = new();

    private DeckUI deckUI;

    public bool RenderDeckCount(int cardCount)
    {
        if (deckUI == null)
        {
            deckUI = Instantiate(deckPrefab, deckSlot).GetComponent<DeckUI>();
            deckUI.Init(cardCount);
        }
        else
        {
            deckUI.SetCardNumber(cardCount);
        }
        return true;
    }
    public void AllowTrumpFlip()
    {
        if(deckUI == null)
        {
            Debug.LogWarning("There is no UI for Deck.");
            return;
        }

        deckUI.SetTrumpToDrawable();
    }

    public void DrawCardUIFromDeckUI(CardUI card)
    {
        if (card == null)
        {
            Debug.LogError("TablePanelUI.DrawCardUIFromDeckUI: There is no UI for card.");
            return;
        }

        card.SetCardFlip(true);
        card.transform.SetParent(deckSlot, false);
        card.SetupRect();
        card.FlipCard();
    }

    public void RegisterStatusCardUI(CardUI statusCardUI, StatusCardType statusCardType)
    {
        statusCardUIs[statusCardType] = statusCardUI;
    }

    public void AddCardToStatusSlot(CardUI statusCard, StatusCardType statusCardType)
    {
        DestroyStatusCard(statusCardType);

        Transform transform = GetStatusCardTransform(statusCardType);

        statusCard.transform.SetParent(transform, false);
        statusCard.SetupRect();
        statusCard.Highlight(3);
    }

    public void MoveCardToStatusSlot(CardUI statusCard, StatusCardType statusCardType)
    {
        Transform slotTransform = GetStatusCardTransform(statusCardType);

        statusCard.AnimateTo(Vector2.zero, slotTransform, 2f);

        statusCardUIs[statusCardType] = statusCard;
    }

    public bool TryGetStatusCardUI(StatusCardType type, out CardUI cardUI)
    {
        return statusCardUIs.TryGetValue(type, out cardUI) && cardUI != null;
    }

    private Transform GetStatusCardTransform(StatusCardType statusCardType)
    {
        return statusCardType switch
        {
            StatusCardType.TrumpCard => trumpSlot,
            StatusCardType.LedCard => ledCardSlot,
            StatusCardType.WinningCard => winningCardSlot,
            _ => throw new ArgumentOutOfRangeException(nameof(statusCardType))
        };
    }

    public void DestroyStatusCard(StatusCardType statusCardType)
    {
        Transform parent = GetStatusCardTransform(statusCardType);

        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        statusCardUIs.Remove(statusCardType);
    }
}