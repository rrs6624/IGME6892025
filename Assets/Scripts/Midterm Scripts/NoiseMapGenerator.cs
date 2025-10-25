using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

public class NoiseMapGenerator
{
    public TextAsset treeData;

    [System.Serializable]
    public class Feature
    {
        public Attributes attributes;
        public Geometry geometry;
    }

    [System.Serializable]
    public class Attributes
    {
        public int ESRI_OID;
        public int OBJECTID;
        public string DOMINANTVE;
        public string DOMINANTMO;
        public string SECONDVEG;
        public string SECONDMOD;
        public string THIRDVEG;
        public string THIRDMOD;
        public string ELCODE_NOT;
        public string SHORT_NAME;
        public string ECOGROUP;
        public int LEAFOFF_CC;
        public int LEAFON_CC;
        public string G_RANK;
        public string VITALNAME;
        public string SOURCEDATE;
        public string LOCATION_ID;
        public string MAPPETHOD;
        public string FCSUBTYPE;
        public string GlobalID;
        public double Shape_Area;
        public double Shape_Length;
    }

    [System.Serializable]
    public class Geometry
    {
        public List<List<List<double>>> rings;
    }

    [System.Serializable]
    public class RootObject
    {
        public List<Feature> features;
    }

    private List<Attributes> canopyDensity = new List<Attributes>();
    private Texture2D noiseTexture;
    private int textureWidth = 1024;
    private int textureHeight = 1024;

    public Texture2D GenerateNoiseMapFromCanopyData(int textureWidth, int textureHeight)
    {
        noiseTexture = new Texture2D(textureWidth, textureHeight);

        if (canopyDensity.Count == 0)
        {
            Debug.LogError("No canopy data loaded!");
            return noiseTexture;
        }

        // Normalize canopy cover values
        float maxCanopyCover = GetMaxCanopyCover();
        Debug.Log($"Max canopy cover: {maxCanopyCover}");

        // Generate the noise map
        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                // Get canopy value for this position
                float canopyValue = GetCanopyCoverAtPosition(x, y, maxCanopyCover);

                // Add some natural variation with Perlin noise
                float noiseValue = Mathf.PerlinNoise(x * 0.01f, y * 0.01f) * 0.2f;

                // Combine values
                float finalValue = Mathf.Clamp01(canopyValue + noiseValue);

                // Convert to color and set pixel
                Color color = ValueToColor(finalValue);
                noiseTexture.SetPixel(x, y, color);
            }
        }

        return noiseTexture;
    }

    private float GetMaxCanopyCover()
    {
        float max = 0;
        foreach (var feature in canopyDensity)
        {
            if (feature.LEAFON_CC > max) max = feature.LEAFON_CC;
        }
        return Mathf.Max(max, 1); // Avoid division by zero
    }

    private float GetCanopyCoverAtPosition(int x, int y, float maxCanopyCover)
    {
        // Simple spatial distribution based on position
        // You can improve this with actual geometry data later
        if (canopyDensity.Count == 0) return 0f;

        // Use a hash of position to select feature data
        int featureIndex = Mathf.Abs((x * textureWidth + y)) % canopyDensity.Count;
        float normalizedCanopy = canopyDensity[featureIndex].LEAFON_CC / maxCanopyCover;

        return normalizedCanopy;
    }

    private Color ValueToColor(float value)
    {
        // Color mapping for canopy density
        if (value < 0.2f) // Very low density
        {
            return Color.Lerp(new Color(0.3f, 0.2f, 0.1f), new Color(0.6f, 0.5f, 0.3f), value / 0.2f);
        }
        else if (value < 0.4f) // Low density
        {
            return Color.Lerp(new Color(0.6f, 0.5f, 0.3f), new Color(0.7f, 0.8f, 0.4f), (value - 0.2f) / 0.2f);
        }
        else if (value < 0.6f) // Medium density
        {
            return Color.Lerp(new Color(0.7f, 0.8f, 0.4f), new Color(0.3f, 0.6f, 0.2f), (value - 0.4f) / 0.2f);
        }
        else if (value < 0.8f) // High density
        {
            return Color.Lerp(new Color(0.3f, 0.6f, 0.2f), new Color(0.1f, 0.4f, 0.1f), (value - 0.6f) / 0.2f);
        }
        else // Very high density
        {
            return Color.Lerp(new Color(0.1f, 0.4f, 0.1f), new Color(0.05f, 0.3f, 0.05f), (value - 0.8f) / 0.2f);
        }
    }

    private void SaveNoiseMapAsPNG()
    {
        if (noiseTexture == null)
        {
            Debug.LogError("No noise texture to save!");
            return;
        }

        // Encode texture to PNG
        byte[] bytes = noiseTexture.EncodeToPNG();

        // Create filename with timestamp
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"CanopyDensityMap_{timestamp}.png";
        string filepath = Path.Combine(Application.dataPath, filename);

        // Save the file
        File.WriteAllBytes(filepath, bytes);

        Debug.Log($"Noise map saved as: {filepath}");
        Debug.Log($"File size: {bytes.Length} bytes");

#if UNITY_EDITOR
        // Refresh the AssetDatabase so the file appears in Unity Editor
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    // Optional: Public method to regenerate with different settings
    public void RegenerateAndSave(int width = 1024, int height = 1024)
    {
        textureWidth = width;
        textureHeight = height;
        GenerateNoiseMapFromCanopyData(textureWidth, textureHeight);
        SaveNoiseMapAsPNG();
    }
}