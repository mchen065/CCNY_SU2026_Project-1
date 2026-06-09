using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Allows other scripts to find the GameManager easily.
    public static GameManager Instance { get; private set; }

    [Header("Score UI")]

    // Displays the score during gameplay.
    [SerializeField] private TMP_Text scoreText;

    // Displays the final score after the timer ends.
    [SerializeField] private TMP_Text finalScoreText;

    [Header("Timer UI")]

    // Displays the countdown timer.
    [SerializeField] private TMP_Text timerText;

    // Total number of seconds in one game.
    [SerializeField] private float gameLengthSeconds = 60f;

    [Header("Game Over")]

    // The panel containing Game Over, Final Score,
    // and the restart button.
    [SerializeField] private GameObject gameOverPanel;

    [Header("Score Settings")]

    // Turn this on if whirlpools are allowed to make
    // the score go below zero.
    [SerializeField] private bool allowNegativeScore = true;

    // Stores the player's current score.
    private int score;

    // Stores the remaining game time.
    private float timeRemaining;

    // Other scripts can read the score but cannot directly change it.
    public int Score => score;

    // Other scripts can check whether the game has ended.
    public bool GameEnded { get; private set; }

    private void Awake()
    {
        // Prevent more than one GameManager from existing.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Ensure the game is not still paused after restarting.
        Time.timeScale = 1f;

        // Reset score and timer.
        score = 0;
        timeRemaining = gameLengthSeconds;
        GameEnded = false;

        // Hide the Game Over screen at the beginning.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Hide Final Score until the game ends.
        if (finalScoreText != null)
        {
            finalScoreText.gameObject.SetActive(false);
        }

        UpdateScoreText();
        UpdateTimerText();
    }

    private void Update()
    {
        // Stop updating the timer after the game ends.
        if (GameEnded)
        {
            return;
        }

        // Remove the amount of time that passed during this frame.
        timeRemaining -= Time.deltaTime;

        // Prevent the timer from becoming negative.
        timeRemaining = Mathf.Max(0f, timeRemaining);

        UpdateTimerText();

        // End the game when the timer reaches zero.
        if (timeRemaining <= 0f)
        {
            EndGame();
        }
    }

    public void AddScore(int amount)
    {
        // Do not change the score after Game Over.
        if (GameEnded)
        {
            return;
        }

        // Positive numbers add points.
        // Negative numbers remove points.
        score += amount;

        // Keep the score at zero or above if negative scores
        // are not allowed.
        if (!allowNegativeScore)
        {
            score = Mathf.Max(0, score);
        }

        UpdateScoreText();
    }

    public void SubtractScore(int amount)
    {
        // Mathf.Abs ensures the supplied number becomes positive,
        // and the minus sign changes it into a penalty.
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

        // Round upward so 0.8 seconds displays as 1 second.
        int totalSeconds = Mathf.CeilToInt(timeRemaining);

        // Convert total seconds into minutes and seconds.
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void EndGame()
    {
        // Prevent EndGame from running multiple times.
        if (GameEnded)
        {
            return;
        }

        GameEnded = true;
        timeRemaining = 0f;

        UpdateTimerText();

        // Change the final-score text to the player's real score.
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {score}";
            finalScoreText.gameObject.SetActive(true);
        }

        // Display the Game Over panel.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Pause gameplay and physics.
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        // Unpause the game before reloading the scene.
        Time.timeScale = 1f;

        // Reload the current scene.
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}