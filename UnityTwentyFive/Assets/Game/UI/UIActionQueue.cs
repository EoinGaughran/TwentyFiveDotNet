using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActionQueue : MonoBehaviour
{
    private readonly Queue<(float delayAfter, Action action)> queue = new();
    private bool isPlaying;

    public void EnqueueUI(float delayAfter, Action action)
    {
        queue.Enqueue((delayAfter, action));

        if (!isPlaying)
            StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        isPlaying = true;

        while (queue.Count > 0)
        {
            var item = queue.Dequeue();
            item.action?.Invoke();

            if (item.delayAfter > 0f)
                yield return new WaitForSeconds(item.delayAfter);
        }

        isPlaying = false;
    }
}