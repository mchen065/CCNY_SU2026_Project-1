using UnityEngine;

// This script creates a parallax effect.
// The background moves at a different speed from the camera.
public class Parallax : MonoBehaviour
{
    [Header("Camera")]

    // Drag the Main Camera into this field in the Inspector.
    public Transform cam;

    [Header("Parallax Settings")]

    // Controls how quickly this background layer follows the camera.
    //
    // 0 = the background does not move.
    // 0.2 = very slow movement, good for distant scenery.
    // 0.5 = medium movement.
    // 0.8 = faster movement, good for nearby scenery.
    // 1 = moves at the same speed as the camera.
    public float parallaxEffect = 0.5f;

    // Saves the background's original position.
    // This prevents its Y and Z positions from changing.
    private Vector3 startPos;

    private void Start()
    {
        // Store the position where the background began.
        startPos = transform.position;

        // Try to find the Main Camera automatically
        // if no camera was assigned in the Inspector.
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        // Stop the script if no camera was found.
        if (cam == null)
        {
            return;
        }

        // Calculate how far this background should move
        // based on the camera's horizontal position.
        float parallaxMovement =
            cam.position.x * parallaxEffect;

        // Move only along the X axis.
        //
        // The Y and Z positions stay at their original values,
        // so the background does not move vertically or
        // change its rendering depth.
        transform.position = new Vector3(
            startPos.x + parallaxMovement,
            startPos.y,
            startPos.z
        );
    }
}