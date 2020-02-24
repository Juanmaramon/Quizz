using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerImageButton : MonoBehaviour 
{
	public Image answerImage;
	private AnswerData answerData;

	void Start () 
	{
        
	}
	
	public void Setup(AnswerData data)
	{
		answerData = data;
        answerImage.sprite = Resources.Load<Sprite>(Constants.IMAGE_ANSWER_PATH + answerData.AnswerImagePath);
    }

	public void HandleClick()
	{
		// Send event to game controller with answer
		EventManager.TriggerEvent (Constants.ON_ANSWER_CLICK_DONE, new BasicEvent(answerData.IsCorrect));
	}
}