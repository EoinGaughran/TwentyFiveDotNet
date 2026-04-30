using TMPro;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public Transform cardParent;
    public GameObject cardPrefab;
    public CardUI selectedCard;

    public TextMeshProUGUI NameTextUI;
    public TextMeshProUGUI ScoreTextUI;

    private Player player;

    public void Bind(Player player)
    {
        this.player = player;

        RenderHand();
    }

    void RenderHand()
    {
        // Clear existing cards
        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }

        NameTextUI.text = player.Name;
        ScoreTextUI.text = "Score: " + player.Points;

        foreach (var card in player.Hand)
        {
            if (card == null)
                Debug.LogError("card is NULL");

            GameObject cardGO = Instantiate(cardPrefab, cardParent);

            CardUI cardUI = cardGO.GetComponent<CardUI>();
            cardUI.Setup(card, player.Id);

            cardUI.OnCardClicked += HandleCardClicked;
        }
    }

    void HandleCardClicked(CardUI card, int playerID)
    {
        if (playerID != 0) return;

        if(selectedCard != null)
            selectedCard.SetSelected(false);

        selectedCard = card;
        selectedCard.SetSelected(true);
    }

    public Card GetSelectedCard()
    {
        return selectedCard?.GetCard();
    }
}
