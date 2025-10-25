using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine;
using Esri.GameEngine.MapView;
using Unity.VisualScripting;
using Esri.HPFramework;
using Esri.GameEngine.Geometry;

public class PlacementEditorWindow : EditorWindow
{

    private TextAsset treeJsonAssetCache = null;
    private TextAsset treeJsonAsset;
    private Texture2D textureNoiseMap;
    private float density = 0.5f;
    private GameObject prefab;
    public ArcGISMapComponent arcGISMap;


    [MenuItem("Tools/Tree Placement Editor")]
    public static void ShowWindow()
    {
        GetWindow<PlacementEditorWindow>("Tree placement");
    }

    private void OnGUI()
    {
        const string JSON_PATH = "Assets/JSONData/data.json";

        treeJsonAsset = (TextAsset)EditorGUILayout.ObjectField(
        "JSON Data File",
        treeJsonAsset,
        typeof(TextAsset),
        false // Do not allow scene objects (false)
        );

        textureNoiseMap = (EditorGUILayout.ObjectField("Noise Map", textureNoiseMap, typeof(Texture2D), false)) as Texture2D;
        if (GUILayout.Button("Generate Noise Map"))
        {
            if (treeJsonAssetCache == null)
            {
                treeJsonAssetCache = AssetDatabase.LoadAssetAtPath<TextAsset>(JSON_PATH);
            }
            if (treeJsonAssetCache == null)
            {
                Debug.LogError($"Load Failure: JSON file not found at '{JSON_PATH}'. Cannot generate map.");
                return;
            }
            int width = (int)Terrain.activeTerrain.terrainData.size.x;
            int height = (int)Terrain.activeTerrain.terrainData.size.z;
            float scale = 5;
            //textureNoiseMap = Noise.GetNoiseMap(width, height, scale);
            textureNoiseMap = Noise.GetCanopyNoiseMap(width, height, scale, treeJsonAsset);
            Debug.Log("Successfully generated noise map");
        }


        density = EditorGUILayout.Slider("Density", density, 0.0f, 1.0f);
        prefab = (EditorGUILayout.ObjectField("Object prefab", prefab, typeof(GameObject), false)) as GameObject;

        if(GUILayout.Button("Place Objects"))
        {
            PlaceObjects(Terrain.activeTerrain, textureNoiseMap, density, prefab);
            Debug.Log("Successfully placed objects");
        }
        if (GUILayout.Button("Debug Location"))
        {
            DebugLocation();
        }
        
    }

    private void PlaceObjects(Terrain terrain, Texture2D noiseMap, float density, GameObject prefab)
    {
        Transform parent = GameObject.FindGameObjectWithTag("Terrain").transform;
        arcGISMap = FindObjectOfType<ArcGISMapComponent>();
        double3 worldPos = arcGISMap.View.GeographicToWorld(new ArcGISPoint(0, 0, 4326));

        for (int x = 0; x < terrain.terrainData.size.x; x++)
        {
            for (int z = 0; z < terrain.terrainData.size.z; z++)
            {
                float noiseMapValue = noiseMap.GetPixel(x, z).g;
                if (Fitness(noiseMap, x, z) > 1 - density)
                {
                    Vector3 pos = new Vector3(x + Random.Range(-50.0f, 50.0f), 0, z + Random.Range(-50.0f, 50.0f));
                    pos.y = (float)worldPos.y + Terrain.activeTerrain.transform.position.y;

                    GameObject go = Instantiate(prefab, pos, Quaternion.identity, parent);
                    go.transform.SetParent(parent);
                }
            }
        }
    }

    private void DebugLocation()
    {
        arcGISMap = FindObjectOfType<ArcGISMapComponent>();
        double3 worldPos = arcGISMap.View.GeographicToWorld(new ArcGISPoint(0, 0, 4326));
        Debug.Log("World Position: " + worldPos);
        Debug.Log("Terrain position: " + Terrain.activeTerrain.transform.position);
    }

    private static float Fitness(Texture2D noiseMap, int x, int z)
    {
        float fitness = noiseMap.GetPixel(x, z).grayscale;
        return fitness;

    }
}
