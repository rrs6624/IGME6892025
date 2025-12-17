using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine;
using Esri.GameEngine.MapView;
using Esri.GameEngine.Map;
using Esri.GameEngine.Geometry;


//data source: https://catalog.data.gov/dataset/new-york-city-population-by-neighborhood-tabulation-areas
//https://data.cityofnewyork.us/City-Government/2020-Neighborhood-Tabulation-Areas-NTAs-Mapped/4hft-v355
public class PopulationParser : MonoBehaviour
{
    List<int> populationIndex = new List<int>();
    List<string> neighborhoodIndex = new List<string>();
    List<int> yearIndex = new List<int>();
    List<string> multiPolygons = new List<string>();
    
    List<string>NTAName = new List<string>();
    
    Dictionary<string, Dictionary<float, float>> neighborhoodCoords = new Dictionary<string, Dictionary<float, float>>();

    public GameObject spawnNodes;
    public GameObject map;
    // Start is called before the first frame update
    void Start()
    {
        ReadCSVFile();
        ReadNeighborhood();
        //SpawnNodes();
    }

    void ReadCSVFile()
    {
        StreamReader streamReader = new StreamReader("Assets/JSONData/ManhattanPopulation.csv");
        bool endOfFile = false;

        while (!endOfFile)
        {
            string data_String = streamReader.ReadLine();
            if (data_String == null)
            {
                endOfFile = true;
                break;
            }
            
            var data_values = data_String.Split(',');
            // Example processing: Log the first two columns
            //Debug.Log("Column 1: " + data_values[0] + ", Column 2: " + data_values[1] + ", Column 3: "
            //    + data_values[2] + ", Column 4: " + data_values[3] + ", Column 5: " + data_values[4]
            //    + ", Column 6: " + data_values[5]);
            yearIndex.Add(int.Parse(data_values[1]));
            neighborhoodIndex.Add(data_values[4]);
            populationIndex.Add(int.Parse(data_values[5]));

        }
        streamReader.Close();
    }

    void ReadNeighborhood()
    {
        StreamReader streamReader = new StreamReader("Assets/JSONData/Neighborhood2.csv");
        bool endOfFile = false;
        while (!endOfFile)
        {
            string data_String = streamReader.ReadLine();
            string nextLine = streamReader.ReadLine();

            if (nextLine == null)
            {
                endOfFile = true;
                break;
            }
            
            var data_values = data_String.Split(',');

            NTAName.Add(data_values[4]);
            multiPolygons.Add(data_values[11]);

            data_String = nextLine;
            nextLine = streamReader.ReadLine();
        }
        streamReader.Close();

        for (int index = 0; index < NTAName.Count; index++)
        {
            string neighborhoodName = NTAName[index];
            string polygonData = multiPolygons[index];

            //Debug.Log("MultiPolygon Count: " + multiPolygons.Count);
            //Debug.Log("NTAName Count: " + NTAName.Count);

            List<float> lon = new List<float>();
            List<float> lat = new List<float>();

            string temp = polygonData;
            temp = temp.Replace("MULTIPOLYGON", "");
            temp = temp.Trim();
            temp = temp.Replace("(", "");
            temp = temp.Replace(")", "");
            temp = temp.Replace(" ", "`");

            var coords = temp.Split('`', StringSplitOptions.RemoveEmptyEntries);

            //Debug.Log("Coords count: " + coords.Length);

            if (coords.Length > 0)
            {
                for (int i = 0; i < coords.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        lon.Add(float.Parse(coords[i]));
                    }
                    else
                    {
                        lat.Add(float.Parse(coords[i]));
                    }
                }
            }

            float centroidX = 0;
            float centroidY = 0;

            if (lon.Count > 0)
            {
                for (int j = 0; j < lon.Count; j++)
                {
                    centroidX += lon[j];
                    centroidY += lat[j];
                }

                centroidX = centroidX / lon.Count;
                centroidY = centroidY / lat.Count;
            }

            Dictionary<float, float> coordPair = new Dictionary<float, float>();
            coordPair.Add(centroidX, centroidY);

            neighborhoodCoords.TryAdd(neighborhoodName, coordPair);

        }
    }
    void SpawnNodes()
    {
        var mapComponent = FindObjectOfType<ArcGISMapComponent>();
        foreach (var neighborhood in neighborhoodCoords)
        {
            string neighborhoodName = neighborhood.Key;
            Dictionary<float, float> coords = neighborhood.Value;

            double longitude = 0;
            double latitude = 0;

            foreach (var coordPair in coords)
            {
                longitude = coordPair.Key;
                latitude = coordPair.Value;
                //Debug.Log($"Neighborhood: {neighborhoodName}, Lon: {longitude}, Lat: {latitude}");
                break; 
            }

            GameObject node = new GameObject(neighborhoodName + " Node");
            node.AddComponent<ArcGISLocationComponent>();

            var geoPosition = new ArcGISPoint(longitude, latitude, 10, ArcGISSpatialReference.WGS84());
            var worldPosition = mapComponent.View.GeographicToWorld(geoPosition);
            //Debug.Log("GeoPosition: " + geoPosition.X + ", " + geoPosition.Y + ", " + geoPosition.Z);
            //Debug.Log("World Position: " + worldPosition.x + ", " + worldPosition.y + ", " + worldPosition.z);
            node.transform.SetParent(map.transform);

            Vector3 worldPos = new Vector3((float)worldPosition.x, (float)worldPosition.y, (float)worldPosition.z);
            //node.transform.position = worldPos;

            if (spawnNodes != null)
            {
                GameObject visual = Instantiate(spawnNodes, node.transform);
                node.GetComponent<ArcGISLocationComponent>().Position = geoPosition;
                visual.transform.localPosition = Vector3.zero;
            }

            //Debug.Log($"Spawned node at {neighborhoodName}: ({longitude}, {latitude})");
        }
    }

    public Dictionary<string, Dictionary<float, float>> GetNeighborhoodCoords()
    {
        return neighborhoodCoords;
    }


}
