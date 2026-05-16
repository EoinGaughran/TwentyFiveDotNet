using TMPro;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;
using UnityEngine.EventSystems;

public class StatusCardUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI label;

    private RectTransform rectTransform;

    private Card statusCard;
    private StatusCardType statusCardType;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Init(Card StatusCard, StatusCardType StatusCardType)
    {
        statusCard = StatusCard;
        statusCardType = StatusCardType;

        SetupRect();
        Render();
    }

    public void SetStatusCard(Card StatusCard, StatusCardType StatusCardType)
    {
        statusCard = StatusCard;
        statusCardType = StatusCardType;
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
        label.text = statusCard.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"{statusCardType} clicked." +
            $"\n It is a {statusCard}");
    }
}