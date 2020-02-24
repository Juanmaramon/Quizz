using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerText : MonoBehaviour, IAnswer
{
	public Text answerText;
	private AnswerData answerData;
    public int index;
	
    void GetRectSet()
    {
        Vector2 S = new Vector2(gameObject.GetComponent<RectTransform>().rect.width, gameObject.GetComponent<RectTransform>().rect.height);
        gameObject.GetComponent<BoxCollider2D>().size = S;
    }

	public void Setup(AnswerData data, int idx)
	{
        GetComponent<CanvasGroup>().alpha = 1f;
        GetComponent<BoxCollider2D>().enabled = true;
        Invoke("GetRectSet", 0.05f);
        answerData = data;
        answerText.text = answerData.AnswerText;
        index = idx;
    }

    public void CheckAnswer(int optionIdx)
    {
        GetComponent<CanvasGroup>().alpha = 0f;
        GetComponent<BoxCollider2D>().enabled = false;
        bool isCorrect = false;
        for (int i = 0; i < answerData.QaCorrespondence.Count; i++)
        {
            isCorrect = (answerData.QaCorrespondence[i][0] == optionIdx) && (answerData.QaCorrespondence[i][1] == index);

            if (isCorrect)
            {
                // Send event to game controller with answer
                EventManager.TriggerEvent(Constants.ON_PAIRS_CHECK_DONE, new BasicEvent(new int[] { optionIdx, index } ));
                return;
            }
        }

        // Send event to game controller with answer
        EventManager.TriggerEvent(Constants.ON_ANSWER_CLICK_DONE, new BasicEvent(isCorrect));
    }
}