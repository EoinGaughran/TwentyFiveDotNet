using TMPro;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrumpUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI label;

    private RectTransform rectTransform;

    private Card TrumpCard;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Init(Card trumpCard)
    {
        TrumpCard = trumpCard;

        SetupRect();
        Render();
    }

    public void SetTrumpCard(Card trumpCard)
    {
        TrumpCard = trumpCard;
        Render();
    }

    private void SetupRect()
    {
        rectTransform.anchorMin =
        rectTransform.anchorMax =
        rectTransform.pivot =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    private void Render()
    {
        label.text = TrumpCard.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Trump clicked.");
    }
}