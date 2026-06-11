using System.Collections.Generic;
using UnityEngine;

public class GolemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject golemPrefab;
    [SerializeField] private Transform astronaut;

    [Header("Amounts")]
    [SerializeField] private int startingGolems = 3;
    [SerializeField] private int maximumGolems = 5;

    [Header("Spawn Area")]
    [SerializeField] private float minimumDistanceBehind = 7f;
    [SerializeField] private float maximumDistanceBehind = 12f;
    [SerializeField] private float minimumY = -4f;
    [SerializeField] private float maximumY = 4f;

    [Header("Respawning")]
    [SerializeField] private float respawnDelay = 3f;

    private readonly List<GameObject> activeGolems =
        new List<GameObject>();

    private float respawnTimer;

    private void Start()
    {
        for (int i = 0; i < startingGolems; i++)
        {
            SpawnGolem();
        }
    }

    private void Update()
    {
        activeGolems.RemoveAll(
            golem => golem == null
        );

        if (activeGolems.Count >= maximumGolems)
        {
            return;
        }

        respawnTimer += Time.deltaTime;

        if (respawnTimer >= respawnDelay)
        {
            SpawnGolem();
            respawnTimer = 0f;
        }
    }

    private void SpawnGolem()
    {
        if (golemPrefab == null ||
            astronaut == null)
        {
            return;
        }

        float spawnX =
            astronaut.position.x -
            Random.Range(
                minimumDistanceBehind,
                maximumDistanceBehind
            );

        float spawnY = Random.Range(
            minimumY,
            maximumY
        );

        GameObject golem = Instantiate(
            golemPrefab,
            new Vector3(spawnX, spawnY, 0f),
            Quaternion.identity
        );

        activeGolems.Add(golem);
    }
}