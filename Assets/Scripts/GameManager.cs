using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Game Timer")]
    [SerializeField] private float gameLengthSeconds = 60f;

    private int score;
    private float timeRemaining;

    public bool GameEnded { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        score = 0;
        timeRemaining = gameLengthSeconds;
        GameEnded = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateScoreText();
        UpdateTimerText();
    }

    private void Update()
    {
        if (GameEnded)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Max(timeRemaining, 0f);

        UpdateTimerText();

        if (timeRemaining <= 0f)
        {
            EndGame();
        }
    }

    public void AddScore(int amount)
    {
        if (GameEnded)
        {
            return;
        }

        score += amount;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(timeRemaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void EndGame()
    {
        GameEnded = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }
}