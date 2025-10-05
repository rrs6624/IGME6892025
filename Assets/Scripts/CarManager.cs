using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;

public class CarManager : MonoBehaviour
{
    [Header("Car Settings")]
    public GameObject[] carPrefabs; // Assign your car prefabs in Inspector
    public Transform[] spawnPoints; // Assign spawn positions in Inspector
    public Transform[] pathPoints; //Assign the path that the cars will try to follow in the race

    [Header("PlayerMovement")]
    public InputActionReference move;
    public float moveSpeed;
    
    private Vector3 direction;
    private Rigidbody rb;

    [Header("Camera Settings")]
    public float cameraHeight = 8f;
    public float cameraDistance = 5f;
    public float cameraFollowSpeed = 1f;

    public float cameraRotationSpeed = 1f;

    private float currentRotationVelocity; // For smooth rotation
    private Vector3 currentCameraVelocity; // For smooth camera movement


    [Header("Player Settings")]
    public int playerCarIndex = 0; // Defaults to the first car being the player
    public GameObject arcGisCamera;

    [Header("Spawning Settings")]
    public float spawnDelay = 3f; // Wait for map to load
    public float verticalOffset = 2f; // Spawn cars slightly above ground

    private List<GameObject> spawnedCars = new List<GameObject>();
    private GameObject playerCar;
  

    void Start()
    {
        StartCoroutine(SpawnCarsAfterDelay());
    }

    private void Update()
    {
        //updating the camera location every frame to always be behind the player car
        Vector3 playerCarLocation  = playerCar.transform.position;


        // Calculate desired camera position based on car's current rotation
        Vector3 desiredPosition = playerCar.transform.position +
                                 playerCar.transform.up * cameraHeight -
                                 playerCar.transform.forward * cameraDistance;
        
        // Smoothly move camera to desired position
        arcGisCamera.transform.position = Vector3.SmoothDamp(
            arcGisCamera.transform.position,
            desiredPosition,
            ref currentCameraVelocity,
            cameraFollowSpeed * Time.deltaTime
        );

        // Smoothly rotate camera to look at car
        Quaternion desiredRotation = Quaternion.LookRotation(playerCar.transform.position - arcGisCamera.transform.position);
        arcGisCamera.transform.rotation = Quaternion.Slerp(
            arcGisCamera.transform.rotation,
            desiredRotation,
            cameraRotationSpeed * Time.deltaTime
        );

        // Always look at the car's position + some height
        arcGisCamera.transform.LookAt(playerCar.transform.position + new Vector3(0f, 5f, 0f));

        direction = move.action.ReadValue<Vector3>();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // Forward/backward movement (using Z component)
        Vector3 forwardMovement = playerCar.transform.forward * (direction.z * moveSpeed);

        // Apply forward movement
        rb.velocity = new Vector3(forwardMovement.x, rb.velocity.y, forwardMovement.z);

        // Rotation (using X component for steering)
        float rotation = direction.x * moveSpeed * 2f; // Adjust multiplier for rotation speed
        rb.angularVelocity = new Vector3(0f, rotation, 0f);
    }

    IEnumerator SpawnCarsAfterDelay()
    {
        // Wait for map to load
        yield return new WaitForSeconds(spawnDelay);

        SpawnAllCars();
        SetupPlayerCar();
    }

    void SpawnAllCars()
    {
        if (carPrefabs.Length == 0)
        {
            Debug.LogError("No car prefabs assigned!");
            return;
        }

        for (int i = 0; i < carPrefabs.Length; i++)
        {
            // Get spawn position (use available spawn points or default)
            Vector3 spawnPos = GetSpawnPosition(i);

            // Instantiate the car
            GameObject car = Instantiate(carPrefabs[i], spawnPos, Quaternion.identity);
            car.name = $"Car_{i}_{carPrefabs[i].name}";

            // Add to list
            spawnedCars.Add(car);

            Debug.Log($"Spawned car: {car.name} at position {spawnPos}");
        }
    }

    Vector3 GetSpawnPosition(int carIndex)
    {
        if (spawnPoints != null && carIndex < spawnPoints.Length)
        {
            return spawnPoints[carIndex].position + Vector3.up * verticalOffset;
        }
        else
        {
            // Default spawn positions in a line
            return new Vector3(carIndex * 5f, verticalOffset, 0f);
        }
    }


    void SetupPlayerCar()
    {
        if (spawnedCars.Count == 0)
        {
            Debug.LogError("No cars spawned!");
            return;
        }

        // Clamp player index to valid range
        playerCarIndex = Mathf.Clamp(playerCarIndex, 0, spawnedCars.Count - 1);
        playerCar = spawnedCars[playerCarIndex];
        rb = playerCar.GetComponent<Rigidbody>();

        // Set up player car (add player controller, etc.)
        SetupCarController(playerCar, true);

        // Set up other cars as AI
        for (int i = 0; i < spawnedCars.Count; i++)
        {
            if (i != playerCarIndex)
            {
                SetupCarController(spawnedCars[i], false);
            }
        }

        Debug.Log($"Player car set to: {playerCar.name}");
    }

    void SetupCarController(GameObject car, bool isPlayer)
    {
        if (isPlayer)
        {
            car.tag = "Player";

        }
        else
        {
            car.tag = "AI";
        }
    }

    // Public methods to switch cars (for debugging or game features)
    public void SwitchToNextCar()
    {
        playerCarIndex = (playerCarIndex + 1) % spawnedCars.Count;
        SetupPlayerCar();
    }

    public void SwitchToCar(int index)
    {
        if (index >= 0 && index < spawnedCars.Count)
        {
            playerCarIndex = index;
            SetupPlayerCar();
        }
    }

    // Getter methods
    public GameObject GetPlayerCar() => playerCar;
    public List<GameObject> GetAllCars() => spawnedCars;
    public int GetCarCount() => spawnedCars.Count;
}