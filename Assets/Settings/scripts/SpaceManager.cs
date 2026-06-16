using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceGameManager : MonoBehaviour
{
    public static SpaceGameManager Instance;

    public float gameTime = 60f;
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI resultText;

    private float timeRemaining;
    AstronautMovement playerOne;
    Rigidbody2D playerRB;

    public bool GameEnded { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        timeRemaining = gameTime;
        GameEnded = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (GameEnded) { playerOne.enabled = false; playerRB.linearVelocity = Vector3.zero; return; }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining < 0)
            timeRemaining = 0;

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        if (timeRemaining <= 0)
            LoseGame();
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
        if (GameEnded) return;

        GameEnded = true;

        if (resultText != null)
            resultText.text = message;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}