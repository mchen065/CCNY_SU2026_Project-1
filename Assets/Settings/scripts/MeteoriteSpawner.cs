using UnityEngine;

public class MeteoriteSpawner : MonoBehaviour
{
    [Header("Meteorite Prefabs")]

    // Drag prefab assets from the PROJECT window here.
    // Do not drag meteorites from the Hierarchy.
    [SerializeField] private GameObject[] meteoritePrefabs;

    [Header("Spawn Timing")]
    [SerializeField] private float minimumSpawnDelay = 0.8f;
    [SerializeField] private float maximumSpawnDelay = 1.5f;

    [Header("Meteorites Per Wave")]
    [SerializeField] private int minimumMeteorites = 1;
    [SerializeField] private int maximumMeteorites = 3;

    [Header("Spawn Area")]
    [SerializeField] private float minimumY = -4f;
    [SerializeField] private float maximumY = 4f;

    [Header("Spacing")]
    [SerializeField] private float horizontalSpacing = 1.5f;

    private float timer;
    private float nextSpawnDelay;

    private void Start()
    {
        ChooseNextSpawnDelay();
    }

    private void Update()
    {
        if (SpaceGameManager.Instance != null &&
            SpaceGameManager.Instance.GameEnded)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= nextSpawnDelay)
        {
            SpawnMeteoriteWave();

            timer = 0f;
            ChooseNextSpawnDelay();
        }
    }

    private void SpawnMeteoriteWave()
    {
        if (meteoritePrefabs == null ||
            meteoritePrefabs.Length == 0)
        {
            Debug.LogError(
                "No meteorite prefabs are assigned.",
                this
            );

            return;
        }

        int amount = Random.Range(
            minimumMeteorites,
            maximumMeteorites + 1
        );

        for (int i = 0; i < amount; i++)
        {
            GameObject meteoritePrefab =
                GetRandomValidMeteorite();

            // Stop safely if every array element is missing.
            if (meteoritePrefab == null)
            {
                Debug.LogError(
                    "Meteorite Prefabs contains missing or destroyed objects. " +
                    "Drag prefab assets from the Project window into the array.",
                    this
                );

                return;
            }

            float randomY = Random.Range(
                minimumY,
                maximumY
            );

            Vector3 spawnPosition = new Vector3(
                transform.position.x +
                i * horizontalSpacing,
                randomY,
                transform.position.z
            );

            Instantiate(
                meteoritePrefab,
                spawnPosition,
                Quaternion.identity
            );
        }
    }

    private GameObject GetRandomValidMeteorite()
    {
        // Begin checking from a random array position.
        int startingIndex =
            Random.Range(0, meteoritePrefabs.Length);

        for (int i = 0; i < meteoritePrefabs.Length; i++)
        {
            int index =
                (startingIndex + i) %
                meteoritePrefabs.Length;

            GameObject prefab =
                meteoritePrefabs[index];

            // Unity destroyed objects compare as null.
            if (prefab != null)
            {
                return prefab;
            }
        }

        return null;
    }

    private void ChooseNextSpawnDelay()
    {
        nextSpawnDelay = Random.Range(
            minimumSpawnDelay,
            maximumSpawnDelay
        );
    }
}