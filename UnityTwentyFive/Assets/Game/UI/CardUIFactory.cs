using System.Collections.Generic;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class CardUIFactory : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;

    private readonly Dictionary<int, CardUI> allCardUIs = new();

    private CardUI CreateCardUI(Card card, bool isCPU, Transform parent)
    {
        GameObject cardGO = Instantiate(cardPrefab, parent, false);

        CardUI cardUI = cardGO.GetComponent<CardUI>();

        cardUI.Setup(card);

        CardSize cardSize;

        if(isCPU)
        {
            cardSize = CardSize.Small;
            cardUI.SetCardFlip(true);
        }
            
        else
            cardSize = CardSize.Large;

        cardUI.SetCardSize(cardSize);
        cardUI.SetupRect();

        return cardUI;
    }

    public CardUI CreateHandCardUI(Card card, bool isCPU, Transform parent)
    {
        CardUI cardUI = CreateCardUI(card, isCPU, parent);

        allCardUIs[card.Id] = cardUI;

        return cardUI;
    }

    public CardUI CreateAnimationCardUI(Card card, bool isCPU, Transform parent)
    {
        CardUI cardUI = CreateCardUI(card, isCPU, parent);

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