using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckUI : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI label;
    public RectTransform CardVisual;

    private int CardNumber;

    public void Setup(int cardNumber)
    {
        this.CardNumber = cardNumber;

        UpdateText();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Deck was clicked.");
    }

    public void UpdateText()
    {
        label.text = CardNumber.ToString();
    }
}
