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
    private readonly StatusCardUI trumpUI;
    private readonly StatusCardUI ledCardUI;
    private readonly StatusCardUI winningCardUI;

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

    public void RenderStatusCard(Card StatusCard, StatusCardType StatusCardType)
    {
        StatusCardData statusCardData = GetStatusCardData(StatusCardType);

        StatusCardUI statusCardUI = statusCardData.StatusCardUI;
        Transform transform = statusCardData.Transform;

        if (statusCardUI == null)
        {
            statusCardUI = Instantiate(statusCardPrefab, transform).GetComponent<StatusCardUI>();
            statusCardUI.Init(StatusCard, StatusCardType);
        }
        else
        {
            statusCardUI.SetStatusCard(StatusCard, StatusCardType);
        }
    }
    private StatusCardData GetStatusCardData(StatusCardType statusCardType)
    {
        StatusCardUI statusCardUI = null;
        Transform slot = null;

        switch (statusCardType)
        {
            case StatusCardType.TrumpCard:

                slot = trumpSlot;
                statusCardUI = trumpUI;
                break;

            case StatusCardType.LedCard:

                slot = ledCardSlot;
                statusCardUI = ledCardUI;
                break;

            case StatusCardType.WinningCard:

                slot = winningCardSlot;
                statusCardUI = winningCardUI;
                break;
        }

        return new StatusCardData(slot, statusCardUI);
    }
}