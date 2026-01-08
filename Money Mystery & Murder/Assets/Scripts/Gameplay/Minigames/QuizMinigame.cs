using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class QuizMinigame : MinigameBase
{
    [System.Serializable]
    public class Question
    {
        [TextArea(2, 4)]
        public string questionText;
        public string[] answers = new string[4];
        public int correctAnswerIndex; // 0-3
        public int rewardPerCorrect = 100;
    }

    [Header("Quiz Settings")]
    [SerializeField] private int startCost = 0; // Bez kosztów na start
    [SerializeField] private int questionsPerGame = 5;
    [SerializeField] private Question[] allQuestions;

    [Header("UI References - Podłącz w Inspectorze")]
    [SerializeField] private TextMeshProUGUI titleText; // Tytuł quizu (opcjonalny)
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] answerButtons = new Button[4]; // 4 buttony z odpowiedziami
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject quizPanel;

    private List<Question> _quizQuestions;
    private int _currentQuestionIndex;
    private int _correctAnswers;

    // PRZYKŁADOWE 10 PYTAŃ - DODAJ DO INSPECTORA LUB ZOSTAW W KODZIE
    private void Awake()
    {
        if (allQuestions == null || allQuestions.Length == 0)
        {
            allQuestions = CreateSampleQuestions();
        }
    }

    private Question[] CreateSampleQuestions()
    {
        return new Question[]
        {
            new Question 
            { 
                questionText = "Ile wynosi 2 + 2?", 
                answers = {"3", "4", "5", "6"}, 
                correctAnswerIndex = 1 
            },
            new Question 
            { 
                questionText = "Jaka jest stolica Polski?", 
                answers = {"Kraków", "Warszawa", "Gdańsk", "Wrocław"}, 
                correctAnswerIndex = 1 
            },
            new Question 
            { 
                questionText = "Ile kontynentów jest na Ziemi?", 
                answers = {"5", "6", "7", "8"}, 
                correctAnswerIndex = 2 
            },
            new Question 
            { 
                questionText = "Jaki kolor ma niebo w słoneczny dzień?", 
                answers = {"Zielony", "Niebieski", "Czerwony", "Żółty"}, 
                correctAnswerIndex = 1 
            },
            new Question 
            { 
                questionText = "Jak nazywa się największa planeta Układu Słonecznego?", 
                answers = {"Ziemia", "Mars", "Jowisz", "Saturn"}, 
                correctAnswerIndex = 2 
            },
            new Question 
            { 
                questionText = "Ile stron ma kwadrat?", 
                answers = {"3", "4", "5", "6"}, 
                correctAnswerIndex = 1 
            },
            new Question 
            { 
                questionText = "Co to jest H2O?", 
                answers = {"Mleko", "Woda", "Sok", "Piwo"}, 
                correctAnswerIndex = 1 
            },
            new Question 
            { 
                questionText = "Jaki jest pierwiastek 36?", 
                answers = {"34", "35", "36", "37"}, 
                correctAnswerIndex = 2 
            },
            new Question 
            { 
                questionText = "Ile = 10 * 2?", 
                answers = {"15", "20", "25", "30"}, 
                correctAnswerIndex = 1 
            },
            new Question 
            { 
                questionText = "Kto wynalazł żarówkę?", 
                answers = {"Tesla", "Edison", "Newton", "Einstein"}, 
                correctAnswerIndex = 1 
            }
        };
    }

    protected override void OnStartGame()
    {
        if (allQuestions == null || allQuestions.Length < questionsPerGame)
        {
            Debug.LogError("[QuizMinigame] Za mało pytań w tablicy!");
            EndGame();
            return;
        }

        quizPanel.SetActive(true);
        if (titleText != null) titleText.text = "QUIZ MINIGRA";

        _correctAnswers = 0;
        _currentQuestionIndex = 0;

        PrepareQuestions();
        ShowQuestion();
    }

    protected override void OnEndGame()
    {
        quizPanel.SetActive(false);
        Debug.Log($"Quiz zakończony! Poprawne: {_correctAnswers}/{questionsPerGame}");
    }

    private void PrepareQuestions()
    {
        _quizQuestions = allQuestions.ToList();
        ShuffleList(_quizQuestions);
        _quizQuestions = _quizQuestions.Take(questionsPerGame).ToList();
    }

    private void ShowQuestion()
    {
        if (_currentQuestionIndex >= questionsPerGame)
        {
            EndGame();
            return;
        }

        Question q = _quizQuestions[_currentQuestionIndex];
        questionText.text = q.questionText;
        scoreText.text = $"Score: {_correctAnswers} / {_currentQuestionIndex}";

        // Ustaw odpowiedzi na buttonach
        for (int i = 0; i < answerButtons.Length && i < q.answers.Length; i++)
        {
            int capturedIndex = i; // Capture dla lambda
            TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = q.answers[i];
            }

            // Wyczyść poprzednie listenery
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => SubmitAnswer(capturedIndex));
        }
    }

    public void SubmitAnswer(int answerIndex)
    {
        Question q = _quizQuestions[_currentQuestionIndex];
        bool isCorrect = (answerIndex == q.correctAnswerIndex);

        if (isCorrect)
        {
            _correctAnswers++;
            Debug.Log("Poprawna odpowiedź!");
        }
        else
        {
            Debug.Log($"Błąd! Poprawna była: {q.answers[q.correctAnswerIndex]}");
        }

        _currentQuestionIndex++;
        ShowQuestion();
    }

    private void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    protected override int GetRewardBalance()
    {
        return _correctAnswers * 100; // 100 pkt za każdą poprawną
    }

    protected override int GetStartCost() => startCost;

    protected override bool AllowNegativeBalanceOnStart() => true;
}
