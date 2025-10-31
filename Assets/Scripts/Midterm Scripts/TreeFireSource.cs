using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeFireSource : MonoBehaviour
{
    [Header("Fire Dynamics")]
    // The fire itself deals damage to the tree's health/temperature
    public float damageToTreePerSecond = 5f;
    public float spreadRadius = 5f;
    public float spreadDelay = 3f;

    private float nextSpreadTime;
    private DamageableTree hostTree;

    public GameObject fireVisualPrefab;
    private GameObject currentFireVisual;

    void Start()
    {
        nextSpreadTime = Time.time + spreadDelay;
        hostTree = GetComponentInParent<DamageableTree>();

        if (fireVisualPrefab != null)
        {
            // 1. Instantiate the visual effect
            currentFireVisual = Instantiate(fireVisualPrefab, transform.position, Quaternion.identity, transform);

            // 2. Adjust its local position if necessary (e.g., lift it slightly off the ground)
            // currentFireVisual.transform.localPosition = new Vector3(0, 1.0f, 0); 
        }
    }

    void Update()
    {
        if (Time.time >= nextSpreadTime)
        {
            SpreadFire();
            nextSpreadTime = Time.time + spreadDelay;
        }
    }

    void SpreadFire()
    {
        // Find nearby objects within the fire radius
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, spreadRadius);

        foreach (Collider col in nearbyObjects)
        {
            // Try to find a DamageableTree component
            if (col.TryGetComponent<DamageableTree>(out var nearbyTree))
            {
                // Only spread to trees that are NOT already on fire
                if (!nearbyTree.isOnFire)
                {
                    // Call TakeDamage to heat up the neighbor tree's temperature gauge
                    // This creates a chain reaction: heat neighbors until their gauge hits 0, then they ignite.
                    nearbyTree.TakeDamage(100f); // Adjust this value based on how quickly you want fire to spread (100f is instant spread).

                    // You could also do a damage tick: nearbyTree.TakeDamage(spreadDamageAmount);
                }
            }
        }
    }

    void OnDestroy()
    {
        // Crucial: Destroy the visual effect when the FireSource component is removed
        if (currentFireVisual != null)
        {
            Destroy(currentFireVisual);
        }
    }
}
