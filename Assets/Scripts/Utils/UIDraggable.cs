using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIDraggable : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Vector2 initialPosition;
    public bool enable;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
        enable = true;
    }

    public void SetInitialPosition()
    {
        initialPosition = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (enable)
            rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (enable)
            rectTransform.anchoredPosition = initialPosition;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.GetComponent<IAnswer>() != null)
        {
            enable = false;
            col.gameObject.GetComponent<IAnswer>().CheckAnswer(GetComponent<IOption>().GetIndex());
            GetComponent<CanvasGroup>().alpha = 0f;
            GetComponent<Collider2D>().enabled = false;
        }
    }
}