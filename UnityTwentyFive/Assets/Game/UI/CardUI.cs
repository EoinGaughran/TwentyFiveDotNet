using System;
using System.Collections;
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
    [SerializeField] private RectTransform rectTransformVisual;
    private RectTransform rectTransform;

    private static readonly Color PlayableColor = Color.white;
    private static readonly Color UnplayableColor = new(0.7f, 0.7f, 0.7f);

    private int cardID;
    private int playerID;
    private bool isPlayable;

    public event Action<CardUI> OnCardClicked;

    public void Setup(Card card)
    {
        cardID = card.Id;
        isPlayable = true;
        label.text = card.ToString();
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public int GetCardID() => cardID;

    public CardUI GetCardUI() => this;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardClicked?.Invoke(this);
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
        isPlayable = playable;
        cardImage.color = playable
            ? PlayableColor
            : UnplayableColor;
    }

    public bool IsPlayable()
    {
        return isPlayable;
    }

    public void SetPositionSlot(float x, float y)
    {
        CardVisual.transform.localPosition =
            new Vector3(
                x,
                y,
                CardVisual.transform.localPosition.z);
    }

    public void SetupRect()
    {
        rectTransform.anchorMin =
        rectTransform.anchorMax =
        rectTransform.pivot =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    public void SetCardSize(CardSize size)
    {
        switch (size)
        {
            case CardSize.Small:
                label.fontSize = 30;
                rectTransformVisual.sizeDelta = new Vector2(90, 140);
                rectTransform.sizeDelta = new Vector2(90, 140);
                break;

            case CardSize.Large:
                label.fontSize = 50;
                rectTransformVisual.sizeDelta = new Vector2(140, 200);
                rectTransform.sizeDelta = new Vector2(140, 200);
                break;
        }
    }

    public void AnimateTo(Vector2 targetAnchoredPosition, float duration)
    {
        StartCoroutine(MoveTo(targetAnchoredPosition, duration));
    }

    private IEnumerator MoveTo(Vector2 target, float duration)
    {
        Vector2 start = rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // smooth ease
            t = t * t * (3f - 2f * t);

            rectTransform.anchoredPosition =
                Vector2.Lerp(start, target, t);

            yield return null;
        }

        rectTransform.anchoredPosition = target;
    }
}
