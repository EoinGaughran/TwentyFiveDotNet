using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleLogUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private ScrollRect scrollRect;

    public void AppendText(string text)
    {
        Debug.Log(text);

        label.text += "\n" + text;

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ClearText()
    {
        label.text = "";
    }
}