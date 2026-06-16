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

    // References to the astronaut.
    private AstronautMovement playerMovement;
    private Rigidbody2D playerRigidbody;

    public bool GameEnded { get; private set; }

    private void Awake()
    {
        // Make sure only one manager exists.
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

        timeRemaining = gameTime;
        GameEnded = false;

        // Find the astronaut automatically.
        playerMovement =
            FindFirstObjectByType<AstronautMovement>();

        if (playerMovement != null)
        {
            playerRigidbody =
                playerMovement.GetComponent<Rigidbody2D>();
        }
        else
        {
            Debug.LogError(
                "SpaceGameManager could not find AstronautMovement."
            );
        }

        // Hide the game-over screen at the start.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (resultText != null)
        {
            resultText.text = "";
        }

        UpdateTimerText();
    }

    private void Update()
    {
        // Do nothing after the game ends.
        if (GameEnded)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining < 0f)
        {
            timeRemaining = 0f;
        }

        UpdateTimerText();

        // The player loses when the timer reaches zero.
        if (timeRemaining <= 0f)
        {
            LoseGame();
        }
    }

    private void UpdateTimerText()
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
        EndGame("You Win!");
    }

    public void LoseGame()
    {
        EndGame("You Lose!");
    }

    private void EndGame(string message)
    {
        if (GameEnded)
        {
            return;
        }

        GameEnded = true;

        // Stop the astronaut safely.
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector2.zero;

            playerRigidbody.angularVelocity = 0f;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Show the result.
        if (resultText != null)
        {
            resultText.text = message;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Pause enemies, meteorites, and the timer.
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