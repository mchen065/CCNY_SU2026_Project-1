using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Transform cam;
    public float parallaxEffect = 0.5f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void LateUpdate()
    {
        transform.position = new Vector3(
            startPos.x + cam.position.x * parallaxEffect,
            startPos.y,
            startPos.z
        );
    }
}