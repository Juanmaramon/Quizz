using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnswerData 
{
	string answerText;
	bool isCorrect;
    List<int[]> qaCorrespondence;
    QuestionData.QuestionType answerType;
    int answerIndex;
    string answerImagePath;
    string optionImagePath;
    string optionText;

    public string AnswerText
	{
		get { return answerText; }
		set { answerText = value; }
	}

	public bool IsCorrect
	{
		get { return isCorrect; }
		set { isCorrect = value; }
	}

    public List<int[]> QaCorrespondence
    {
        get { return qaCorrespondence; }
        set { qaCorrespondence = value; }
    }

    public QuestionData.QuestionType AnswerType
    {
        get { return answerType; }
        set { answerType = value; }
    }

    public int AnswerIndex
    {
        get { return answerIndex; }
        set { answerIndex = value;  }
    }

    public string AnswerImagePath
    {
        get { return answerImagePath; }
        set { answerImagePath = value; }
    }

    public string OptionImagePath
    {
        get { return optionImagePath; }
        set { optionImagePath = value; }
    }

    public string OptionText
    {
        get { return optionText; }
        set { optionText = value; }
    }
}
