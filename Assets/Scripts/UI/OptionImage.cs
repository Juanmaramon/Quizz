using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionImage : MonoBehaviour, IOption
{
	public Image optionImage;
	private AnswerData answerData;
    int index;

    public int GetIndex()
    {
        return index;
    }

    void GetRectSet()
    {
        GetComponent<UIDraggable>().SetInitialPosition();
        Vector2 S = new Vector2(gameObject.GetComponent<RectTransform>().rect.width, gameObject.GetComponent<RectTransform>().rect.height);
        gameObject.GetComponent<BoxCollider2D>().size = S;
    }

    public void Setup(AnswerData data, int idx)
	{
        GetComponent<CanvasGroup>().alpha = 1f;
        GetComponent<BoxCollider2D>().enabled = true;
        GetComponent<UIDraggable>().enable = true;
        Invoke("GetRectSet", 0.05f);
        answerData = data;
        optionImage.sprite = Resources.Load<Sprite>(Constants.IMAGE_OPTION_PATH + answerData.OptionImagePath);
        index = idx;
	}
}