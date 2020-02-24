using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataController : MonoBehaviour {

	RoundData[] allRoundData;
	int currentRound;
	bool lastRound = false;
    private List<QuestionData> questionsParsed;
    public bool random;

    void Start () {
		// Reiceive data from Remote storege (FireBase)
		EventManager.StartListening<BasicEvent> (Constants.ON_REMOTE_DATA_RECEIVED, OnRemoteDataReceived);
	
		// Init
		currentRound = 0;
		allRoundData = new RoundData [Constants.NUM_ROUNDS];

		// Check last round
		EventManager.StartListening<BasicEvent> (Constants.IS_LAST_ROUND, OnIsLastRound);

		lastRound = false;

        EventManager.TriggerEvent(Constants.ON_REMOTE_DATA_RECEIVED);
    }

    // Knuth shuffle algorithm
    public void Shuffle()
    {
        QuestionData tempGO;

        var levels = EnumUtil.GetNames<Constants.Levels>();

        // All levels ...
        for (int j = 0; j < levels.Length; j++)
        {
            Debug.Log("Suffle " + GetMinQuestionIndex(j) + " " + GetMaxQuestionIndex(j));
            for (int i = GetMinQuestionIndex(j); i < GetMaxQuestionIndex(j); i++)
            {
                int rnd = Random.Range(GetMinQuestionIndex(j), GetMaxQuestionIndex(j));
                tempGO = questionsParsed[rnd];
                questionsParsed[rnd] = questionsParsed[i];
                questionsParsed[i] = tempGO;
            }
        }
    }

#pragma warning disable 0162
    private int GetMaxQuestionIndex(int level)
    {
        switch (level)
        {
            case 0:
                return Constants.QUESTIONS_PER_ROUND * 2;
                break;
            case 1:
                return Constants.QUESTIONS_PER_ROUND * 4;
                break;
            case 2:
                return Constants.QUESTIONS_PER_ROUND * 6;
                break;
        }

        return 0;
    }

    private int GetMinQuestionIndex(int level)
    {
        switch (level)
        {
            case 0:
                return 0;
                break;
            case 1:
                return Constants.QUESTIONS_PER_ROUND * 2;
                break;
            case 2:
                return Constants.QUESTIONS_PER_ROUND * 4;
                break;
        }
        return 0;
    }
#pragma warning restore 0162

    void OnRemoteDataReceived (BasicEvent e)
	{
		//string jsonData = (string)e.Data;

        JsonParser parser = new JsonParser(Resources.Load<TextAsset>("quiz").text);

        //JsonParser parser = new JsonParser(jsonData);
		questionsParsed = parser.Parse();

        // Random question order
        if (random)
        {
            Shuffle();
        }

		// 6 questions per round * 3 difficulty levels = 18 questions stored per round
		int numberQuestions = Constants.QUESTIONS_PER_ROUND * EnumUtil.GetCount<Constants.Levels> ();

		// For all rounds
		for (int i = 0; i < Constants.NUM_ROUNDS; i++) 
		{
			// Create a set of questions based on parsed data
			allRoundData [i] = new RoundData ();
			allRoundData[i].Questions = new QuestionData [numberQuestions];

			// For all the questions on round
			// Index on parsed data list, first row starts on 0 second row starts on 6 ...
			int index = i * Constants.QUESTIONS_PER_ROUND;
			for (int j = 0; j < numberQuestions; j++) 
			{
				// Create a question
				allRoundData [i].Questions [j] = new QuestionData ();

				// Get question from parsed data
				allRoundData [i].Questions [j] = questionsParsed [index];

				// Get next index, on chunks of 6 questions
				if ((j > 0) && ((j+1) % Constants.QUESTIONS_PER_ROUND) == 0)
					index += (Constants.QUESTIONS_PER_ROUND + 1);
				else
					index++;
			}
		}

		// Send current round to Game Controller
		EventManager.TriggerEvent (Constants.ON_CURRENT_DATA_RECEIVED, new BasicEvent(GetCurrentRoundData()));
	}

	void OnDisable()
	{
		EventManager.StopListening<BasicEvent> (Constants.IS_LAST_ROUND, OnIsLastRound);
		EventManager.StopListening<BasicEvent> (Constants.ON_REMOTE_DATA_RECEIVED, OnRemoteDataReceived);
	}

	private void OnIsLastRound(BasicEvent e)
	{
		// Check if its last round, skip to next round internally
		NextRoundData ();

		// Answer with isLastRound check to Game Controller
		EventManager.TriggerEvent (Constants.LAST_ROUND_ANSWER, new BasicEvent(lastRound));
	}

	private RoundData GetCurrentRoundData()
	{
		return allRoundData [currentRound];
	}

	private int GetRoundNumber()
	{
		return currentRound;
	}

	private void NextRoundData()
	{
		currentRound++;

		// Finish game!
		if (currentRound == Constants.NUM_ROUNDS) 
		{
			lastRound = true;
		} 
		else 
		{
			// Send current round to Game Controller
			EventManager.TriggerEvent (Constants.ON_CURRENT_DATA_RECEIVED, new BasicEvent(GetCurrentRoundData()));
		}
	}

}
