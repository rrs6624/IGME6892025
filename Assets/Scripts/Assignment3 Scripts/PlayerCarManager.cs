using Esri.GameEngine.MapView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCarManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private InputActionReference moveAction;
    
    private GameObject arcGisCamera;

    private Vector2 input = Vector2.zero;
    private GameObject playerCar;

    [Header("Reset Settings")]
    public Transform spawnPoint; // Assign the spawn point for this car
    public float resetHeight = 10f; // Height threshold for reset
    public float maxTiltAngle = 80f;

    void CheckForReset()
    {
        if (spawnPoint == null) return;

        // Check if car is too high above its spawn point
        float heightAboveSpawn = transform.position.y - spawnPoint.position.y;
        float heightBelowSpawn = transform.position.y + spawnPoint.position.y;

        if (heightAboveSpawn > resetHeight || heightBelowSpawn < resetHeight)
        {
            Debug.Log("Car launched too far - resetting!");
            ResetCar();
        }
    }

    void ResetCar()
    {
        if (spawnPoint == null) return;

        // Reset position to spawn point (same X and Z, spawn point's Y)
        Vector3 resetPosition = new Vector3(
            transform.position.x,  // Keep current X position
            spawnPoint.position.y, // Use spawn point's Y (ground level)
            transform.position.z   // Keep current Z position
        );

        transform.position = resetPosition;

        // Reset rotation to upright
        transform.rotation = Quaternion.identity;

        // Stop all physics movement
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"Car reset to position: {resetPosition}");
    }

    IEnumerator FindingPlayerAfterLoad()
    {
        // Wait for map to load
        yield return new WaitForSeconds(3f);

        playerCar = GameObject.FindGameObjectWithTag("Player");
        arcGisCamera = GameObject.FindGameObjectWithTag("MainCamera");

        if (rb == null)
            rb = playerCar.GetComponent<Rigidbody>();
    }
    void Start()
    {
        StartCoroutine(FindingPlayerAfterLoad());

        if (!gameObject.CompareTag("Player"))
        {
            this.enabled = false;
            return;
        }

        SetupCamera();

        if (moveAction != null)
        {
            moveAction.action.Enable();
            Debug.Log("Input action enabled!");
        }
        else
        {
            Debug.LogError("MoveAction reference is null!");
        }
    }

    void Update()
    {
        if (moveAction != null)
        {
            input = moveAction.action.ReadValue<Vector2>();


            if (input.magnitude > 0.1f)
            {
                Debug.Log($"Input detected in Update: {input}");
            }
        }
        CheckForReset();

        // Auto-right if too tilted
        if (Mathf.Abs(transform.eulerAngles.x) > maxTiltAngle ||
            Mathf.Abs(transform.eulerAngles.z) > maxTiltAngle)
        {
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            if (rb != null) rb.angularVelocity = Vector3.zero;
        }
        UpdateCameraPosition();
        
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // Simple direct movement - no physics
        float speed = 50f;
        float turnSpeed = 2f;

        // Direct position change
        transform.position += transform.forward * input.y * speed * Time.fixedDeltaTime;
        transform.Rotate(0, input.x * turnSpeed, 0);

        Debug.Log($"Direct movement - Input: {input}");
    }

    void SetupCamera()
    {
        if (playerCar == null || arcGisCamera == null)
        {
            Debug.LogError("Player car or ArcGIS camera not assigned!");
            return;
        }

        // DON'T parent the camera - just set initial position
        UpdateCameraPosition();

        Debug.Log($"Camera set to follow {playerCar.name}");
    }

    void UpdateCameraPosition()
    {
        if (playerCar == null || arcGisCamera == null) return;

        // Calculate position behind and above the car
        Vector3 cameraOffset = -playerCar.transform.forward * 5f + playerCar.transform.up * 5f;
        arcGisCamera.transform.position = playerCar.transform.position + cameraOffset;

        // Make camera look at the car
        arcGisCamera.transform.LookAt(playerCar.transform.position + new Vector3(0f, 5f, 0f));
    }
}
