using UnityEngine;

public class PlayerWASD : MonoBehaviour
{
    public static PlayerWASD THEplayer;
    //for a basic WASD we'll need speed, and movement keys
    public float speed;
    public float jumpforce = 5f;
    public KeyCode LeftKey = KeyCode.A;
    public KeyCode RightKey = KeyCode.D;
    public float score;
    public float jumpTimer = 0;
    public bool grounded;
    //we also need the transform of the player
    //this is data that is accesible from inside : MonoBehaviour
    Transform pT; //we can declare it as pT just to show how it works
    PlayerWASD myScript; //we can also declare our own script here
    Rigidbody2D myRB;
    SpriteRenderer mySprite;

    void Start()
    {
        //this is a keyword that describes the space or scope of the script itself
        myScript = this;
        //gameobject is a property of all MonoBehaviour classes
        //transform is a propert of all GameObjects
        pT = this.gameObject.transform;



        //myRB = this.gameObject.GetComponent<Rigidbody2D>();
        //myRB = gameObject.GetComponent<Rigidbody2D>();
        myRB = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void OnCollisionStay2D(Collision2D collision)
    {
        grounded = true;
    }

    //function for jumping
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpTimer = .1f;
        }
        if (jumpTimer >= 0f)
        {
            jumpTimer -= Time.deltaTime;
        }
    }
    void FixedUpdate()
    {
        //ConditionalMoveExample();
        Vector3 dir = Direction();
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            myRB.linearVelocity = dir * speed * Time.fixedDeltaTime;
        }
        if (jumpTimer > 0f && grounded)
        {
            jump(jumpforce);
            jumpTimer = 0f;
        }


    }

    //simple directional calculation
    Vector3 Direction()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        return new Vector3(h, v, 0);
    }

    void jump(float jumpforce)
    {
        myRB.AddForce(Vector3.up * jumpforce);
    }

    //this is an example of using basic IF/else statements to move a player back and forth
    void ConditionalMoveExample()
    {
        if (Input.GetKey(LeftKey))
        {
            //pT.position -= Vector3.right * speed * Time.deltaTime;
            myRB.linearVelocity = Vector3.right * -speed * Time.deltaTime;
        }

        if (Input.GetKey(RightKey))
        {
            //pT.position += Vector3.right * speed * Time.deltaTime;
            myRB.linearVelocity = Vector3.right * speed * Time.deltaTime;
        }

        if (!Input.GetKey(LeftKey) && !Input.GetKey(RightKey))
        {
            myRB.linearVelocity = Vector3.zero;
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        //all collision2D data type have a property .gameobject 
        Debug.Log("collided with" + collision.gameObject);
        if (collision.gameObject.tag == "Collectible")
        {
            Destroy(collision.gameObject);
            score++;

        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        grounded = false;
    }
    public void AddScore(float s)
    {
        score += s;
    }
}


