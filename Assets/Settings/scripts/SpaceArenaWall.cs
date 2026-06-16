using UnityEngine;

public class SpaceArenaWalls : MonoBehaviour
{
    [Header("Arena Size")]
    [SerializeField]
    private Vector2 arenaSize =
        new Vector2(30f, 12f);

    [SerializeField] private float wallThickness = 1f;

    [Header("Layers")]
    [SerializeField] private string playerLayerName = "Player";
    [SerializeField] private string wallLayerName = "MeteoriteWall";

    private void Start()
    {
        int playerLayer =
            LayerMask.NameToLayer(playerLayerName);

        int wallLayer =
            LayerMask.NameToLayer(wallLayerName);

        if (playerLayer == -1 || wallLayer == -1)
        {
            Debug.LogError(
                "Create Player and MeteoriteWall layers first.",
                this
            );

            return;
        }

        // The astronaut will pass through the meteorite walls.
        Physics2D.IgnoreLayerCollision(
            playerLayer,
            wallLayer,
            true
        );

        CreateWall(
            "TopWall",
            new Vector2(0f, arenaSize.y / 2f),
            new Vector2(arenaSize.x, wallThickness),
            wallLayer
        );

        CreateWall(
            "BottomWall",
            new Vector2(0f, -arenaSize.y / 2f),
            new Vector2(arenaSize.x, wallThickness),
            wallLayer
        );

        CreateWall(
            "LeftWall",
            new Vector2(-arenaSize.x / 2f, 0f),
            new Vector2(wallThickness, arenaSize.y),
            wallLayer
        );

        CreateWall(
            "RightWall",
            new Vector2(arenaSize.x / 2f, 0f),
            new Vector2(wallThickness, arenaSize.y),
            wallLayer
        );
    }

    private void CreateWall(
        string wallName,
        Vector2 localPosition,
        Vector2 wallSize,
        int wallLayer
    )
    {
        GameObject wall =
            new GameObject(wallName);

        wall.transform.SetParent(transform);
        wall.transform.localPosition =
            localPosition;

        wall.layer = wallLayer;

        BoxCollider2D wallCollider =
            wall.AddComponent<BoxCollider2D>();

        wallCollider.size = wallSize;
        wallCollider.isTrigger = false;

        PhysicsMaterial2D bounceMaterial =
            new PhysicsMaterial2D(
                wallName + "_Bounce"
            );

        bounceMaterial.friction = 0f;
        bounceMaterial.bounciness = 1f;

        wallCollider.sharedMaterial =
            bounceMaterial;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(
            transform.position,
            arenaSize
        );
    }
}