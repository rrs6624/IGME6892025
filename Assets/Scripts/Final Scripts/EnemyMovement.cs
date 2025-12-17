using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float baseSpeed = 3f;
    private float moveSpeed;
    private GameObject player;
    private Rigidbody rb;
    [SerializeField] private float fastSpeed = 100f;
    [SerializeField] private float colliderDisableDistance = 200f;
    private int waitTime = 0;

    public int health = 3;

    private Collider enemyCollider;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        enemyCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > colliderDisableDistance)
        {
            if (enemyCollider != null) enemyCollider.enabled = false;
            moveSpeed = fastSpeed;
            Debug.Log("Moving fast!");
        }
        else
        {
            if (enemyCollider != null) enemyCollider.enabled = true;
            moveSpeed = baseSpeed;
            Debug.Log("Slowing down!");
        }
        MoveTowardsPlayer();
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }

    public void TakeDamage(int damageAmount = 1)
    {
        health -= damageAmount;
        Debug.Log($"Enemy hit! Health: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy died!");
        Destroy(gameObject);
    }
}
