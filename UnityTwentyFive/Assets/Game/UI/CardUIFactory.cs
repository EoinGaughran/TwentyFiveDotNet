using System.Collections.Generic;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class CardUIFactory : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject cardPrefabSmall;

    private readonly Dictionary<int, CardUI> allCardUIs = new();

    public CardUI CreateCardUI(Card card, bool sizeSmall)
    {
        GameObject _cardPrefab;

        if (sizeSmall) 
            _cardPrefab = cardPrefabSmall;
        else
            _cardPrefab = cardPrefab;

        GameObject cardGO = Instantiate(_cardPrefab);

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