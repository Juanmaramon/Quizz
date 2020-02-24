using UnityEngine;
using System;
using System.Collections.Generic;
using MiniJSON;

public class JsonParser
{
    private string dataFile;
    private Dictionary<string, object> json;
    private string levelNode;
    private List<object> levelNodeProcessed;
    private string questionData;
    private Dictionary<string, object> questionDataProcessed;

	// List with all questions => from 0,QUESTIONS_PER_ROUND => BASIC...
	private List<QuestionData> questionResult = new List<QuestionData>();

    public JsonParser(string jsonData)
    {
		dataFile = jsonData;
    }

	public List<QuestionData> Parse()
    {
        json = Json.Deserialize(dataFile) as Dictionary<string, object>;

        var levels = EnumUtil.GetNames<Constants.Levels>();

        // All levels ...
        for (int i = 0; i < levels.Length; i++)
        {
            ParseLevel(levels[i]);
        }

		return questionResult;
    }

    private void ParseLevel(string level)
    {
        levelNode = Json.Serialize(json[level.ToLower()]);

        levelNodeProcessed = Json.Deserialize(levelNode) as List<object>;

        // All questions ...
        foreach (object element in levelNodeProcessed)
        {
            questionData = Json.Serialize(element);
            ParseQuestion(questionData);
        }
    }

    private void ParseQuestion(string questionData)
    {
        questionDataProcessed = Json.Deserialize(questionData) as Dictionary<string, object>;
		QuestionData question = new QuestionData ();
		foreach (KeyValuePair<string, object> par in questionDataProcessed)
        {
            // Question data parsing goes here
            // Load question container
            switch (par.Key.ToString())
            {
                case "id":
                    question.Id = int.Parse(par.Value.ToString());
                    break;
                case "type":
                    question.Type = QuestionData.DecodeType(par.Value.ToString().ToUpper());
                    break;
                case "options":
                    int j = 0;
                    foreach (object val in (List<object>)par.Value)
                    {
                        switch (question.Type)
                        {
                            case QuestionData.QuestionType.IMAGES:
                                question.Answers[j].OptionImagePath = val.ToString();
                                break;
                            case QuestionData.QuestionType.TEXTS:
                                Debug.Log(val.ToString());
                                question.Answers[j].OptionText = val.ToString();
                                break;
                        }
                        j++;
                    }
                    break;
				case "question":
					question.QuestionText = par.Value.ToString();
                    break;
				case "answers":
					question.Answers = new List<AnswerData> ();
                    AnswerData answer = null;
                    foreach (object val in (List<object>)par.Value)
					{
                        switch (question.Type)
                        {
                            case QuestionData.QuestionType.DEFAULT:
                            case QuestionData.QuestionType.TEXTS:
                                answer = new AnswerData();
                                answer.AnswerText = val.ToString();
                                question.Answers.Add(answer);
                                break;
                            case QuestionData.QuestionType.IMAGES:
                                answer = new AnswerData();
                                answer.AnswerImagePath = val.ToString();
                                question.Answers.Add(answer);
                                break;
                        }
						
					}
                    break;
				case "correct":
					int i = 0;
                    Debug.Log(question.Id);
                    switch (question.Type)
                    {
                        case QuestionData.QuestionType.DEFAULT:
                            foreach (object val in (List<object>)par.Value)
                            {
                                question.Answers[i].IsCorrect = bool.Parse(val.ToString());
                                i++;
                            }
                            break;
                        case QuestionData.QuestionType.IMAGES:
                        case QuestionData.QuestionType.TEXTS:
                            List<int[]> tmplist = new List<int[]>();
                            foreach (object val in (List<object>)par.Value)
                            {
                                question.Answers[i].QaCorrespondence = new List<int[]>();
                                string tmpAnswer = val.ToString();
                                int[] qaCorrespondenceNode = new int[2];
                                qaCorrespondenceNode[0] = int.Parse(tmpAnswer.Split(':')[0]);
                                qaCorrespondenceNode[1] = int.Parse(tmpAnswer.Split(':')[1]);
                                tmplist.Add(qaCorrespondenceNode);
                                i++;
                            }
                            i = 0;
                            foreach (object val in (List<object>)par.Value)
                            {
                                question.Answers[i].QaCorrespondence = new List<int[]>(tmplist);
                                i++;
                            }
                            break;
                    }
				
                    break;
				case "timeLimit":
					question.TimeLimit = float.Parse (par.Value.ToString());
					break;
				case "points":
					question.PointsAdded = int.Parse (par.Value.ToString());
					break;
            }
        }
		questionResult.Add (question);
    }
   
}
