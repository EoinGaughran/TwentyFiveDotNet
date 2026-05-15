using TMPro;
using UnityEngine;

public class ConsoleLogUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    public void AppendText(string text)
    {
        label.text += "\n" + text;
    }

    public void ClearText()
    {
        label.text = "";
    }
}