using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    private int _id;

    public void Init(int id)
    {
        _id = id;
    }

    public void UpdateFrom(Player player)
    {
        // update text, cards, etc.
    }
}
