using TMPro;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class CardUI : MonoBehaviour
{
    public TextMeshProUGUI label;

    public void Setup(Card card, bool isHuman)
    {
        if (isHuman)
        {
            label.text = card.ToString();
        }
        else
        {
            label.text = card.ToString();
        }
    }
}
