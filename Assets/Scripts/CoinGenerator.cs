using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject normalCoinPrefab;
    [SerializeField] private GameObject speedCoinPrefab;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("River Area")]
    [SerializeField] private float riverBottomY = -2f;
    [SerializeField] private float riverTopY = 1f;

    [Header("Horizontal Spawn Distance")]
    [SerializeField] private float minimumDistance = 7f;
    [SerializeField] private float maximumDistance = 12f;

    [Header("Timing")]
    [SerializeField] private float minimumSpawnDelay = 1f;
    [SerializeField] private float maximumSpawnDelay = 2.5f;

    [Header("Special Coin")]
    [Range(0f, 1f)]
    [SerializeField] private float speedCoinChance = 0.15f;

    [Header("Limits")]
    [SerializeField] private int maximumCoins = 12;
    [SerializeField] private float despawnDistance = 30f;

    [Header("Rendering")]
    [SerializeField] private float coinZPosition = -2f;

    private readonly List<GameObject> spawnedCoins =
        new List<GameObject>();

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private IEnumerator Start()
    {
        while (true)
        {
            float delay = Random.Range(
                minimumSpawnDelay,
                maximumSpawnDelay
            );

            yield return new WaitForSeconds(delay);

            RemoveOldCoins();

            if (GameManager.Instance != null &&
                GameManager.Instance.GameEnded)
            {
                continue;
            }

            if (spawnedCoins.Count < maximumCoins)
            {
                SpawnCoin();
            }
        }
    }

    private void SpawnCoin()
    {
        if (normalCoinPrefab == null || cameraTransform == null)
        {
            return;
        }

        // Coins may appear on either side because the duck
        // can swim left or right.
        float side = Random.value < 0.5f ? -1f : 1f;

        float xDistance = Random.Range(
            minimumDistance,
            maximumDistance
        );

        float spawnX =
            cameraTransform.position.x + xDistance * side;

        float spawnY = Random.Range(
            riverBottomY,
            riverTopY
        );

        Vector3 spawnPosition = new Vector3(
            spawnX,
            spawnY,
            coinZPosition
        );

        bool createSpeedCoin =
            speedCoinPrefab != null &&
            Random.value < speedCoinChance;

        GameObject selectedPrefab = createSpeedCoin
            ? speedCoinPrefab
            : normalCoinPrefab;

        // No parent is assigned, so the coin does not inherit
        // movement from a parallax background.
        GameObject newCoin = Instantiate(
            selectedPrefab,
            spawnPosition,
            Quaternion.identity
        );

        spawnedCoins.Add(newCoin);
    }

    private void RemoveOldCoins()
    {
        for (int i = spawnedCoins.Count - 1; i >= 0; i--)
        {
            GameObject coin = spawnedCoins[i];

            if (coin == null)
            {
                spawnedCoins.RemoveAt(i);
                continue;
            }

            float distanceFromCamera = Mathf.Abs(
                coin.transform.position.x -
                cameraTransform.position.x
            );

            if (distanceFromCamera > despawnDistance)
            {
                Destroy(coin);
                spawnedCoins.RemoveAt(i);
            }
        }
    }
}