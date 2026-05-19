using TMPro;
using TwentyFiveDotNet.Core.Models;
using UnityEngine;

public class PhaseUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentPhase;

    public void Render(string phase)
    {
        currentPhase.text = "Current Phase: " + phase;
    }
}