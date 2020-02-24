using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour 
{
    [Header("User")]
	public Text userNameText;

    [Header("Question")]
    public Text questionText;

    [Header("UI")]
    public Text scoreText;
	public Text timeText;
	public Text roundText;
	public Text markerText;
	public Text winText;
	public Text loseText;

    [Header("Pannels")]

    public Fade qaPannelFade;
	public Fade roundsPannelFade;
	public Fade overPannelFade;
    public Fade defaultAnswersFade;
    public Fade imageTextAnswersFade;
    public Fade imageTextOptionfade;
    public Fade imageAnswerFade;

    [Header("Pools")]

    public SimpleObjectPool defaultAnswerButtonObjectPool;
    public SimpleObjectPool imageOptionObjectPool;
    public SimpleObjectPool imageAnswerObjectPool;
    public SimpleObjectPool textOptionObjectPool;
    public SimpleObjectPool textAnswerObjectPool;
    public SimpleObjectPool imageAnswerButtonObjectPool;

    [Header("Root objects")]

    public Transform answerButtonDefaultParent;
    public Transform answerImageTextParent;
    public Transform optionImageParent;
    public Transform imageAnswerButtonDefaultParent;

    [Header("Audio")]

    public AudioClip correctClip;
    public AudioClip wrongClip;
    public AudioSource audioSource;
	private RoundData currentRoundData;
	private int currentRoundNumber = -1;
	private QuestionData[] questionPool;
	private bool isRoundActive;
    private bool hasTimer;
	private float timeRemaining;
	private int questionNumber;
	private int nextQuestionIndex;
	private int playerScore;
	private List<GameObject> answerButtonGameObjects = new List<GameObject>();
    private List<GameObject> imageOptionGameObjects = new List<GameObject>();
    private List<GameObject> answerImageGameObjects = new List<GameObject>();
    private List<GameObject> textOptionGameObjects = new List<GameObject>();
    private List<GameObject> answerTextGameObjects = new List<GameObject>();
    private List<GameObject> imageAnswerButtonGameObjects = new List<GameObject>();
    private QuestionData questionData;
	private int corrects;
	private int correctsOnChunk;
	private int errors;
	private bool win;
	private Constants.Levels currentLevel;
	private UserLocalData user;
    private int correctAnswersMultiResponse;

	void Start()
	{
		// Listen to events
		EventManager.StartListening<BasicEvent> (Constants.ON_CURRENT_DATA_RECEIVED, OnCurrentDataReceived);
		EventManager.StartListening<BasicEvent> (Constants.ON_ANSWER_CLICK_DONE, OnAnswerClickDone);
		EventManager.StartListening<BasicEvent> (Constants.LAST_ROUND_ANSWER, OnLastRoundAnswer);
        EventManager.StartListening<BasicEvent> (Constants.ON_PAIRS_CHECK_DONE, OnPairsCheckDone);
        playerScore = 0;
		questionNumber = 0;
		currentRoundNumber = -1;
		isRoundActive = false;
        hasTimer = false;
        corrects = correctsOnChunk = 0;
		errors = 0;
		markerText.text = corrects + "/" + Constants.QUESTIONS_PER_ROUND;
		user = UserLocal.ReadUserData ();
		// Show user name
		userNameText.text = user.Name;
		win = true;
		currentLevel = user.Level;
        correctAnswersMultiResponse = 0;

        // Show Round #0, only for first round
        if (currentRoundNumber == -1) 
		{
			StartGame ();
		}
	}

	void OnDisable()
	{
		EventManager.StopListening<BasicEvent> (Constants.ON_CURRENT_DATA_RECEIVED, OnCurrentDataReceived);
		EventManager.StopListening<BasicEvent> (Constants.ON_ANSWER_CLICK_DONE, OnAnswerClickDone);
		EventManager.StopListening<BasicEvent> (Constants.LAST_ROUND_ANSWER, OnLastRoundAnswer);
        EventManager.StopListening<BasicEvent> (Constants.ON_PAIRS_CHECK_DONE, OnPairsCheckDone);
    }

	private void OnCurrentDataReceived(BasicEvent e)
	{
		currentRoundData = (RoundData)e.Data;
		questionPool = currentRoundData.Questions;

		timeRemaining = questionPool [0].TimeLimit;

		// After 2 secs show questions, start the game!
		Invoke("StartRound", 2f);
		// Show first question
		Invoke("ShowQuestion", 2f);
	}

    private void OnAnswerClickDone(BasicEvent e)
	{
		bool isCorrect = (bool)e.Data;
		
		AnswerButtonClicked (isCorrect);
	}

    private void OnPairsCheckDone(BasicEvent e)
    {
        int[] indexes = (int[])e.Data;

        correctAnswersMultiResponse++;

        // All answers are ok
        if (questionData.Answers.Count == correctAnswersMultiResponse)
        {
            AnswerButtonClicked(true);
        }
        else
        {
            audioSource.clip = correctClip;
            audioSource.Play();
            // Return to pool option and answer used
            /*imageOptionObjectPool.ReturnObject(imageOptionGameObjects[indexes[0]-1]);
            imageOptionGameObjects.RemoveAt(indexes[0]-1);

            imageAnswerObjectPool.ReturnObject(answerOptionGameObjects[indexes[1]-1]);
            answerOptionGameObjects.RemoveAt(indexes[1]-1);*/
        }
    }

    private void OnLastRoundAnswer(BasicEvent e)
	{
		bool isLastRound = (bool)e.Data;

		// End game, show results
		if (isLastRound) 
		{
			timeText.text = "0";
			markerText.enabled = false;
			isRoundActive = false;
			overPannelFade.StartFadeIn (0.3f);
			qaPannelFade.StartFadeOut (0.3f);
			roundsPannelFade.StartFadeOut (0.3f);

			// Save last score
			user.LastScore = playerScore;
			UserLocal.SaveUserData (user);

			// Send score to database
			EventManager.TriggerEvent (Constants.ON_SEND_DATA_TO_DATABASE, new BasicEvent(user));

			// Show proper feedback
			if (win) 
			{
				winText.enabled = true;
				loseText.enabled = false;
			} 
			else 
			{
				winText.enabled = false;
				loseText.enabled = true;
			}
		}
	}

	private void RemoveAnswerButtons()
	{
        // Hide all answer containers
        defaultAnswersFade.StartFadeOut(0.1f);
        imageTextAnswersFade.StartFadeOut(0.1f);
        imageTextOptionfade.StartFadeOut(0.1f);
        imageAnswerFade.StartFadeOut(0.1f);

        // Remove old answer buttons
        while (answerButtonGameObjects.Count > 0) 
		{
            defaultAnswerButtonObjectPool.ReturnObject (answerButtonGameObjects[0]);
			answerButtonGameObjects.RemoveAt (0);
		}

        while (imageOptionGameObjects.Count > 0)
        {
            imageOptionObjectPool.ReturnObject(imageOptionGameObjects[0]);
            imageOptionGameObjects.RemoveAt(0);
        }

        while (answerImageGameObjects.Count > 0)
        {
            imageAnswerObjectPool.ReturnObject(answerImageGameObjects[0]);
            answerImageGameObjects.RemoveAt(0);
        }

        while (textOptionGameObjects.Count > 0)
        {
            textOptionObjectPool.ReturnObject(textOptionGameObjects[0]);
            textOptionGameObjects.RemoveAt(0);   
        }

        while (answerTextGameObjects.Count > 0)
        {
            textAnswerObjectPool.ReturnObject(answerTextGameObjects[0]);
            answerTextGameObjects.RemoveAt(0);
        }

        while (imageAnswerButtonGameObjects.Count > 0)
        {
            imageAnswerButtonObjectPool.ReturnObject(imageAnswerButtonGameObjects[0]);
            imageAnswerButtonGameObjects.RemoveAt(0);
        }
    }

	private void ShowQuestion()
	{
		// First remove old answer buttons
		RemoveAnswerButtons ();
		// Get question from pool
		questionData = questionPool [nextQuestionIndex];
		questionText.text = questionData.QuestionText;
		timeRemaining = questionData.TimeLimit;
        correctAnswersMultiResponse = 0;

        // The question have time or not?
        if (timeRemaining == 0)
        {
            hasTimer = false;
            timeText.text = Mathf.Round(timeRemaining).ToString();
        }
        else
        {
            hasTimer = true;
        }

        // Show containers depending on question type
        switch (questionData.Type)
        {
            case QuestionData.QuestionType.DEFAULT:
                defaultAnswersFade.StartFadeIn(0.3f);
                break;
            case QuestionData.QuestionType.IMAGES:
            case QuestionData.QuestionType.TEXTS:
                imageTextAnswersFade.StartFadeIn(0.3f);
                imageTextOptionfade.StartFadeIn(0.3f);
                break;
            case QuestionData.QuestionType.IMAGE:
                imageAnswersFade.StartFadeIn(0.3f);
                break;
        }

        // Create N answer buttons
        for (int i = 0; i < questionData.Answers.Count; i++) 
		{
            switch (questionData.Type)
            {
                case QuestionData.QuestionType.DEFAULT:
                    GameObject answerButtonGameObject = defaultAnswerButtonObjectPool.GetObject();
                    answerButtonGameObject.transform.SetParent(answerButtonDefaultParent, false);
                    answerButtonGameObjects.Add(answerButtonGameObject);
                    AnswerTextButton answerButton = answerButtonGameObject.GetComponent<AnswerTextButton>();
                    answerButton.Setup(questionData.Answers[i]);
                    break;
                case QuestionData.QuestionType.IMAGES:
                    // Create option
                    GameObject imageOptionGameObject = imageOptionObjectPool.GetObject();
                    imageOptionGameObject.transform.SetParent(optionImageParent, false);
                    imageOptionGameObjects.Add(imageOptionGameObject);
                    OptionImage optionImage = imageOptionGameObject.GetComponent<OptionImage>();
                    optionImage.Setup(questionData.Answers[i], i+1);

                    // Create answer
                    GameObject answerImageGameObject = imageAnswerObjectPool.GetObject();
                    answerImageGameObject.transform.SetParent(answerImageTextParent, false);
                    answerImageGameObjects.Add(answerImageGameObject);
                    AnswerImage answerImage = answerImageGameObject.GetComponent<AnswerImage>();
                    answerImage.Setup(questionData.Answers[i], i+1);

                    break;
                case QuestionData.QuestionType.TEXTS:
                    // Create option
                    GameObject textOptionGameObject = textOptionObjectPool.GetObject();
                    textOptionGameObject.transform.SetParent(optionImageParent, false);
                    textOptionGameObjects.Add(textOptionGameObject);
                    OptionText optionText = textOptionGameObject.GetComponent<OptionText>();
                    optionText.Setup(questionData.Answers[i], i + 1);

                    // Create answer
                    GameObject answerTextGameObject = textAnswerObjectPool.GetObject();
                    answerTextGameObject.transform.SetParent(answerImageTextParent, false);
                    answerTextGameObjects.Add(answerTextGameObject);
                    AnswerText answerText = answerTextGameObject.GetComponent<AnswerText>();
                    answerText.Setup(questionData.Answers[i], i + 1);

                    break;
                case QuestionData.QuestionType.IMAGE:
                    GameObject imageAnswerButtonGameObject = defaultAnswerButtonObjectPool.GetObject();
                    imageAnswerButtonGameObject.transform.SetParent(answerButtonDefaultParent, false);
                    answerButtonGameObjects.Add(imageAnswerButtonGameObject);
                    AnswerImageButton imageAnswerButton = imageAnswerButtonGameObject.GetComponent<AnswerImageButton>();
                    imageAnswerButton.Setup(questionData.Answers[i]);
                    break;
            }
		}
	}

	public void AnswerButtonClicked(bool isCorrect)
	{
		// Was correct?
		if (isCorrect) 
		{
            audioSource.clip = correctClip;
            audioSource.Play();
            corrects++;
			correctsOnChunk++;
			// Show marker
			markerText.text = corrects + "/" + Constants.QUESTIONS_PER_ROUND;
			// Add score to user
			playerScore += questionData.PointsAdded;
			scoreText.text = playerScore.ToString ();
		}
		else 
		{
            audioSource.clip = wrongClip;
            audioSource.Play();
            errors++;
		}

		// Level check done on 1/2 questions and last question of every round (chunk)
		if ((questionNumber != 0) && (((questionNumber + 1) % Constants.NUM_QUESTION_CHECK_LEVEL) == 0)) 
		{
			// If this chunk of question was done perfect, level up
			if (correctsOnChunk == Constants.NUM_QUESTION_CHECK_LEVEL) 
			{
				// Level up ...
				LevelUp();
			} 
			// If only one or less correct answer, level down
			else if (correctsOnChunk <= 0) 
			{
				// Level down ...
				LevelDown();
			}

			// Reset to check in next chunk
			correctsOnChunk = 0;
		}

		// Get next question or end round
		NextQuestionOrEnd ();
	}

	private void LevelUp()
	{
		switch (currentLevel) 
		{
			default:
				currentLevel = (Constants.Levels)(currentLevel + 1);
				break;
			case Constants.Levels.HARD:
				break;
		}

		// Save new level
		user.Level = currentLevel;
		UserLocal.SaveUserData (user);
	}

	private void LevelDown()
	{
		switch (currentLevel) 
		{
			default:
				currentLevel = (Constants.Levels)(currentLevel - 1);
				break;
			case Constants.Levels.BASIC:
				break;
		}

		// Save new level
		user.Level = currentLevel;
		UserLocal.SaveUserData (user);
	}

	private void NextQuestionOrEnd()
	{
		// Wasn't last question of current round?
		if (Constants.QUESTIONS_PER_ROUND > (questionNumber + 1)) 
		{
			// Will get next question
			questionNumber++;
			GetNextQuestionIndex();
			ShowQuestion ();
		}
		// Was last question
		else 
		{
			// End round
			EndRound();
		}
	}

	private void GetNextQuestionIndex()
	{
		// Get question index on question pool based on user's level
		// First 6 questions => BASIC
		// 6 on the middle => MEDIUM
		// the last 6 => HARD
		switch (currentLevel) 
		{
			case Constants.Levels.BASIC:
				nextQuestionIndex = questionNumber;
				break;
			case Constants.Levels.MEDIUM:
				nextQuestionIndex = questionNumber + Constants.QUESTIONS_PER_ROUND;
				break;
			case Constants.Levels.HARD:
				nextQuestionIndex = questionNumber + (Constants.QUESTIONS_PER_ROUND * 2);
				break;
		}
	}

    public void EndRound()
	{
		// Resets question index
		questionNumber = 0;
		GetNextQuestionIndex ();

		// If user is winning and corrects are more than 5, errors less than 3
		if (currentRoundNumber != -1) 
		{
			if (win && (corrects >= 5) && (errors < 3)) 
			{
				win = true;
			} 
			else 
			{
				win = false;
			}
		}

		// Next round
		currentRoundNumber++;

		// Show round pannel
		roundText.text = currentRoundNumber.ToString();
		isRoundActive = false;
		qaPannelFade.StartFadeOut (0f);
		roundsPannelFade.StartFadeIn (0.3f);
		overPannelFade.StartFadeOut (0f);
	
		// Is last round?
		EventManager.TriggerEvent (Constants.IS_LAST_ROUND);
	}

	private void StartGame()
	{
		EndRound ();
	}

	private void StartRound()
	{
		// Show Q&A pannel and restart marker
		corrects = errors = 0;
		markerText.text = corrects + "/" + Constants.QUESTIONS_PER_ROUND;
		roundText.text = currentRoundNumber.ToString();
		isRoundActive = true;
		qaPannelFade.StartFadeIn (0.3f);
		roundsPannelFade.StartFadeOut (0.3f);
		overPannelFade.StartFadeOut (0.3f);

	}

	public void RestartGame()
	{
		SceneManager.LoadScene ("Start");
	}

	private void UpdateTimeRemaing ()
	{
		timeText.text = Mathf.Round(timeRemaining).ToString();

		// If time is over, next question or end
		if (timeRemaining <= 0f) 
		{
			errors++;
			NextQuestionOrEnd ();
		}
	}

	void Update()
	{
		// Update time if there is a question
		if (isRoundActive && hasTimer) 
		{
			timeRemaining -= Time.deltaTime;
			UpdateTimeRemaing ();
		}
	}
}
