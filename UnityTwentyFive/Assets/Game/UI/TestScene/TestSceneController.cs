using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class TestSceneController : MonoBehaviour
{
    [SerializeField] private Transform cardSlot;
    [SerializeField] private CardUIFactory _cardUIFactory;

    private Card testCard;
    private CardUI testCardUI;

    void Start()
    {
        testCard = new()
        {
            Id = 0,
            Suit = Card.Suits.Hearts,
            Rank = Card.Ranks.Two
        };

        testCardUI = _cardUIFactory.CreateAnimationCardUI(testCard, true, cardSlot);

        testCardUI.SetupRect();

        testCardUI.OnCardClicked += HandleCardClicked;
    }

    private void HandleCardClicked(CardUI cardUI)
    {
        if (cardUI == null)
        {
            Debug.Log($"The UI for clicked card {cardUI} is null.");
            return;
        }

        Debug.Log($"Card pressed, CardID: {cardUI.GetCardID()}. Its is a {cardUI.name}");

        cardUI.FlipCard();
    }
}
