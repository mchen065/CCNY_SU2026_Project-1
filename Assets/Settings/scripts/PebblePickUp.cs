using UnityEngine;

public class PebblePickup : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float destroyX = -20f;

    private void Update()
    {
        transform.position +=
            Vector3.left * moveSpeed * Time.deltaTime;

        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
}