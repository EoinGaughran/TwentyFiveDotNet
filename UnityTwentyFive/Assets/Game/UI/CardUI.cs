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


    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite backSprite;
    public bool Flipped { get; private set; }
    public float flipDuration = 0.5f;
    private float currentYRotation = 0f;
    private bool isFlipping = false;

    public event Action<CardUI> OnCardClicked;

    public void Setup(Card card)
    {
        cardID = card.Id;
        isPlayable = true;
        label.text = card.ToString();
        Flipped = false;
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

    public void AnimateTo(Vector2 targetAnchoredPosition, Transform newParent, float duration)
    {
        Vector3 worldPosition = transform.position;
        rectTransform.SetParent(newParent, false);
        SetupRect();
        transform.position = worldPosition;

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

    public void SetCardFlip(bool flipState)
    {
        Flipped = flipState;
        currentYRotation = 180f;
        cardVisual.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);

        cardImage.sprite = Flipped ? backSprite : frontSprite;
        label.enabled = !Flipped;

        cardVisual.localScale = Flipped
            ? new Vector3(-1, 1, 1)
            : new Vector3(1, 1, 1);
    }

    public void FlipCard()
    {
        StartCoroutine(FlipToBackRoutine());
    }

    public IEnumerator FlipToBackRoutine()
    {
        if (isFlipping) yield break;
        isFlipping = true;

        float startAngle = currentYRotation;
        float endAngle = startAngle + 180f;
        float swapAngle = startAngle + 90f;

        float t = 0f;
        bool swapped = false;

        while (t < flipDuration)
        {
            t += Time.deltaTime;

            float progress = Mathf.Clamp01(t / flipDuration);
            currentYRotation = Mathf.Lerp(startAngle, endAngle, progress);

            cardVisual.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);

            if (!swapped && currentYRotation >= swapAngle)
            {
                Flipped = !Flipped;

                cardImage.sprite = Flipped ? backSprite : frontSprite;
                label.enabled = !Flipped;

                cardVisual.localScale = Flipped
                    ? new Vector3(-1, 1, 1)
                    : new Vector3(1, 1, 1);

                swapped = true;
            }

            yield return null;
        }

        currentYRotation = endAngle;
        cardVisual.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);

        isFlipping = false;
    }
}
