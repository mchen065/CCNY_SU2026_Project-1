using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Manages the astronaut game's timer, lives,
// win condition, lose condition, and Game Over screen.
public class SpaceGameManager : MonoBehaviour
{
    // Allows astronaut-game scripts to access this manager.
    public static SpaceGameManager Instance { get; private set; }

    [Header("Timer")]

    // The player has one minute to reach the spaceship.
    [SerializeField] private float gameLengthSeconds = 60f;

    // Text that displays the remaining time.
    [SerializeField] private TMP_Text timerText;


    [Header("Health")]

    // The astronaut begins with three lives.
    [SerializeField] private int startingLives = 3;

    // Drag Heart1, Heart2, and Heart3 into this array.
    [SerializeField] private GameObject[] heartIcons;


    [Header("Game Over UI")]

    // The panel displayed after winning or losing.
    [SerializeField] private GameObject gameOverPanel;

    // Text that displays the win or loss message.
    [SerializeField] private TMP_Text resultText;


    [Header("Starting Delay")]

    // Brief delay before the game begins.
    [SerializeField] private float startDelay = 0.75f;


    // Stores the remaining time.
    private float timeRemaining;

    // Stores the astronaut's remaining lives.
    private int currentLives;


    // Other scripts can check whether gameplay has begun.
    public bool GameStarted { get; private set; }

    // Other scripts can check whether the game has ended.
    public bool GameEnded { get; private set; }

    // Allows other scripts to read the current number of lives.
    public int CurrentLives => currentLives;


    private void Awake()
    {
        // Prevent multiple SpaceGameManagers from existing.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private IEnumerator Start()
    {
        // Unpause the game when the scene starts.
        Time.timeScale = 1f;

        // Reset timer and lives.
        timeRemaining = gameLengthSeconds;
        currentLives = startingLives;

        GameStarted = false;
        GameEnded = false;

        // Hide the Game Over screen at the beginning.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateTimerUI();
        UpdateHeartUI();

        // Keep the astronaut in its Idle state briefly.
        yield return new WaitForSeconds(startDelay);

        GameStarted = true;
    }


    private void Update()
    {
        // Do not count down before the game begins
        // or after the game ends.
        if (!GameStarted || GameEnded)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Max(0f, timeRemaining);

        UpdateTimerUI();

        // Running out of time causes a loss.
        if (timeRemaining <= 0f)
        {
            LoseGame("Time's Up!");
        }
    }


    // Called when the astronaut touches a meteorite
    // or gets hit by a monster's glowing orb.
    public void LoseLife()
    {
        if (GameEnded)
        {
            return;
        }

        currentLives--;
        currentLives = Mathf.Max(0, currentLives);

        UpdateHeartUI();

        // End the game when all three hearts are gone.
        if (currentLives <= 0)
        {
            LoseGame("All Lives Lost!");
        }
    }


    // Called when the astronaut reaches the spaceship.
    public void WinGame()
    {
        if (GameEnded)
        {
            return;
        }

        EndGame("You Reached the Spaceship!");
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

        // Pause gameplay, physics, enemies, and meteorites.
        Time.timeScale = 0f;
    }


    private void UpdateTimerUI()
    {
        if (timerText == null)
        {
            return;
        }

        int displayedTime = Mathf.CeilToInt(timeRemaining);

        int minutes = displayedTime / 60;
        int seconds = displayedTime % 60;

        timerText.text =
            $"Time: {minutes:00}:{seconds:00}";
    }


    private void UpdateHeartUI()
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] != null)
            {
                // Show one icon for every remaining life.
                heartIcons[i].SetActive(i < currentLives);
            }
        }
    }


    // Connect this method to the Restart button.
    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}