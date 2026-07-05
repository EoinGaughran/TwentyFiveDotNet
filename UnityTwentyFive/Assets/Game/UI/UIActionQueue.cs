using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActionQueue : MonoBehaviour
{
    private readonly Queue<(float delayAfter, Func<IEnumerator> coroutine)> queue = new();
    private bool isPlaying;

    public void EnqueueUI(float delayAfter, Action action)
    {
        EnqueueUI(delayAfter, () => RunAction(action));
    }
    private IEnumerator RunAction(Action action)
    {
        action?.Invoke();
        yield break;
    }

    public void EnqueueUI(
    float delayAfter,
    Func<IEnumerator> coroutine)
    {
        queue.Enqueue((delayAfter, coroutine));

        if (!isPlaying)
            StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        isPlaying = true;

        while (queue.Count > 0)
        {
            var item = queue.Dequeue();

            yield return item.coroutine();

            if (item.delayAfter > 0f)
                yield return new WaitForSeconds(item.delayAfter);
        }

        isPlaying = false;
    }
}