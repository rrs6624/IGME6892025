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
   
    private Rigidbody rb;

    [Header("Player Settings")]
    public int playerCarIndex = 0; // Defaults to the first car being the player
    

    [Header("Spawning Settings")]
    public float spawnDelay = 3f; // Wait for map to load
    public float verticalOffset = 2f; // Spawn cars slightly above ground

    private List<GameObject> spawnedCars = new List<GameObject>();
    private GameObject playerCar;

    [Header("Win Con")]
    public Transform finishLine; // Drag your final checkpoint here
    public float finishLineDistance = 10f;
    private GameObject winningCar;
    private bool raceFinished = false;


    void Start()
    {
        StartCoroutine(SpawnCarsAfterDelay());

        
    }
    IEnumerator SpawnCarsAfterDelay()
    {
        // Wait for map to load
        yield return new WaitForSeconds(spawnDelay);

        SpawnAllCars();
        SetupPlayerCar();
        
    }

    private void Update()
    {
        CheckForWinner();
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

        playerCarIndex = Mathf.Clamp(playerCarIndex, 0, spawnedCars.Count - 1);
        playerCar = spawnedCars[playerCarIndex];
        rb = playerCar.GetComponent<Rigidbody>();

        SetupCarController(playerCar, true);

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
            PlayerCarManager playerManager = car.GetComponent<PlayerCarManager>();
            if (playerManager != null) playerManager.enabled = true;

            AICarController aiController = car.GetComponent<AICarController>();
            if (aiController != null) aiController.enabled = false;
        }
        else
        {
            car.tag = "AI";
            PlayerCarManager playerManager = car.GetComponent<PlayerCarManager>();
            if (playerManager != null) playerManager.enabled = false;

            AICarController aiController = car.GetComponent<AICarController>();
            if (aiController == null)
            {
                aiController = car.AddComponent<AICarController>();
            }

            // Make sure path points are assigned BEFORE enabling
            if (pathPoints != null && pathPoints.Length > 0)
            {
                aiController.pathPoints = pathPoints;
                Debug.Log($"Assigned {pathPoints.Length} waypoints to {car.name}");
            }
            else
            {
                Debug.LogError("No path points assigned in CarManager!");
            }

            aiController.enabled = true;
            aiController.isRacing = true; // Force start racing
        }
    }
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

    public Transform[] GetPath()
    {
        return pathPoints;
    }

    public Rigidbody GetRigidbody(int index = 0)
    {
        if (index >= spawnedCars.Count)
        {
            rb = spawnedCars[index].GetComponent<Rigidbody>();
            return rb;
        }
        rb = playerCar.GetComponent<Rigidbody>();
        return rb;
    }

    public int GetPlayerIndex()
    {
        return playerCarIndex;
    }

    void CheckForWinner()
    {
        foreach (GameObject car in spawnedCars)
        {
            if (car == null) continue;

            // Check if car is close to finish line
            float distanceToFinish = Vector3.Distance(car.transform.position, finishLine.position);

            if (distanceToFinish < finishLineDistance && !raceFinished)
            {
                winningCar = car;
                raceFinished = true;
                DeclareWinner(car);
                break; // Stop checking once we have a winner
            }
        }
    }
    void DeclareWinner(GameObject winner)
    {
        Debug.Log(" RACE FINISHED! ");
        Debug.Log("WINNER: " + winner.name);

        if (winner.CompareTag("Player"))
        {
            Debug.Log(" YOU WIN! ");
        }
        else
        {
            Debug.Log(" YOU LOSE! Winner: " + winner.name);
        }

    }
}