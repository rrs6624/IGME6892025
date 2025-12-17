using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 50f;
    public float maxLifetime = 5f;
    public int damage = 1;

    void Start()
    {
        Destroy(gameObject, maxLifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Bullet hit: {other.gameObject.name} with tag: {other.tag}");

        if (other.CompareTag("Player") || other.CompareTag("Bullet"))
            return;

        EnemyMovement enemy = other.GetComponent<EnemyMovement>();
        if (enemy != null)
        {
            Debug.Log("Hit an enemy");
            enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
