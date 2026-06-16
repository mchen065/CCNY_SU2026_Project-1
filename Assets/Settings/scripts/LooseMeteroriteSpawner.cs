using System.Collections;
using UnityEngine;

public class LooseMeteroriteSpawner : MonoBehaviour
{
    [Header("Prefab")]

    // Drag the blue LooseMeteorite prefab
    // from the Project window here.
    [SerializeField] private GameObject looseMeteoritePrefab;

    [Header("Spawn Timing")]
    [SerializeField] private float minimumDelay = 0.8f;
    [SerializeField] private float maximumDelay = 1.5f;

    [Header("Spawn Height")]
    [SerializeField] private float minimumY = -4f;
    [SerializeField] private float maximumY = 4f;

    private void Start()
    {
        if (looseMeteoritePrefab == null)
        {
            Debug.LogError(
                "Loose Meteorite Prefab is not assigned.",
                this
            );

            return;
        }

        SpawnMeteorite();
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float delay = Random.Range(
                minimumDelay,
                maximumDelay
            );

            yield return new WaitForSeconds(delay);

            if (SpaceGameManager.Instance != null &&
                SpaceGameManager.Instance.GameEnded)
            {
                yield break;
            }

            SpawnMeteorite();
        }
    }

    private void SpawnMeteorite()
    {
        if (looseMeteoritePrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = new Vector3(
            transform.position.x,
            Random.Range(minimumY, maximumY),
            transform.position.z
        );

        Instantiate(
            looseMeteoritePrefab,
            spawnPosition,
            Quaternion.identity
        );
    }
}