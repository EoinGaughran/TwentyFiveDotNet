using TMPro;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class PhaseUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentPhase;

    public void Render(GamePhase phase)
    {
        currentPhase.text = "Current Phase: " + phase;
    }
}