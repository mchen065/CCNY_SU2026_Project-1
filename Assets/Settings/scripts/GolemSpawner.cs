using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemSpawner : MonoBehaviour
{
    [Header("Required")]
    [SerializeField] private GameObject golemPrefab;
    [SerializeField] private Transform astronaut;

    [Header("Amount")]
    [SerializeField] private int startingGolems = 3;
    [SerializeField] private int maximumGolems = 5;

    [Header("Spawn Area")]
    [SerializeField] private float minimumDistance = 4f;
    [SerializeField] private float maximumDistance = 8f;
    [SerializeField] private float minimumY = -4f;
    [SerializeField] private float maximumY = 4f;

    [Header("Respawning")]
    [SerializeField] private float respawnDelay = 4f;

    private readonly List<GameObject> activeGolems =
        new List<GameObject>();

    private void Start()
    {
        if (astronaut == null)
        {
            AstronautMovement player =
                FindObjectOfType<AstronautMovement>();

            if (player != null)
            {
                astronaut = player.transform;
            }
        }

        if (golemPrefab == null)
        {
            Debug.LogError(
                "Golem Prefab is not assigned.",
                this
            );

            return;
        }

        if (astronaut == null)
        {
            Debug.LogError(
                "Astronaut was not found.",
                this
            );

            return;
        }

        for (int i = 0; i < startingGolems; i++)
        {
            SpawnGolem();
        }

        StartCoroutine(RespawnLoop());
    }

    private IEnumerator RespawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                respawnDelay
            );

            if (SpaceGameManager.Instance != null &&
                SpaceGameManager.Instance.GameEnded)
            {
                yield break;
            }

            activeGolems.RemoveAll(
                golem => golem == null
            );

            if (activeGolems.Count < maximumGolems)
            {
                SpawnGolem();
            }
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
                minimumDistance,
                maximumDistance
            );

        float spawnY = Random.Range(
            minimumY,
            maximumY
        );

        GameObject newGolem = Instantiate(
            golemPrefab,
            new Vector3(
                spawnX,
                spawnY,
                astronaut.position.z
            ),
            Quaternion.identity
        );

        GolemEnemy enemy =
            newGolem.GetComponent<GolemEnemy>();

        if (enemy != null)
        {
            enemy.SetTarget(astronaut);
        }
        else
        {
            Debug.LogError(
                "The golem prefab does not have GolemEnemy.",
                newGolem
            );
        }

        activeGolems.Add(newGolem);

        Debug.Log(
            "Golem spawned: " +
            newGolem.name
        );
    }
}