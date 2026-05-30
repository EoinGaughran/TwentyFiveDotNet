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
    [SerializeField] public RectTransform cardVisual;
    [SerializeField] private Image cardImage;
    [SerializeField] private Outline cardOutline;
    private RectTransform rectTransform;

    private static readonly Color PlayableColor = Color.white;
    private static readonly Color UnplayableColor = new(0.7f, 0.7f, 0.7f);

    private int cardID;
    private int playerID;
    private bool isPlayable;

    [SerializeField] private Sprite backSprite;
    public float flipDuration = 0.5f;

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
           cardVisual.transform.localPosition.x,
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
        cardVisual.transform.localPosition =
            new Vector3(
                x,
                y,
                cardVisual.transform.localPosition.z);
    }

    public void SetTransparentStyle()
    {
        // fully transparent white
        cardImage.color =
            new Color(1f, 1f, 1f, 0.02f);

        // white outline/stroke
        cardOutline.effectColor = Color.white;

        // white text
        label.color = Color.white;
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
                cardVisual.sizeDelta = new Vector2(90, 126);
                rectTransform.sizeDelta = new Vector2(90, 126);
                break;

            case CardSize.Large:
                label.fontSize = 50;
                cardVisual.sizeDelta = new Vector2(140, 196);
                rectTransform.sizeDelta = new Vector2(140, 196);
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
    public void Highlight(int loops = 1)
    {
        StartCoroutine(HighlightRoutine(loops));
    }

    private IEnumerator HighlightRoutine(int loops)
    {
        Vector3 original = transform.localScale;
        Vector3 enlarged = original * 1.1f;

        float duration = 0.08f;

        for (int i = 0; i < loops; i++)
        {
            float t = 0f;

            // Scale up
            while (t < duration)
            {
                t += Time.deltaTime;

                transform.localScale = Vector3.Lerp(
                    original,
                    enlarged,
                    t / duration);

                yield return null;
            }

            t = 0f;

            // Scale down
            while (t < duration)
            {
                t += Time.deltaTime;

                transform.localScale = Vector3.Lerp(
                    enlarged,
                    original,
                    t / duration);

                yield return null;
            }
        }

        transform.localScale = original;
    }

    public void FlipToBack()
    {
        StartCoroutine(FlipToBackRoutine());
    }

    public IEnumerator FlipToBackRoutine()
    {
        float t = 0f;
        bool swapped = false;

        while (t < flipDuration)
        {
            t += Time.deltaTime;
            float progress = t / flipDuration;
            float angle = Mathf.Lerp(0f, 180f, progress);

            cardVisual.transform.localRotation = Quaternion.Euler(0f, angle, 0f);

            if (!swapped && angle >= 90f)
            {
                cardVisual.localScale = new Vector3(-1, 1, 1);
                cardImage.sprite = backSprite;
                label.enabled = false;
                swapped = true;
            }

            yield return null;
        }

        cardVisual.localRotation = Quaternion.Euler(0f, 180f, 0f);
    }
}
