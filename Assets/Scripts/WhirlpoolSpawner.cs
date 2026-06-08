using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhirlpoolSpawner : MonoBehaviour
{
    [Header("Required Objects")]
    [SerializeField] private GameObject whirlpoolPrefab;
    [SerializeField] private Camera gameplayCamera;

    [Header("River Limits")]
    [SerializeField] private float riverBottomY = -2f;
    [SerializeField] private float riverTopY = 1f;

    [Header("Visible Spawn Area")]
    [SerializeField] private float edgePadding = 1f;
    [SerializeField] private float whirlpoolZ = -1f;

    [Header("Spawn Timing")]
    [SerializeField] private float minimumSpawnDelay = 2f;
    [SerializeField] private float maximumSpawnDelay = 4f;

    [Header("Limits")]
    [SerializeField] private int startingWhirlpools = 2;
    [SerializeField] private int maximumWhirlpools = 6;
    [SerializeField] private float minimumSpacing = 2f;
    [SerializeField] private float despawnPadding = 5f;

    private readonly List<GameObject> activeWhirlpools =
        new List<GameObject>();

    private void Awake()
    {
        if (gameplayCamera == null)
        {
            gameplayCamera = Camera.main;
        }
    }

    private IEnumerator Start()
    {
        // Allows the camera to reach its starting position first.
        yield return null;

        for (int i = 0; i < startingWhirlpools; i++)
        {
            TrySpawnWhirlpool();
        }

        while (true)
        {
            float delay = Random.Range(
                minimumSpawnDelay,
                maximumSpawnDelay
            );

            yield return new WaitForSeconds(delay);

            RemoveOldWhirlpools();

            if (GameManager.Instance != null &&
                GameManager.Instance.GameEnded)
            {
                continue;
            }

            if (activeWhirlpools.Count < maximumWhirlpools)
            {
                TrySpawnWhirlpool();
            }
        }
    }

    private void TrySpawnWhirlpool()
    {
        if (whirlpoolPrefab == null)
        {
            Debug.LogError(
                "Whirlpool Prefab has not been assigned.",
                this
            );

            return;
        }

        if (gameplayCamera == null)
        {
            Debug.LogError(
                "Gameplay Camera has not been assigned.",
                this
            );

            return;
        }

        float halfCameraWidth =
            gameplayCamera.orthographicSize *
            gameplayCamera.aspect;

        float minimumX =
            gameplayCamera.transform.position.x -
            halfCameraWidth +
            edgePadding;

        float maximumX =
            gameplayCamera.transform.position.x +
            halfCameraWidth -
            edgePadding;

        for (int attempt = 0; attempt < 20; attempt++)
        {
            float randomX = Random.Range(minimumX, maximumX);
            float randomY = Random.Range(
                riverBottomY,
                riverTopY
            );

            Vector3 spawnPosition = new Vector3(
                randomX,
                randomY,
                whirlpoolZ
            );

            if (IsTooClose(spawnPosition))
            {
                continue;
            }

            GameObject newWhirlpool = Instantiate(
                whirlpoolPrefab,
                spawnPosition,
                Quaternion.identity
            );

            newWhirlpool.SetActive(true);

            // Force every particle system in the prefab to play.
            ParticleSystem[] particleSystems =
                newWhirlpool.GetComponentsInChildren<ParticleSystem>(
                    true
                );

            foreach (ParticleSystem particles in particleSystems)
            {
                particles.gameObject.SetActive(true);
                particles.Clear(true);
                particles.Play(true);
            }

            activeWhirlpools.Add(newWhirlpool);

            Debug.Log(
                $"Whirlpool spawned at {spawnPosition}",
                newWhirlpool
            );

            return;
        }
    }

    private bool IsTooClose(Vector3 newPosition)
    {
        foreach (GameObject whirlpool in activeWhirlpools)
        {
            if (whirlpool == null)
            {
                continue;
            }

            float distance = Vector2.Distance(
                newPosition,
                whirlpool.transform.position
            );

            if (distance < minimumSpacing)
            {
                return true;
            }
        }

        return false;
    }

    private void RemoveOldWhirlpools()
    {
        float halfCameraWidth =
            gameplayCamera.orthographicSize *
            gameplayCamera.aspect;

        float leftLimit =
            gameplayCamera.transform.position.x -
            halfCameraWidth -
            despawnPadding;

        float rightLimit =
            gameplayCamera.transform.position.x +
            halfCameraWidth +
            despawnPadding;

        for (int i = activeWhirlpools.Count - 1; i >= 0; i--)
        {
            GameObject whirlpool = activeWhirlpools[i];

            if (whirlpool == null)
            {
                activeWhirlpools.RemoveAt(i);
                continue;
            }

            float whirlpoolX = whirlpool.transform.position.x;

            if (whirlpoolX < leftLimit ||
                whirlpoolX > rightLimit)
            {
                Destroy(whirlpool);
                activeWhirlpools.RemoveAt(i);
            }
        }
    }
}