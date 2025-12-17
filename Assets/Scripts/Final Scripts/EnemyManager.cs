using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.ArcGISMapsSDK.Components;
using UnityEngine.Events;
using Esri.GameEngine;
using Esri.GameEngine.MapView;
using Esri.GameEngine.Map;
using Esri.GameEngine.Geometry;

public class EnemyManager : MonoBehaviour
{
    // Start is called before the first frame update

    private PopulationParser populationParser;
    private Dictionary<string, Dictionary<float, float>> neighborhoodCoords;

    public GameObject player;
    public GameObject enemyPrefab;
    public GameObject map;

    public float spawnInterval;

    public int maxEnemies = 1;
    private int currentEnemyCount = 0;
    private ArcGISMapComponent mapComponent;
    private bool isMapReady = false;

    [SerializeField]
    float spawnTimer = 0;
    void Start()
    {
        populationParser = FindObjectOfType<PopulationParser>();
        neighborhoodCoords = populationParser.GetNeighborhoodCoords();
        player = GameObject.FindWithTag("Player");
        mapComponent = FindObjectOfType<ArcGISMapComponent>();

        StartCoroutine(WaitForMapReady());
        EnemyMovement.OnAnyEnemyDied.AddListener(OnEnemyDeath);
    }

    IEnumerator WaitForMapReady()
    {
        while (mapComponent == null || mapComponent.View == null)
        {
            yield return new WaitForSeconds(0.5f);
        }

        isMapReady = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isMapReady)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval && currentEnemyCount < maxEnemies)
        {
            ArcGISPoint neighborhoodLocation = ClosestNeighborhood();
            SpawnEnemy(neighborhoodLocation);
            spawnTimer = 0f;
            currentEnemyCount++;
        }

    }

    double CalculateDistance(ArcGISPoint point1, ArcGISPoint point2)
    {
        double x = point1.X - point2.X;
        double y = point1.Y - point2.Y;
        double distance = System.Math.Sqrt(x * x + y * y);
        //Debug.Log("Distance : " + distance);
        return distance;
        
    }
    public void SpawnEnemy(ArcGISPoint neighborhoodLocation)
    {

        GameObject enemy = Instantiate(enemyPrefab);
        enemy.transform.SetParent(mapComponent.transform);

        float randomLonOffset = Random.Range(-0.005f, 0.005f); 
        float randomLatOffset = Random.Range(-0.005f, 0.005f);

        var geoPosition = new ArcGISPoint(
        neighborhoodLocation.X + randomLonOffset,
        neighborhoodLocation.Y + randomLatOffset,
        8,
        ArcGISSpatialReference.WGS84());

        enemy.GetComponent<ArcGISLocationComponent>().Position = geoPosition;
        //Debug.Log("GeoPosition: " + geoPosition.X + ", " + geoPosition.Y + ", " + geoPosition.Z);
    }

    void OnEnemyDeath()
    {
        ArcGISPoint neighborhoodLocation = ClosestNeighborhood();
        SpawnEnemy(neighborhoodLocation);
    }

    public int GetCurrentEnemyCount()
    {
        return currentEnemyCount;
    }

    public int GetMaxEnemies()
    {
        return maxEnemies;
    }

    ArcGISPoint ClosestNeighborhood()
    {
        double shortestDist = 17313541450723057097;
        ArcGISPoint neighborhoodLoc = null;
        foreach (string key in neighborhoodCoords.Keys)
        {
            Dictionary<float, float> coords = neighborhoodCoords[key];

            foreach (float keys in coords.Keys)
            {
                double longitude = keys;
                double latitude = coords[keys];
                var neighborhoodLocation = new ArcGISPoint(longitude, latitude, 6, ArcGISSpatialReference.WGS84());

                var playerLocation = player.GetComponent<ArcGISLocationComponent>();
                var playerPoint = new ArcGISPoint(playerLocation.Position.X, playerLocation.Position.Y, 6, ArcGISSpatialReference.WGS84());

                //calcing closest neighborhood to player
                double distance = CalculateDistance(playerPoint, neighborhoodLocation);
                if (distance < shortestDist)
                {
                    shortestDist = distance;
                    neighborhoodLoc = neighborhoodLocation;
                }
            }
        }
        return neighborhoodLoc;
    }
}
