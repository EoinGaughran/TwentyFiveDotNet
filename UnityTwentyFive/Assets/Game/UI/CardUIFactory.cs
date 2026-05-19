using System.Collections.Generic;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class CardUIFactory : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;

    private readonly Dictionary<int, CardUI> allCardUIs = new();

    public CardUI CreateCardUI(Card card)
    {
        GameObject cardGO = Instantiate(cardPrefab);

        CardUI cardUI = cardGO.GetComponent<CardUI>();

        cardUI.Setup(card);

        allCardUIs[card.Id] = cardUI;

        return cardUI;
    }

    public bool TryGetCardUI(int cardId, out CardUI cardUI)
    {
        return allCardUIs.TryGetValue(cardId, out cardUI);
    }

    public void ClearAllCardUIs()
    {
        allCardUIs.Clear();
    }
}