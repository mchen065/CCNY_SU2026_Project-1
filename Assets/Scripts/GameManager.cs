using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text finalScoreText;

    [Header("Timer UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float gameLengthSeconds = 60f;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Score Settings")]
    [Tooltip("Turn this off if the score should never go below zero.")]
    [SerializeField] private bool allowNegativeScore = false;

    private int score;
    private float timeRemaining;

    public int Score => score;
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

        // Hide the entire game-over screen at the start.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Also hide the final score text directly, just in case
        // it is not correctly parented under the panel.
        if (finalScoreText != null)
        {
            finalScoreText.gameObject.SetActive(false);
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
        timeRemaining = Mathf.Max(0f, timeRemaining);

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

        if (!allowNegativeScore)
        {
            score = Mathf.Max(0, score);
        }

        UpdateScoreText();
    }

    public void SubtractScore(int amount)
    {
        AddScore(-Mathf.Abs(amount));
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        int displayedSeconds = Mathf.CeilToInt(timeRemaining);
        int minutes = displayedSeconds / 60;
        int seconds = displayedSeconds % 60;

        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void EndGame()
    {
        if (GameEnded)
        {
            return;
        }

        GameEnded = true;
        timeRemaining = 0f;

        UpdateTimerText();

        // Update and display the final score.
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {score}";
            finalScoreText.gameObject.SetActive(true);
        }

        // Display the entire game-over screen.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}