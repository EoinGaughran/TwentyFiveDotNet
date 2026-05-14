using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class TrumpPanelUI : MonoBehaviour
{
    [SerializeField] private Transform TrumpSlot;
    [SerializeField] private GameObject TrumpPrefab;

    private TrumpUI TrumpUI;

    public void RenderTrump(Card TrumpCard)
    {
        if (TrumpUI == null)
        {
            TrumpUI = Instantiate(TrumpPrefab, TrumpSlot).GetComponent<TrumpUI>();
            TrumpUI.Init(TrumpCard);
        }
        else
        {
            TrumpUI.SetTrumpCard(TrumpCard);
        }
    }
}