using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2f, -5f);
    public float followSpeed = 5f;
    public float rotationSpeed = 3f;

    [Header("Collision Avoidance")]
    public LayerMask obstacleLayers = -1;
    public float collisionOffset = 0.5f;

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired position
        Vector3 targetForward = target.forward;
        Vector3 desiredPosition = target.position +
                                 target.up * offset.y +
                                 targetForward * offset.z;

        // Handle collision avoidance
        Vector3 adjustedPosition = HandleCollision(desiredPosition, target.position);

        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, adjustedPosition, followSpeed * Time.deltaTime);

        // Look at target with smooth rotation
        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position + Vector3.up * 1f);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }

    Vector3 HandleCollision(Vector3 desiredPosition, Vector3 targetPosition)
    {
        RaycastHit hit;
        Vector3 direction = desiredPosition - targetPosition;

        if (Physics.Raycast(targetPosition, direction.normalized, out hit, direction.magnitude, obstacleLayers))
        {
            return hit.point - direction.normalized * collisionOffset;
        }

        return desiredPosition;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetupCamera(GameObject camera)
    {

        if (camera != null)
        {
            // Parent the camera to the player car
            camera.transform.SetParent(target.transform);

            // Set local position/rotation relative to car
            camera.transform.localPosition = new Vector3(0f, 5f, -10f);
            camera.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);

            Debug.Log($"Camera successfully parented to {target.name}");
        }
        else
        {
            Debug.LogError("ArcGIS camera not found in scene!");
        }
    }
}