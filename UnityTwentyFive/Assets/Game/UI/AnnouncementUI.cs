using System.Collections;
using TMPro;
using UnityEngine;

public class AnnouncementUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup canvasGroup;

    public void Show(string message)
    {
        StopAllCoroutines();
        StartCoroutine(ShowRoutine(message));
    }
    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator ShowRoutine(string message)
    {
        text.text = message;

        yield return Fade(0, 1, 0.5f);

        yield return new WaitForSeconds(1f);

        yield return Fade(1, 0, 0.5f);
    }

    private IEnumerator FadeOutRoutine()
    {
        yield return Fade(1, 0, 0.5f);
    }

    private IEnumerator Fade(float start, float end, float duration)
    {
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;

            canvasGroup.alpha =
                Mathf.Lerp(start, end, t / duration);

            yield return null;
        }

        canvasGroup.alpha = end;
    }
}