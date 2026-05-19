using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DeckUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI label;

    private RectTransform rectTransform;

    private int cardNumber;
    private bool TrumpDrawable;

    [Header("Events")]
    public UnityEvent OnTrumpFliped;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Init(int initialCardNumber)
    {
        cardNumber = initialCardNumber;
        TrumpDrawable = false;

        SetupRect();
        Render();
    }

    public void SetCardNumber(int newCardNumber)
    {
        cardNumber = newCardNumber;
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
        label.text = cardNumber.ToString();
    }

    public void SetTrumpToDrawable()
    {
        TrumpDrawable = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Deck clicked.");

        if(TrumpDrawable)
        {
            OnTrumpFliped?.Invoke();
            TrumpDrawable=false;
        }
        else
        {
            Debug.Log("You can not draw a card right now.");
        }

        
    }
}