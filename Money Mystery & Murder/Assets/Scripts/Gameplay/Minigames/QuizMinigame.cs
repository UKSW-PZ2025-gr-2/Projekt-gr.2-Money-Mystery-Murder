using UnityEngine;
<<<<<<< HEAD
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Quiz minigame where players answer multiple-choice questions to earn balance.
/// Each correct answer adds reward to their balance when the game ends.
/// Inherits from <see cref="MinigameBase"/> and integrates with <see cref="Player"/>.
/// </summary>
public class QuizMinigame : MinigameBase
{
    /// <summary>
    /// Question data structure for the quiz.
    /// </summary>
    [System.Serializable]
    public class Question
    {
        [TextArea(2, 4)]
        public string questionText;
        public string[] answers = new string[4];
        public int correctAnswerIndex; // 0-3
        [Range(10, 500)]
        public int rewardPerCorrect = 50;
    }

    /// <summary>
    /// Cost to start playing the quiz.
    /// Set this in the Unity Inspector.
    /// </summary>
    [Header("Quiz Settings")]
    [SerializeField] private int startCost = 5;

    /// <summary>
    /// All available questions for the quiz.
    /// Add questions in the Unity Inspector.
    /// </summary>
    [SerializeField] private Question[] questions = new Question[0];

    /// <summary>
    /// Duration in seconds for the quiz.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private float quizDurationSeconds = 30f;

    /// <summary>
    /// TextMeshPro component to display question text.
    /// Assign in the Unity Inspector.
    /// </summary>
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI questionDisplay;

    /// <summary>
    /// Array of buttons for answer options.
    /// Assign in the Unity Inspector.
    /// </summary>
    [SerializeField] private UnityEngine.UI.Button[] answerButtons = new UnityEngine.UI.Button[4];

    /// <summary>
    /// TextMeshPro component to display remaining time.
    /// Assign in the Unity Inspector.
    /// </summary>
    [SerializeField] private TextMeshProUGUI timerDisplay;

    /// <summary>
    /// TextMeshPro component to display score.
    /// Assign in the Unity Inspector.
    /// </summary>
    [SerializeField] private TextMeshProUGUI scoreDisplay;

    /// <summary>
    /// Canvas or panel containing the quiz UI.
    /// Assign in the Unity Inspector.
    /// </summary>
    [SerializeField] private GameObject quizPanel;

    // runtime-created root (if UI is auto-generated)
    private GameObject _runtimeCanvasRoot;

    /// <summary>
    /// Current question index.
    /// </summary>
    private int _currentQuestionIndex = 0;

    /// <summary>
    /// Number of correct answers so far.
    /// </summary>
    private int _correctAnswersCount = 0;

    /// <summary>
    /// Remaining time in quiz.
    /// </summary>
    private float _timeRemaining;

    /// <summary>
    /// Whether the quiz is actively running (not paused).
    /// </summary>
    private bool _isActivelyRunning = false;

    /// <summary>
    /// Called when the quiz starts. Shows the first question and starts the timer.
    /// </summary>
    protected override void OnStartGame()
    {
        if (questions == null || questions.Length == 0)
        {
            Debug.LogWarning("[QuizMinigame] No questions available!");
            EndGame();
            return;
        }

        // Auto-build basic UI if inspector refs not provided
        EnsureUI();

        _currentQuestionIndex = 0;
        _correctAnswersCount = 0;
        _timeRemaining = quizDurationSeconds;
        _isActivelyRunning = true;

        if (quizPanel != null)
            quizPanel.SetActive(true);

        DisplayQuestion();
        Debug.Log("[QuizMinigame] Quiz started with " + questions.Length + " questions");
    }

    /// <summary>
    /// Called when the quiz ends. Calculates final reward based on correct answers.
    /// </summary>
    protected override void OnEndGame()
    {
        _isActivelyRunning = false;

        if (quizPanel != null)
            quizPanel.SetActive(false);

        Debug.Log($"[QuizMinigame] Quiz ended. Correct answers: {_correctAnswersCount}/{questions.Length}");
    }

    /// <summary>
    /// Called per frame to update the quiz timer.
    /// </summary>
    private void Update()
    {
        if (!IsRunning || !_isActivelyRunning)
            return;

        _timeRemaining -= Time.deltaTime;

        // Update timer display
        if (timerDisplay != null)
        {
            timerDisplay.text = "Time: " + Mathf.Max(0, _timeRemaining).ToString("F1") + "s";
        }

        // Auto-end quiz when time runs out
        if (_timeRemaining <= 0)
        {
            _timeRemaining = 0;
            EndGame();
        }
    }

    /// <summary>
    /// Displays the current question and its answer options.
    /// </summary>
    private void DisplayQuestion()
    {
        if (_currentQuestionIndex >= questions.Length)
        {
            Debug.LogWarning("[QuizMinigame] No more questions!");
            EndGame();
            return;
        }

        Question currentQuestion = questions[_currentQuestionIndex];

        // Update question text
        if (questionDisplay != null)
        {
            questionDisplay.text = currentQuestion.questionText;
        }

        // Update answer buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < currentQuestion.answers.Length && answerButtons[i] != null)
            {
                TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = currentQuestion.answers[i];
                }

                // Assign the correct answer handler
                int answerIndex = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => SubmitAnswer(answerIndex));
            }
        }

        // Update score display
        if (scoreDisplay != null)
        {
            scoreDisplay.text = "Correct: " + _correctAnswersCount + "/" + questions.Length;
        }

        Debug.Log($"[QuizMinigame] Displaying question {_currentQuestionIndex + 1}/{questions.Length}");
    }

    /// <summary>
    /// Called when a player submits an answer.
    /// </summary>
    /// <param name="answerIndex">Index of the selected answer (0-3).</param>
    public void SubmitAnswer(int answerIndex)
    {
        if (!IsRunning || !_isActivelyRunning || _currentQuestionIndex >= questions.Length)
            return;

        Question currentQuestion = questions[_currentQuestionIndex];

        // Check if answer is correct
        if (answerIndex == currentQuestion.correctAnswerIndex)
        {
            _correctAnswersCount++;
            Debug.Log($"[QuizMinigame] Correct! Score: {_correctAnswersCount}");
        }
        else
        {
            Debug.Log($"[QuizMinigame] Wrong answer. Correct was: {currentQuestion.correctAnswerIndex}");
        }

        // Move to next question
        _currentQuestionIndex++;

        if (_currentQuestionIndex < questions.Length)
        {
            DisplayQuestion();
        }
        else
        {
            // All questions answered
            Debug.Log("[QuizMinigame] All questions answered!");
            EndGame();
        }
    }

    /// <summary>
    /// Override to provide reward balance based on correct answers.
    /// </summary>
    protected override int GetRewardBalance()
    {
        if (questions.Length == 0)
            return 0;

        int totalReward = 0;
        for (int i = 0; i < Mathf.Min(_correctAnswersCount, questions.Length); i++)
        {
            totalReward += questions[i].rewardPerCorrect;
        }

        Debug.Log($"[QuizMinigame] Total reward: {totalReward} ({_correctAnswersCount} correct answers)");
        return totalReward;
    }

    /// <summary>
    /// Override to provide the start cost.
    /// </summary>
    protected override int GetStartCost()
    {
        return startCost;
    }

    /// <summary>
    /// Override to allow negative balance when paying start cost.
    /// </summary>
    protected override bool AllowNegativeBalanceOnStart()
    {
        return false;
    }

    private void Awake()
    {
        if (questions == null || questions.Length == 0)
        {
            PopulateSampleQuestions();
        }
    }

    private void EnsureUI()
    {
        // If user already assigned all refs, nothing to do
        if (questionDisplay != null && answerButtons != null && answerButtons.Length >= 4 && timerDisplay != null && scoreDisplay != null && quizPanel != null)
            return;

        // Create runtime canvas root
        if (_runtimeCanvasRoot == null)
        {
            _runtimeCanvasRoot = new GameObject("QuizCanvas_Runtime");
            var canvas = _runtimeCanvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _runtimeCanvasRoot.AddComponent<CanvasScaler>();
            _runtimeCanvasRoot.AddComponent<GraphicRaycaster>();

            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        // Panel
        if (quizPanel == null)
        {
            quizPanel = new GameObject("QuizPanel_Runtime");
            quizPanel.transform.SetParent(_runtimeCanvasRoot.transform, false);
            var prt = quizPanel.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.5f);
            prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(700, 420);
            var img = quizPanel.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.8f);
        }

        // Question text
        if (questionDisplay == null)
        {
            var qObj = new GameObject("QuestionText");
            qObj.transform.SetParent(quizPanel.transform, false);
            var rt = qObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -20);
            rt.sizeDelta = new Vector2(640, 120);
            var qText = qObj.AddComponent<TextMeshProUGUI>();
            qText.fontSize = 22;
            qText.alignment = TextAlignmentOptions.TopLeft;
            questionDisplay = qText;
        }

        // Timer
        if (timerDisplay == null)
        {
            var tObj = new GameObject("TimerText");
            tObj.transform.SetParent(quizPanel.transform, false);
            var rt = tObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-10, -10);
            rt.sizeDelta = new Vector2(140, 30);
            var tText = tObj.AddComponent<TextMeshProUGUI>();
            tText.fontSize = 18;
            tText.alignment = TextAlignmentOptions.TopRight;
            timerDisplay = tText;
        }

        // Score
        if (scoreDisplay == null)
        {
            var sObj = new GameObject("ScoreText");
            sObj.transform.SetParent(quizPanel.transform, false);
            var rt = sObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10, -10);
            rt.sizeDelta = new Vector2(200, 30);
            var sText = sObj.AddComponent<TextMeshProUGUI>();
            sText.fontSize = 18;
            sText.alignment = TextAlignmentOptions.TopLeft;
            scoreDisplay = sText;
        }

        // Answer buttons
        if (answerButtons == null || answerButtons.Length < 4)
        {
            var container = new GameObject("AnswerButtons");
            container.transform.SetParent(quizPanel.transform, false);
            var crt = container.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0.5f);
            crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.anchoredPosition = new Vector2(0, -40);
            crt.sizeDelta = new Vector2(640, 260);
            var layout = container.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;

            answerButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                var b = new GameObject($"AnswerButton_{i + 1}");
                b.transform.SetParent(container.transform, false);
                var brt = b.AddComponent<RectTransform>();
                brt.sizeDelta = new Vector2(620, 46);
                var bg = b.AddComponent<Image>();
                bg.color = new Color(1f, 1f, 1f, 0.95f);
                var btn = b.AddComponent<Button>();

                var txt = new GameObject("Text");
                txt.transform.SetParent(b.transform, false);
                var trt = txt.AddComponent<RectTransform>();
                trt.anchorMin = new Vector2(0, 0);
                trt.anchorMax = new Vector2(1, 1);
                trt.sizeDelta = Vector2.zero;
                var tcomp = txt.AddComponent<TextMeshProUGUI>();
                tcomp.fontSize = 20;
                tcomp.alignment = TextAlignmentOptions.Center;
                tcomp.text = "Answer " + (i + 1);

                int idx = i;
                btn.onClick.AddListener(() => SubmitAnswer(idx));
                answerButtons[i] = btn;
            }
        }

        if (quizPanel != null)
            quizPanel.SetActive(false);
    }

    private void PopulateSampleQuestions()
    {
        questions = new Question[2];
        questions[0] = new Question
        {
            questionText = "Jaka jest stolica Polski?",
            answers = new string[] { "Kraków", "Warszawa", "Gdańsk", "Wrocław" },
            correctAnswerIndex = 1,
            rewardPerCorrect = 100
        };
        questions[1] = new Question
        {
            questionText = "2 + 2 = ?",
            answers = new string[] { "3", "4", "5", "6" },
            correctAnswerIndex = 1,
            rewardPerCorrect = 50
        };
=======

public class QuizMinigame : MinigameBase
{
    // Handles Question/Answer logic
    public void AskQuestion()
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }

    public void SubmitAnswer(int answerIndex)
    {
        // TODO: Logic
        throw new System.NotImplementedException();
>>>>>>> c41e47bb5f2e86e1a0411a514d0da0bb22320e71
    }
}
