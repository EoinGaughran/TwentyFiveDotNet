using UnityEngine;

public class StatusCardData
{
    public Transform Transform { get; }
    public CardUI StatusCardUI { get; }
    public StatusCardData(Transform transform, CardUI statusCardUI)
    {
        Transform = transform;
        StatusCardUI = statusCardUI;
    }
}