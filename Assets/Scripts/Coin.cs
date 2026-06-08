using UnityEngine;

public class Coin : MonoBehaviour
{
    public enum CoinType
    {
        Normal,
        SpeedBoost
    }

    [Header("Coin Settings")]
    [SerializeField] private CoinType coinType = CoinType.Normal;
    [SerializeField] private int scoreValue = 1;

    [Header("Speed Coin Settings")]
    [SerializeField] private float speedMultiplier = 2f;
    [SerializeField] private float boostDuration = 4f;

    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        collected = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        if (coinType == CoinType.SpeedBoost)
        {
            PlayerMovement movement =
                other.GetComponent<PlayerMovement>();

            if (movement != null)
            {
                movement.ApplySpeedBoost(
                    speedMultiplier,
                    boostDuration
                );
            }
        }

        Destroy(gameObject);
    }
}