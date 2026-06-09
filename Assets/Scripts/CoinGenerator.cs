using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Prefabs")]

    // Regular coin prefab.
    [SerializeField] private GameObject normalCoinPrefab;

    // Special speed-boost coin prefab.
    [SerializeField] private GameObject speedCoinPrefab;

    [Header("Camera")]

    // Coins spawn around the camera because the camera follows the duck.
    [SerializeField] private Transform cameraTransform;

    [Header("River Area")]

    // The lowest Y position where coins may appear.
    [SerializeField] private float riverBottomY = -2f;

    // The highest Y position where coins may appear.
    [SerializeField] private float riverTopY = 1f;

    [Header("Horizontal Spawn Distance")]

    // Closest distance from the camera where a coin may spawn.
    [SerializeField] private float minimumDistance = 7f;

    // Farthest distance from the camera where a coin may spawn.
    [SerializeField] private float maximumDistance = 12f;

    [Header("Spawn Timing")]

    // Minimum delay between coin spawns.
    [SerializeField] private float minimumSpawnDelay = 1f;

    // Maximum delay between coin spawns.
    [SerializeField] private float maximumSpawnDelay = 2.5f;

    [Header("Special Coin")]

    // 0.15 means a 15% chance of creating a speed coin.
    [Range(0f, 1f)]
    [SerializeField] private float speedCoinChance = 0.15f;

    [Header("Limits")]

    // Maximum number of coins allowed at once.
    [SerializeField] private int maximumCoins = 12;

    // Coins farther than this distance are deleted.
    [SerializeField] private float despawnDistance = 30f;

    [Header("Rendering")]

    // Set this to the visible Z position used by your coins.
    [SerializeField] private float coinZPosition = -1f;

    // Keeps track of all generated coins.
    private readonly List<GameObject> spawnedCoins =
        new List<GameObject>();

    private void Awake()
    {
        // Automatically use the Main Camera when no camera
        // was manually assigned.
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private IEnumerator Start()
    {
        // Continue generating coins until the object is disabled.
        while (true)
        {
            // Choose a random delay.
            float delay = Random.Range(
                minimumSpawnDelay,
                maximumSpawnDelay
            );

            yield return new WaitForSeconds(delay);

            RemoveOldCoins();

            // Do not spawn more coins after Game Over.
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

        // Randomly choose the left or right side of the camera.
        float side = Random.value < 0.5f ? -1f : 1f;

        // Choose a random horizontal distance.
        float distance = Random.Range(
            minimumDistance,
            maximumDistance
        );

        float spawnX =
            cameraTransform.position.x + distance * side;

        // Choose a random vertical position inside the river.
        float spawnY = Random.Range(
            riverBottomY,
            riverTopY
        );

        Vector3 spawnPosition = new Vector3(
            spawnX,
            spawnY,
            coinZPosition
        );

        // Decide whether to spawn a normal or special coin.
        bool createSpeedCoin =
            speedCoinPrefab != null &&
            Random.value < speedCoinChance;

        GameObject selectedPrefab = createSpeedCoin
            ? speedCoinPrefab
            : normalCoinPrefab;

        // Create the selected coin in world space.
        // It is not parented to the parallax background.
        GameObject newCoin = Instantiate(
            selectedPrefab,
            spawnPosition,
            Quaternion.identity
        );

        spawnedCoins.Add(newCoin);
    }

    private void RemoveOldCoins()
    {
        // Loop backward so entries can safely be removed.
        for (int i = spawnedCoins.Count - 1; i >= 0; i--)
        {
            GameObject coin = spawnedCoins[i];

            // Remove empty list entries after coins are collected.
            if (coin == null)
            {
                spawnedCoins.RemoveAt(i);
                continue;
            }

            float distanceFromCamera = Mathf.Abs(
                coin.transform.position.x -
                cameraTransform.position.x
            );

            // Delete coins that are far away from the camera.
            if (distanceFromCamera > despawnDistance)
            {
                Destroy(coin);
                spawnedCoins.RemoveAt(i);
            }
        }
    }
}