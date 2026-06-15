using UnityEngine;

public class MeteoriteSpawner : MonoBehaviour
{
    [Header("Prefab")]
    // Drag the blue prefab asset from the Project window here.
    [SerializeField] private GameObject meteoritePrefab;

    [Header("Spawning")]
    [SerializeField] private float minimumDelay = 0.8f;
    [SerializeField] private float maximumDelay = 1.5f;

    [SerializeField] private int minimumAmount = 1;
    [SerializeField] private int maximumAmount = 3;

    [SerializeField] private float minimumY = -4f;
    [SerializeField] private float maximumY = 4f;
    [SerializeField] private float spacing = 1.5f;

    private float timer;
    private float nextDelay;

    private void Start()
    {
        ChooseNextDelay();
    }

    private void Update()
    {
        if (SpaceGameManager.Instance != null &&
            SpaceGameManager.Instance.GameEnded)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= nextDelay)
        {
            SpawnWave();

            timer = 0f;
            ChooseNextDelay();
        }
    }

    private void SpawnWave()
    {
        if (meteoritePrefab == null)
        {
            Debug.LogError(
                "Meteorite Prefab is missing. " +
                "Assign the blue prefab from the Project window.",
                this
            );

            return;
        }

        int amount = Random.Range(
            minimumAmount,
            maximumAmount + 1
        );

        for (int i = 0; i < amount; i++)
        {
            float randomY = Random.Range(
                minimumY,
                maximumY
            );

            Vector3 spawnPosition = new Vector3(
                transform.position.x + i * spacing,
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

    private void ChooseNextDelay()
    {
        nextDelay = Random.Range(
            minimumDelay,
            maximumDelay
        );
    }
}