using UnityEngine;

public class StatusCardData
{
    public Transform Transform { get; }
    public StatusCardUI StatusCardUI { get; }
    public StatusCardData(Transform transform, StatusCardUI statusCardUI)
    {
        Transform = transform;
        StatusCardUI = statusCardUI;
    }
}