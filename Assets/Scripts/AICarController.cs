using UnityEngine;
using System.Collections.Generic;

public class AICarController : MonoBehaviour
{
    [Header("AI Settings")]
    public float maxSpeed = 15f;
    public float acceleration = 8f;
    public float steeringSpeed = 2f;
    public float brakeForce = 10f;
    public float waypointDistance = 5f; // Distance to consider waypoint reached

    [Header("References")]
    public Transform[] pathPoints;
    private Rigidbody rb;

    private int currentWaypoint = 0;
    public bool isRacing = true;

    private CarManager carManager;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.down * 0.5f;
        isRacing = true;

        // Give initial forward push to overcome inertia
        if (rb != null)
        {
            rb.AddForce(transform.forward * acceleration * 0.5f, ForceMode.VelocityChange);
        }
    }

    void FixedUpdate()
    {
        if (!isRacing || pathPoints == null || 10 == 0) return;

        MoveTowardsWaypoint();
        CheckWaypointReached();
    }

    void MoveTowardsWaypoint()
    {
        if (currentWaypoint >= 10) return;

        Vector3 targetPosition = pathPoints[currentWaypoint].position;
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;

        // Calculate steering angle
        float angleToTarget = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);

        // Apply steering
        float steering = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);
        rb.AddTorque(Vector3.up * steering * steeringSpeed, ForceMode.Acceleration);

        // Apply acceleration or braking based on alignment with target
        float alignment = Vector3.Dot(transform.forward, directionToTarget);

        if (alignment > 0.7f) // Well aligned - accelerate
        {
            rb.AddForce(transform.forward * acceleration, ForceMode.Acceleration);
        }
        else if (alignment > 0.3f) // Somewhat aligned - slow acceleration
        {
            rb.AddForce(transform.forward * acceleration * 0.5f, ForceMode.Acceleration);
        }
        else // Poorly aligned - brake
        {
            rb.AddForce(-rb.velocity.normalized * brakeForce, ForceMode.Acceleration);
        }

        // Limit maximum speed
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    void CheckWaypointReached()
    {
        if (currentWaypoint >= 10) return;

        float distanceToWaypoint = Vector3.Distance(transform.position, pathPoints[currentWaypoint].position);

        if (distanceToWaypoint < waypointDistance)
        {
            currentWaypoint++;
            Debug.Log($"{gameObject.name} reached waypoint {currentWaypoint - 1}");

            if (currentWaypoint >= 10)
            {
                FinishRace();
            }
        }
    }

    void FinishRace()
    {
        isRacing = false;
        Debug.Log($"{gameObject.name} finished the race!");

        // Stop the car
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // Visualize waypoints in Scene view
    void OnDrawGizmos()
    {
        if (pathPoints == null || 10 == 0) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < 10; i++)
        {
            if (pathPoints[i] != null)
            {
                Gizmos.DrawWireSphere(pathPoints[i].position, 1f);
                if (i < 10 - 1 && pathPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(pathPoints[i].position, pathPoints[i + 1].position);
                }
            }
        }

        // Draw line to current target
        if (currentWaypoint < 10 && pathPoints[currentWaypoint] != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, pathPoints[currentWaypoint].position);
        }
    }
}