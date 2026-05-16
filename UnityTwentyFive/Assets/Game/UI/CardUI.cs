using System;
using TMPro;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI label;
    public RectTransform CardVisual;
    [SerializeField] private Image cardImage;

    private static readonly Color PlayableColor = Color.white;
    private static readonly Color UnplayableColor = new Color(0.7f, 0.7f, 0.7f);

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
        SetPositionSlot(
           CardVisual.transform.localPosition.x,
           offset
           );

    }

    public void SetPlayable(bool playable)
    {
        cardImage.color = playable
            ? PlayableColor
            : UnplayableColor;
    }

    public void SetPositionSlot(float x, float y)
    {
        CardVisual.transform.localPosition =
            new Vector3(
                x,
                y,
                CardVisual.transform.localPosition.z);
    }
}
