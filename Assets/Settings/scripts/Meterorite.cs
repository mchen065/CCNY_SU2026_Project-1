using UnityEngine;

public class Meterorite : MonoBehaviour
{
    [SerializeField] private float speed = 4f;
    [SerializeField] private float destroyX = -20f;

    private void Update()
    {
        transform.position +=
            Vector3.left * speed * Time.deltaTime;

        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
}