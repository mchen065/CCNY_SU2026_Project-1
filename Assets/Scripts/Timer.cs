using UnityEngine;

public class Timer : MonoBehaviour
{
    public float timer = 3f;
    public float gameTimer = 60f;
    public GameObject myObject;
    public GameObject myPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            Vector3 pos = new Vector3(Random.Range(-8f, 8f),
                                      Random.Range(-4f, 4f), 0);
            Instantiate(myObject, pos, Quaternion.identity);
            timer = 3f;
        }

        if(gameTimer > 0)
        {
            gameTimer -= Time.deltaTime;    
            if(gameTimer <= 0) 
            { 
                myPlayer.SetActive(false);
                Debug.Log("GAME OVER");
            }
        }
    }
}
