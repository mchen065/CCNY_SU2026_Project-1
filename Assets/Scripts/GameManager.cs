using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;

    public float gameTimer = 60f;
    public TextMeshProUGUI scoreText;
    public GameObject myObject;
    public GameObject myPlayer;
    public TextMeshProUGUI gameTimeText;
    public PlayerWASD myWASD;
    Rigidbody2D playerRB;
    public bool gameOn;
    public List<Coin> allCoins;

    private void Awake()
    {
        _instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameOn = true;
        myPlayer = PlayerWASD.THEplayer.gameObject;

        myPlayer = myPlayer.GetComponent<PlayerWASD>();
        //PlayerScript=PlayerWASD.GetComponent<PlayerWASD>();
        playerScript = PlayerWASD.THEplayer;

        playerRB = myPlayer.GetComponent<Rigidbody2D>();


    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            Vector3 pos = new Vector3(Random.Range(-8f, 8f),
                                      Random.Range(-4f, 4f), 0);
            Instantiate(myObject, pos, Quaternion.identity);
            timer = 3f;
        }

        if (gameTimer > 0)
        {
            gameTimer -= Time.deltaTime;
            if (gameTimer <= 0)
            {
                myPlayer.SetActive(false);
                playerScript.enabled = false;
                playerRB.Addforce(Vector3.up * 100f);
                playerRB.AddTorque(5f);
                GameObject[] allCoins = GameObject.FindObjectsByType<Coin>(FindObjectsSortMode.None);
                foreach (Coin c in allCoins)
                {
                    Destroy(c.gameObject);

                }
                Debug.Log("GAME OVER");
                gameOn = false;


            }
        }
        gameTimeText.text = gameTimer.ToString("F1");
        scoreText.text = myWASD.score.ToString();
    }
}


