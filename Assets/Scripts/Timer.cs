using UnityEngine;

public class Timer : MonoBehaviour
{
    public GameObject myObject;
    public float timer = 2f;
    public bool gameOn;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        gameOn = GameManager._instance.gameOn;
        timer -= Time.deltaTime;
        if (timer < 0 && gameOn)
        {
            Vector3 pos = new Vector3(Random.Range(-8f, 8f),
                                      Random.Range(-4f, 4f), 0);
            Instantiate(myObject, pos, Quaternion.identity);
            timer = 3f;
        }
    }
}
