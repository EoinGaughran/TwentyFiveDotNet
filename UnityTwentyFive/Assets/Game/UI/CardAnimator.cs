using System.Collections;
using UnityEngine;

public class CardAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform AnimationLayer;

    public IEnumerator AnimateMove(CardUI cardUI, Vector3 startPosition)
    {
        CardUI AnimCardUI = cardUI.Clone(AnimationLayer);
        AnimCardUI.transform.position = startPosition;
        cardUI.SetHidden(true);
        AnimCardUI.SetHidden(true);

        Vector3 endPosition = cardUI.transform.position;

        Debug.Log(
            "CardAnim is animating from x: "
            + startPosition.x
            + ", y: "
            + startPosition.y
            + " to x: "
            + endPosition.x
            + ", y: "
            + endPosition.y
        );

        AnimCardUI.SetHidden(false);

        yield return AnimCardUI.AnimateTo(
            endPosition,
            0.1f);

        cardUI.SetHidden(false);
        Destroy(AnimCardUI.gameObject);
    }
}