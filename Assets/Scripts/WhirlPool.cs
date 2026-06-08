using UnityEngine;

public class WhirlpoolPull : MonoBehaviour

{

    public Transform center;

    public float pullStrength = 2f;

    void Update()

    {

        Vector2 dir = (center.position - transform.position).normalized;

        transform.position += (Vector3)(dir * pullStrength * Time.deltaTime);

    }

}