using System.Collections.Generic;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class CardUIFactory : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;

    private readonly Dictionary<int, CardUI> allCardUIs = new();

    public CardUI CreateCardUI(Card card, bool sizeSmall, bool register)
    {
        GameObject cardGO = Instantiate(cardPrefab);

        CardUI cardUI = cardGO.GetComponent<CardUI>();

        cardUI.Setup(card);

        if (register)
            allCardUIs[card.Id] = cardUI;

        CardSize cardSize;

        if(sizeSmall)
            cardSize = CardSize.Small;
        else
            cardSize = CardSize.Large;

        cardUI.SetCardSize(cardSize);
        cardUI.SetupRect();

        return cardUI;
    }

    public bool TryGetCardUI(int cardId, out CardUI cardUI)
    {
        return allCardUIs.TryGetValue(cardId, out cardUI);
    }

    public void ClearAllCardUIs()
    {
        foreach (var cardUI in allCardUIs.Values)
        {
            if (cardUI != null)
                Destroy(cardUI.gameObject);
        }

        allCardUIs.Clear();
    }
}