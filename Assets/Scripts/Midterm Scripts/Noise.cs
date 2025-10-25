using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
    public static TextAsset treeData;

    [System.Serializable]
    public class Feature
    {
        public Attributes attributes;
    }

    [System.Serializable]
    public class Attributes
    {
        public int LEAFON_CC;
    }

    [System.Serializable]
    public class RootObject
    {
        public List<Feature> features;
    }

    private static List<int> canopyLeafOn = new List<int>();
    public static Texture2D GetNoiseMap(int width, int height, float scale)
    {
        Texture2D newNoiseMap = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noiseValue = Mathf.PerlinNoise((float)x / width * scale, (float)y / height * scale);

                newNoiseMap.SetPixel(x, y, new Color(0, noiseValue, 0));
            }
        }
        newNoiseMap.Apply();
        return newNoiseMap;
    }

    public static void ReadJson(TextAsset dataAsset)
    {
        if (dataAsset == null)
        {
            Debug.LogError("ReadJson failed: The TextAsset is null.");
            return;
        }

        canopyLeafOn.Clear();
        RootObject rootObject = JsonUtility.FromJson<RootObject>(dataAsset.text);

        if (rootObject == null || rootObject.features == null)
        {
            Debug.LogError("JSON parsing failed or features list is null.");
            return;
        }

        foreach (Feature feature in rootObject.features)
        {
            canopyLeafOn.Add(feature.attributes.LEAFON_CC);
        }
    }

    public static Texture2D GetCanopyNoiseMap(int width, int height, float scale, TextAsset textAsset)
    {
        Texture2D newNoiseMap = new Texture2D(width, height);
        ReadJson(textAsset);

        if (canopyLeafOn.Count == 0)
        {
            Debug.LogWarning("No canopy data loaded. Generating pure Perlin Noise as fallback.");
            return GetNoiseMap(width, height, scale);
        }

        //converts 1D list to 2D grid based on number of data points
        int dataPointsPerRow = Mathf.CeilToInt(Mathf.Sqrt(canopyLeafOn.Count));
        int tileWidth = width / dataPointsPerRow;
        int tileHeight = height / dataPointsPerRow;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 1. Calculate Perlin Noise (High-frequency detail)
                float perlinNoise = Mathf.PerlinNoise((float)x / width * scale, (float)y / height * scale);

                // 2. Determine Data Influence (Low-frequency regional density)
                int tileX = x / tileWidth;
                int tileY = y / tileHeight;
                int dataIndex = tileY * dataPointsPerRow + tileX;

                float canopyValueNormalized = 0f;
                if (dataIndex < canopyLeafOn.Count)
                {
                    // Normalize the LeafOn_CC value from a percentage (0-100) to a float (0.0-1.0)
                    canopyValueNormalized = (float)canopyLeafOn[dataIndex] / 100f;
                }

                // 3. Combine Noise and Data
                // The combined value is a weighted average of the Perlin noise and the canopy data.
                float combinedValue = (perlinNoise) + (canopyValueNormalized);

                // Clamp the final value to ensure it's a valid color component
                float finalValue = Mathf.Clamp01(combinedValue);

                // Set the pixel color (using grayscale to represent tree density/probability)
                newNoiseMap.SetPixel(x, y, new Color(finalValue, finalValue, finalValue));
            }
        }

        newNoiseMap.Apply();
        return newNoiseMap;
    }
}