using System;
using TMPro;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI label;

    private Card card;
    private int playerID;

    public event Action<CardUI, int> OnCardClicked;

    public void Setup(Card card, int playerID)
    {
        this.card = card;
        this.playerID = playerID;

        if (playerID == 0)
        {
            label.text = card.ToString();
        }
        else
        {
            label.text = card.ToString();
        }

        Debug.Log(card.ToString() + " cooards: "
            + "\n x position: " + transform.localPosition.x
            + "\n y position: " + transform.localPosition.y
            + "\n z position: " + transform.localPosition.z);
    }

    public Card GetCard() => card;

    public CardUI GetCardUI() => this;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardClicked?.Invoke(this, playerID);
    }

    public void SetSelected(bool selected)
    {
        float offset = selected ? 30f : 0f;
        transform.localPosition = new Vector3(transform.localPosition.x, offset, transform.localPosition.z);
    }
}
