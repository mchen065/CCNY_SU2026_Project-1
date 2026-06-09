using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Randomly creates whirlpools inside the visible river area.
//
// The spawner follows the camera's current position,
// but the generated whirlpools remain in normal world space.
// They should not be children of a parallax background.
public class WhirlpoolSpawner : MonoBehaviour
{
    [Header("Required Objects")]

    // Drag the complete Whirlpool prefab into this field.
    //
    // The prefab should contain:
    // - Particle System
    // - PullArea with WhirlpoolPull
    // - CenterTrigger with WhirlpoolCenter
    [SerializeField] private GameObject whirlpoolPrefab;

    // The camera used during gameplay.
    // The script automatically uses Camera.main if this is empty.
    [SerializeField] private Camera gameplayCamera;


    [Header("River Limits")]

    // The lowest Y position where a whirlpool may spawn.
    //
    // This should match the PlayerMovement riverBottomY value.
    [SerializeField] private float riverBottomY = -34f;

    // The highest Y position where a whirlpool may spawn.
    //
    // This should match the PlayerMovement riverTopY value.
    [SerializeField] private float riverTopY = -32f;


    [Header("Visible Spawn Area")]

    // Keeps whirlpools away from the left and right edges
    // of the camera so they do not appear partially off-screen.
    [SerializeField] private float edgePadding = 1f;

    // Controls the Z position of generated whirlpools.
    //
    // Use the same Z value as a manually placed whirlpool
    // that is visible in front of the water.
    [SerializeField] private float whirlpoolZ = -1f;


    [Header("Spawn Timing")]

    // The shortest possible delay between whirlpool spawns.
    [SerializeField] private float minimumSpawnDelay = 2f;

    // The longest possible delay between whirlpool spawns.
    [SerializeField] private float maximumSpawnDelay = 4f;


    [Header("Limits")]

    // Number of whirlpools generated immediately
    // when the game begins.
    [SerializeField] private int startingWhirlpools = 2;

    // Maximum number of active whirlpools allowed at one time.
    [SerializeField] private int maximumWhirlpools = 6;

    // Minimum distance required between whirlpools.
    // This prevents them from spawning on top of each other.
    [SerializeField] private float minimumSpacing = 2f;

    // Extra distance outside the camera before an old
    // whirlpool is destroyed.
    [SerializeField] private float despawnPadding = 5f;


    // Stores references to all whirlpools created by this spawner.
    private readonly List<GameObject> activeWhirlpools =
        new List<GameObject>();


    private void Awake()
    {
        // Automatically find the Main Camera if no camera
        // was assigned in the Inspector.
        if (gameplayCamera == null)
        {
            gameplayCamera = Camera.main;
        }
    }


    private IEnumerator Start()
    {
        // Wait one frame so the camera can move into
        // its correct starting position.
        yield return null;


        // Create the starting whirlpools.
        for (int i = 0; i < startingWhirlpools; i++)
        {
            TrySpawnWhirlpool();
        }


        // Continue generating whirlpools while the game runs.
        while (true)
        {
            // Choose a random delay between the minimum
            // and maximum spawn times.
            float delay = Random.Range(
                minimumSpawnDelay,
                maximumSpawnDelay
            );

            // Wait before attempting another spawn.
            yield return new WaitForSeconds(delay);


            // Delete whirlpools that are too far away
            // and clean empty references from the list.
            RemoveOldWhirlpools();


            // Stop creating whirlpools after the game ends.
            if (GameManager.Instance != null &&
                GameManager.Instance.GameEnded)
            {
                continue;
            }


            // Generate another whirlpool only when the
            // maximum amount has not been reached.
            if (activeWhirlpools.Count < maximumWhirlpools)
            {
                TrySpawnWhirlpool();
            }
        }
    }


    private void TrySpawnWhirlpool()
    {
        // Stop and display an error if the prefab
        // was not assigned in the Inspector.
        if (whirlpoolPrefab == null)
        {
            Debug.LogError(
                "Whirlpool Prefab has not been assigned.",
                this
            );

            return;
        }


        // Stop and display an error if no camera was found.
        if (gameplayCamera == null)
        {
            Debug.LogError(
                "Gameplay Camera has not been assigned.",
                this
            );

            return;
        }


        // Calculate half of the camera's visible world width.
        //
        // orthographicSize gives half of the camera's height.
        // Multiplying by aspect converts it into half-width.
        float halfCameraWidth =
            gameplayCamera.orthographicSize *
            gameplayCamera.aspect;


        // Calculate the leftmost allowed spawn position.
        float minimumX =
            gameplayCamera.transform.position.x -
            halfCameraWidth +
            edgePadding;


        // Calculate the rightmost allowed spawn position.
        float maximumX =
            gameplayCamera.transform.position.x +
            halfCameraWidth -
            edgePadding;


        // Try up to 20 random positions.
        //
        // Multiple attempts help prevent whirlpools
        // from spawning too close to one another.
        for (int attempt = 0; attempt < 20; attempt++)
        {
            // Choose a random horizontal position
            // inside the visible camera area.
            float randomX =
                Random.Range(minimumX, maximumX);


            // Choose a random vertical position
            // between the river's bottom and top limits.
            float randomY = Random.Range(
                riverBottomY,
                riverTopY
            );


            // Create the complete world position.
            Vector3 spawnPosition = new Vector3(
                randomX,
                randomY,
                whirlpoolZ
            );


            // Reject this position if it is too close
            // to another active whirlpool.
            if (IsTooClose(spawnPosition))
            {
                continue;
            }


            // Create a copy of the whirlpool prefab.
            //
            // No parent is assigned, so it does not inherit
            // movement from the parallax background.
            GameObject newWhirlpool = Instantiate(
                whirlpoolPrefab,
                spawnPosition,
                Quaternion.identity
            );


            // Make sure the generated prefab is active.
            newWhirlpool.SetActive(true);


            // Find every Particle System contained
            // inside the new whirlpool prefab.
            ParticleSystem[] particleSystems =
                newWhirlpool.GetComponentsInChildren<ParticleSystem>(
                    true
                );


            // Activate and play all Particle Systems.
            foreach (ParticleSystem particles in particleSystems)
            {
                particles.gameObject.SetActive(true);

                // Remove any particles copied from an old state.
                particles.Clear(true);

                // Begin playing the particle effect.
                particles.Play(true);
            }


            // Add the new whirlpool to the active list.
            activeWhirlpools.Add(newWhirlpool);


            // Print its position in the Console for testing.
            Debug.Log(
                $"Whirlpool spawned at {spawnPosition}",
                newWhirlpool
            );


            // The whirlpool was successfully created,
            // so stop trying additional positions.
            return;
        }


        // This appears only when none of the 20 attempted
        // positions had enough spacing.
        Debug.LogWarning(
            "Could not find an open position for a whirlpool.",
            this
        );
    }


    private bool IsTooClose(Vector3 newPosition)
    {
        // Check the proposed position against every
        // currently active whirlpool.
        foreach (GameObject whirlpool in activeWhirlpools)
        {
            // Ignore whirlpools that have already been destroyed.
            if (whirlpool == null)
            {
                continue;
            }


            // Calculate the 2D distance between the new position
            // and the existing whirlpool.
            float distance = Vector2.Distance(
                newPosition,
                whirlpool.transform.position
            );


            // Reject the new position when it is too close.
            if (distance < minimumSpacing)
            {
                return true;
            }
        }


        // No nearby whirlpool was found.
        return false;
    }


    private void RemoveOldWhirlpools()
    {
        // Stop safely if the camera is missing.
        if (gameplayCamera == null)
        {
            return;
        }


        // Calculate half of the visible camera width.
        float halfCameraWidth =
            gameplayCamera.orthographicSize *
            gameplayCamera.aspect;


        // Set the destruction boundary to the left
        // of the visible camera.
        float leftLimit =
            gameplayCamera.transform.position.x -
            halfCameraWidth -
            despawnPadding;


        // Set the destruction boundary to the right
        // of the visible camera.
        float rightLimit =
            gameplayCamera.transform.position.x +
            halfCameraWidth +
            despawnPadding;


        // Loop backward so list entries can be removed safely.
        for (int i = activeWhirlpools.Count - 1; i >= 0; i--)
        {
            GameObject whirlpool = activeWhirlpools[i];


            // Remove empty references left by whirlpools
            // that were destroyed by another script.
            if (whirlpool == null)
            {
                activeWhirlpools.RemoveAt(i);
                continue;
            }


            // Read the whirlpool's horizontal world position.
            float whirlpoolX =
                whirlpool.transform.position.x;


            // Destroy it when it travels too far outside
            // either side of the camera.
            if (whirlpoolX < leftLimit ||
                whirlpoolX > rightLimit)
            {
                Destroy(whirlpool);
                activeWhirlpools.RemoveAt(i);
            }
        }
    }
}