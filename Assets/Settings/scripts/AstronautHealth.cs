using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AstronautHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;

    // Drag the three-heart UI image here.
    public Image healthBar;

    [Header("Damage Protection")]
    public float damageCooldown = 1f;

    private int currentHealth;
    private bool canTakeDamage = true;

    private void Start()
    {
        currentHealth = maxHealth;

        UpdateHealthBar();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Touching a meteorite removes one heart.
        if (HasTagInParents(other.transform, "Meteorite"))
        {
            TakeDamage();
            return;
        }

        // Touching an enemy removes one heart.
        if (HasTagInParents(other.transform, "Enemy"))
        {
            TakeDamage();
            return;
        }

        // A glowing bullet removes one heart.
        if (HasTagInParents(other.transform, "PowerBullet"))
        {
            TakeDamage();

            Destroy(other.transform.root.gameObject);
            return;
        }

        // Touching the spaceship wins the game.
        if (HasTagInParents(other.transform, "Spaceship"))
        {
            if (SpaceGameManager.Instance != null)
            {
                SpaceGameManager.Instance.WinGame();
            }
        }
    }

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        // Supports meteorites with Is Trigger turned off.
        if (HasTagInParents(
            collision.transform,
            "Meteorite"
        ))
        {
            TakeDamage();
            return;
        }

        // Supports enemies with Is Trigger turned off.
        if (HasTagInParents(
            collision.transform,
            "Enemy"
        ))
        {
            TakeDamage();
        }
    }

    public void TakeDamage()
    {
        if (!canTakeDamage)
        {
            return;
        }

        if (SpaceGameManager.Instance != null &&
            SpaceGameManager.Instance.GameEnded)
        {
            return;
        }

        currentHealth--;

        currentHealth = Mathf.Max(
            currentHealth,
            0
        );

        UpdateHealthBar();

        Debug.Log(
            $"Astronaut health: {currentHealth}/{maxHealth}"
        );

        if (currentHealth <= 0)
        {
            if (SpaceGameManager.Instance != null)
            {
                SpaceGameManager.Instance.LoseGame(
                    "You Lose!"
                );
            }

            return;
        }

        StartCoroutine(DamageCooldown());
    }

    private IEnumerator DamageCooldown()
    {
        canTakeDamage = false;

        yield return new WaitForSeconds(
            damageCooldown
        );

        canTakeDamage = true;
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
        {
            return;
        }

        // 3 health = full image
        // 2 health = two-thirds
        // 1 health = one-third
        // 0 health = empty
        healthBar.fillAmount =
            (float)currentHealth / maxHealth;
    }

    private bool HasTagInParents(
        Transform objectTransform,
        string requiredTag
    )
    {
        Transform current = objectTransform;

        while (current != null)
        {
            if (current.CompareTag(requiredTag))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }
}