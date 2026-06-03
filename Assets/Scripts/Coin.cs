using UnityEngine;

public class Coin : MonoBehaviour
{
    public float lifetime = 3f;
    public float value = 1f;
    public PlayerWASD targetPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetPlayer = PlayerWASD.THEplayer;
        GameManager._instance.allCoins.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime < 0f)
        {
            GameManager._instance.allCoins.Remove(this);
            Destroy(this.gameObject);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            PlayerWASD pS = collision.gameObject.GetComponent<PlayerWASD>();
            Debug.Log("we found the player");
            pS.AddScore(value);
            collision.gameObject.SendMessage("AddScore");
            Destroy(this.gameObject);
        }
    }
}
