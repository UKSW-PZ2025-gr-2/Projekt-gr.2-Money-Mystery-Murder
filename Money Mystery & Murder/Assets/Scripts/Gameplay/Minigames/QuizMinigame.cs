using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class MafiaQuizMinigame : MinigameBase
{
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] answers;
        public int correctIndex;
    }

    [Header("Quiz Settings")]
    [SerializeField] private List<Question> questionPool = new();
    [SerializeField] private int questionsPerGame = 5;
    [SerializeField] private int rewardPerCorrectAnswer = 50;

    [Header("UI Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);
    [SerializeField] private float answerSpacing = 1f;
    [SerializeField] private Color questionColor = Color.black;
    [SerializeField] private Color answerColor = Color.black;
    [SerializeField] private int questionFontSize = 64;
    [SerializeField] private int answerFontSize = 48;
    [SerializeField] private Color backgroundColor = new Color(1, 1, 1, 0.5f);
    [SerializeField] private Vector2 backgroundSize = new Vector2(8, 6);

    private List<Question> _currentQuestions = new();
    private int _currentIndex;
    private int _score;
    private bool _gameRunning;

    private TextMesh _questionText;
    private List<TextMesh> _answerTexts = new();
    private GameObject _background;
    private TextMesh _scoreText;

    void Awake()
    {
        if (questionPool.Count == 0)
        {
            questionPool = new List<Question>
            {
                new Question { questionText = "Kto rządzi mafią?", answers = new string[]{ "Don", "Soldier", "Informant", "Policjant" }, correctIndex = 0 },
                new Question { questionText = "Co to jest tajemnicza sprawa?", answers = new string[]{ "Napad", "Porwanie", "Zadanie od Don’a", "Pranie pieniędzy" }, correctIndex = 2 },
                new Question { questionText = "Jak zdobyć pieniądze w mafii?", answers = new string[]{ "Kradzież", "Handel narkotykami", "Legalna praca", "Zakłady" }, correctIndex = 1 },
                new Question { questionText = "Kto jest moderem gry?", answers = new string[]{ "Don", "Policjant", "Admin", "Soldier" }, correctIndex = 2 },
                new Question { questionText = "Co grozi za zdradę mafii?", answers = new string[]{ "Nagroda", "Ucieczka", "Kara śmierci", "Awans" }, correctIndex = 2 },
                new Question { questionText = "Gdzie trzyma się dowody?", answers = new string[]{ "W sejfie", "Na ulicy", "W lodówce", "W samochodzie" }, correctIndex = 0 },
                new Question { questionText = "Co to jest 'zadanie tajemnicze'?", answers = new string[]{ "Rozwiązanie zagadki", "Krótka misja", "Spotkanie z Donem", "Porwanie" }, correctIndex = 1 },
                new Question { questionText = "Kto odpowiada za kasę mafii?", answers = new string[]{ "Don", "Księgowy", "Soldier", "Policjant" }, correctIndex = 1 },
                new Question { questionText = "Jakie narzędzie używa mafioso?", answers = new string[]{ "Maczuga", "Laptop", "Pistolet", "Samochód" }, correctIndex = 2 },
                new Question { questionText = "Co zrobić z wrogim gangiem?", answers = new string[]{ "Negocjacje", "Zabić", "Ignorować", "Zgłosić policji" }, correctIndex = 1 },
                new Question { questionText = "Kto może wtrącać się w sprawy Don’a?", answers = new string[]{ "Policja", "Admin", "Inny Don", "Nikt" }, correctIndex = 3 },
                new Question { questionText = "Co to jest informator?", answers = new string[]{ "Zdrajca", "Pomocnik", "Sprzymierzeniec", "Przeciwnik" }, correctIndex = 0 },
                new Question { questionText = "Gdzie trzyma się broń?", answers = new string[]{ "W sejfie", "W samochodzie", "U Don’a", "W magazynie" }, correctIndex = 0 },
                new Question { questionText = "Co robi mafioso w nocy?", answers = new string[]{ "Śpi", "Planuje akcję", "Jest w pracy", "Spotyka się z policją" }, correctIndex = 1 },
                new Question { questionText = "Co to są pieniądze mafii?", answers = new string[]{ "Gotówka", "Złoto", "Kryptowaluty", "Wszystko powyżej" }, correctIndex = 3 }
            };
        }

        BuildUI();
        ResetUI();
    }

    protected override void OnStartGame()
    {
        _gameRunning = true;
        _currentIndex = 0;
        _score = 0;
        _currentQuestions.Clear();

        var tempPool = new List<Question>(questionPool);
        for (int i = 0; i < questionsPerGame; i++)
        {
            if (tempPool.Count == 0) break;
            int idx = Random.Range(0, tempPool.Count);
            _currentQuestions.Add(tempPool[idx]);
            tempPool.RemoveAt(idx);
        }

        ShowQuestion(_currentIndex);

        _background.SetActive(true);
        _questionText.gameObject.SetActive(true);
        foreach (var t in _answerTexts) t.gameObject.SetActive(true);
        _scoreText.gameObject.SetActive(true);
    }

    private void BuildUI()
    {
        if (_background == null)
        {
            _background = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _background.name = "QuizBackground";
            _background.transform.SetParent(transform, false);
            _background.transform.localScale = new Vector3(backgroundSize.x, backgroundSize.y, 1);
            var renderer = _background.GetComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Unlit/Color"));
            renderer.material.color = backgroundColor;
            _background.SetActive(false);
        }

        if (_questionText == null)
        {
            GameObject qObj = new GameObject("QuestionText");
            qObj.transform.SetParent(transform, false);
            _questionText = qObj.AddComponent<TextMesh>();
            _questionText.fontSize = questionFontSize;
            _questionText.characterSize = 0.1f;
            _questionText.color = questionColor;
            _questionText.alignment = TextAlignment.Center;
            _questionText.anchor = TextAnchor.MiddleCenter;
            _questionText.gameObject.SetActive(false);
        }

        if (_answerTexts.Count == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject aObj = new GameObject("Answer_" + i);
                aObj.transform.SetParent(transform, false);
                var txt = aObj.AddComponent<TextMesh>();
                txt.fontSize = answerFontSize;
                txt.characterSize = 0.1f;
                txt.color = answerColor;
                txt.alignment = TextAlignment.Center;
                txt.anchor = TextAnchor.MiddleCenter;
                txt.gameObject.SetActive(false);
                _answerTexts.Add(txt);
            }
        }

        if (_scoreText == null)
        {
            GameObject sObj = new GameObject("ScoreText");
            sObj.transform.SetParent(transform, false);
            _scoreText = sObj.AddComponent<TextMesh>();
            _scoreText.fontSize = 48;
            _scoreText.characterSize = 0.1f;
            _scoreText.color = Color.green;
            _scoreText.alignment = TextAlignment.Center;
            _scoreText.anchor = TextAnchor.MiddleCenter;
            _scoreText.gameObject.SetActive(false);
        }
    }

    private void ShowQuestion(int index)
    {
        if (index >= _currentQuestions.Count)
        {
            ShowEndMessage(true);
            return;
        }

        var q = _currentQuestions[index];

        Vector3 basePos = player.position + offset;
        _questionText.transform.position = basePos;
        _questionText.text = q.questionText;

        for (int i = 0; i < 4; i++)
        {
            _answerTexts[i].transform.position = basePos + new Vector3(0, -answerSpacing * (i + 1), 0);
            _answerTexts[i].text = $"{(char)('A' + i)}) {q.answers[i]}";
        }

        _background.transform.position = basePos - new Vector3(0, backgroundSize.y / 2 - 0.5f, 0);
        _scoreText.transform.position = basePos + new Vector3(0, backgroundSize.y / 2 + 0.5f, 0);
        _scoreText.text = $"Punkty: {_score}";
    }

    void Update()
    {
        if (!_gameRunning) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.aKey.wasPressedThisFrame) Answer(0);
        if (Keyboard.current.bKey.wasPressedThisFrame) Answer(1);
        if (Keyboard.current.cKey.wasPressedThisFrame) Answer(2);
        if (Keyboard.current.dKey.wasPressedThisFrame) Answer(3);
    }

    private void Answer(int idx)
    {
        if (!_gameRunning) return;

        var q = _currentQuestions[_currentIndex];
        if (idx == q.correctIndex)
        {
            _score++;
            
            if (ActivatingPlayer != null)
                ActivatingPlayer.AddBalance(rewardPerCorrectAnswer);
            
            _currentIndex++;
            if (_currentIndex >= _currentQuestions.Count)
            {
                ShowEndMessage(true);
                return;
            }
            ShowQuestion(_currentIndex);
        }
        else
        {
            ShowEndMessage(false);
        }
    }

    private void ShowEndMessage(bool win)
    {
        _gameRunning = false;

        _questionText.gameObject.SetActive(false);
        foreach (var t in _answerTexts) t.gameObject.SetActive(false);

        _scoreText.text = win ? $"WYGRANA! Punkty: {_score}" : $"PRZEGRAŁEŚ! Punkty: {_score}";
        _scoreText.color = win ? Color.green : Color.black;
        _scoreText.transform.position = player.position + offset;
        _scoreText.gameObject.SetActive(true);

        _background.SetActive(false);

        StartCoroutine(EndGameAfterDelay(3f));
    }

    private IEnumerator EndGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetUI();
        // Call base EndGame to restore player movement and shooting
        base.EndGame();
    }

    private void ResetUI()
    {
        _questionText.gameObject.SetActive(false);
        foreach (var t in _answerTexts) t.gameObject.SetActive(false);
        _scoreText.gameObject.SetActive(false);
        _background.SetActive(false);

        _currentQuestions.Clear();
        _currentIndex = 0;
        _score = 0;
        _gameRunning = false;
    }
}
