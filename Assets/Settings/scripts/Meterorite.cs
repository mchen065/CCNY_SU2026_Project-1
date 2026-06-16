using UnityEngine;

public class Meterorite : MonoBehaviour
{
    [Header("Meteorite Type")]
    [SerializeField] private bool isSmall;

    [Header("Breaking")]
    [SerializeField] private GameObject breakEffect;

    private bool broken;

    public bool IsSmall => isSmall;

    public void BreakMeteorite()
    {
        if (broken)
        {
            return;
        }

        broken = true;

        if (breakEffect != null)
        {
            Instantiate(
                breakEffect,
                transform.position,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }
}