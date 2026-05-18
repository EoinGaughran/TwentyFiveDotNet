using System;
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
    [SerializeField] private GameObject statusCardPrefab;

    private DeckUI deckUI;

    public void RenderDeckCount(int cardCount)
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
    }
    public void AllowTrumpFlip()
    {
        //TO DO
    }

    public void RenderStatusCard(Card StatusCard, StatusCardType statusCardType)
    {
        Transform transform = GetStatusCardTransform(statusCardType);
 
        StatusCardUI statusCardUI = Instantiate(statusCardPrefab, transform).GetComponent<StatusCardUI>();
        statusCardUI.Init(StatusCard, statusCardType);

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
    }
}