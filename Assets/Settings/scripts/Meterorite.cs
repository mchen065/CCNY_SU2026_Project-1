using UnityEngine;

public class Meteorite : MonoBehaviour
{
    [Header("Movement")]

    // Speed of the meteorite.
    [SerializeField] private float moveSpeed = 4f;

    // Check this if the meteorite should move right.
    // Uncheck it if it should move left.
    [SerializeField] private bool moveRight = false;

    [Header("Pebble Drop")]

    // Collectible pebble prefab.
    [SerializeField] private GameObject pebblePickupPrefab;

    // 0.50 means a 50% chance.
    [Range(0f, 1f)]
    [SerializeField] private float pebbleChance = 0.50f;

    [Header("Destroy Position")]

    // Meteorite is removed after leaving the screen.
    [SerializeField] private float destroyX = -15f;

    private void Start()
    {
        // Give this meteorite a 50% chance
        // to create one collectible pebble.
        if (pebblePickupPrefab != null &&
            Random.value <= pebbleChance)
        {
            Instantiate(
                pebblePickupPrefab,
                transform.position,
                Quaternion.identity
            );
        }
    }

    private void Update()
    {
        // Choose the horizontal direction.
        float direction = moveRight ? 1f : -1f;

        // Move horizontally.
        transform.position +=
            Vector3.right *
            direction *
            moveSpeed *
            Time.deltaTime;

        // Destroy after leaving the screen.
        if (!moveRight && transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
        else if (moveRight && transform.position.x > -destroyX)
        {
            Destroy(gameObject);
        }
    }
}