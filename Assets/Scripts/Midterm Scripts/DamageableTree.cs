using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableTree : MonoBehaviour
{
    [Header("Temperature/Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    public GameObject fireSourcePrefab;

    public bool isOnFire { get; private set; } = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isOnFire) return;

        currentHealth -= amount;

        currentHealth = Mathf.Max(currentHealth, 0f);

        if (currentHealth <= 0 && !isOnFire)
        {
            Debug.Log($"{gameObject.name} temperature reached 0. IGNITING!");
            Ignite();
        }
    }

    public void Ignite()
    {
        if (isOnFire || fireSourcePrefab == null) return;

        isOnFire = true;

        GameObject fireSourceInstance = Instantiate(fireSourcePrefab, transform.position, Quaternion.identity, transform);

    }

    // Optional: Add a method for cooling down/healing
    public void CoolDown(float amount)
    {
        if (isOnFire) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
}