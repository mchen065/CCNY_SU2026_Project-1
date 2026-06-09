using UnityEngine;

public class Coin : MonoBehaviour
{
    // The two kinds of collectible coins.
    public enum CoinType
    {
        Normal,
        SpeedBoost
    }

    [Header("Coin Settings")]

    // Select Normal or Speed Boost in the Inspector.
    [SerializeField] private CoinType coinType = CoinType.Normal;

    // Number of points the coin gives.
    [SerializeField] private int scoreValue = 1;

    [Header("Speed Coin Settings")]

    // 2 means the duck moves at double speed.
    [SerializeField] private float speedMultiplier = 2f;

    // Number of seconds the boost lasts.
    [SerializeField] private float boostDuration = 4f;

    // Prevents one coin from being collected multiple times.
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Find the Rigidbody2D connected to the collider.
        Rigidbody2D playerBody = other.attachedRigidbody;

        // Ignore objects without a Rigidbody2D.
        if (playerBody == null)
        {
            return;
        }

        // Only allow an object tagged Player to collect the coin.
        if (!playerBody.gameObject.CompareTag("Player"))
        {
            return;
        }

        // Prevent duplicate collection.
        if (collected)
        {
            return;
        }

        collected = true;

        // Add the coin's score value.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        // Apply a temporary speed boost when this is a special coin.
        if (coinType == CoinType.SpeedBoost)
        {
            PlayerMovement movement =
                playerBody.GetComponent<PlayerMovement>();

            if (movement != null)
            {
                movement.ApplySpeedBoost(
                    speedMultiplier,
                    boostDuration
                );
            }
        }

        // Remove the collected coin from the scene.
        Destroy(gameObject);
    }
}