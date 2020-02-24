using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnswer
{
    void CheckAnswer(int optionIdx);
}

public interface IOption
{
    int GetIndex();
}

public class QuestionData 
{
    int id;
	string questionText;
	float timeLimit;
	int pointsAdded;
    QuestionType type;

    public enum QuestionType {
        DEFAULT,
        IMAGES,
        TEXTS,
        IMAGE
    }

	List<AnswerData> answers;

    public int Id
    {
        get { return id; }
        set { id = value; }
    }

	public string QuestionText
	{
		get { return questionText; }
		set { questionText = value; }
	}

	public float TimeLimit
	{
		get { return timeLimit; }
		set { timeLimit = value; }
	}

	public int PointsAdded
	{
		get { return pointsAdded; }
		set { pointsAdded = value; }
	}

	public List<AnswerData> Answers
	{
		get { return answers; }
		set { answers = value; }
	}

    public QuestionType Type
    {
        get { return type; }
        set { type = value; }
    }

#pragma warning disable 0162
    public static QuestionType DecodeType(string typeArg)
    {
        switch (typeArg)
        {
            case "DEFAULT":
                return QuestionType.DEFAULT;
                break;
            case "IMAGES":
                return QuestionType.IMAGES;
                break;
            case "TEXTS":
                return QuestionType.TEXTS;
                break;
        }

        return QuestionType.DEFAULT;
    }
#pragma warning restore 0162
}
