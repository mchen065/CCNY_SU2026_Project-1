using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceGameManager : MonoBehaviour
{
    public static SpaceGameManager Instance;

    [Header("Timer")]
    public float gameTime = 60f;
    public TextMeshProUGUI timerText;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI resultText;

    private float timeRemaining;

    public bool GameStarted { get; private set; }
    public bool GameEnded { get; private set; }

    private void Awake()
    {
        // Make sure there is only one SpaceGameManager.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Unpause the game when the scene starts.
        Time.timeScale = 1f;

        timeRemaining = gameTime;

        GameStarted = true;
        GameEnded = false;

        // Hide the game-over screen at the beginning.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateTimer();
    }

    private void Update()
    {
        if (!GameStarted || GameEnded)
        {
            return;
        }

        // Count down from one minute.
        timeRemaining -= Time.deltaTime;

        if (timeRemaining < 0f)
        {
            timeRemaining = 0f;
        }

        UpdateTimer();

        // Running out of time means losing.
        if (timeRemaining <= 0f)
        {
            LoseGame("Time's Up!");
        }
    }

    private void UpdateTimer()
    {
        if (timerText == null)
        {
            return;
        }

        int totalSeconds =
            Mathf.CeilToInt(timeRemaining);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text =
            $"{minutes:00}:{seconds:00}";
    }

    public void WinGame()
    {
        if (GameEnded)
        {
            return;
        }

        EndGame("You Win!");
    }

    public void LoseGame(string message)
    {
        if (GameEnded)
        {
            return;
        }

        EndGame(message);
    }

    private void EndGame(string message)
    {
        GameEnded = true;

        if (resultText != null)
        {
            resultText.text = message;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Pause movement, enemies, meteorites, and timer.
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}