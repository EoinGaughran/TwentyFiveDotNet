using UnityEngine;

public class DeckPanelUI : MonoBehaviour
{
    [SerializeField] private Transform deckSlot;
    [SerializeField] private GameObject deckPrefab;

    private DeckUI deckUI;

    public void RenderDeckCount(int cardCount)
    {
        if (deckUI == null)
        {
            deckUI = Instantiate(deckPrefab, deckSlot).GetComponent<DeckUI>();
            deckUI.Init(cardCount);
        }
        else
        {
            deckUI.SetCardNumber(cardCount);
        }
    }
    public void AllowTrumpFlip()
    {
        //TO DO
    }
}