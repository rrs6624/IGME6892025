using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Required for UI components like Slider and Text

public class SimulationManager : MonoBehaviour
{
    [Header("Data Source")]
    public DataParse dataParser;

    [Header("Visual Target")]
    public Renderer waterRenderer;

    [Header("UI Controls")]
    public Slider yearSlider;
    public TextMeshProUGUI yearText;
    public TextMeshProUGUI temperatureText;

    [Header("Color Gradient")]
    public Gradient temperatureGradient; // Define our blue-to-orange color scale

    private int currentYear;

    void Start()
    {
        // Setup the slider's range based on the loaded data
        if (dataParser != null)
        {

            if (dataParser.years.Count == 0)
            {
                dataParser.LoadTemperatureData();
            }

            if (dataParser.years.Count > 0)
            {
                {

                    yearSlider.minValue = dataParser.minYear;
                    yearSlider.maxValue = dataParser.maxYear;
                    yearSlider.value = dataParser.minYear; // Start at the earliest year

                    // Add a listener to call our method whenever the slider is moved
                    yearSlider.onValueChanged.AddListener(UpdateSimulationYear);
                }

                // Manually call the update once to initialize the scene
                UpdateSimulationYear(yearSlider.value);
            }
        }
    }

    // This function is called every time the slider changes
    public void UpdateSimulationYear(float yearValue)
    {
        currentYear = Mathf.RoundToInt(yearValue);

        // 1. Update the UI Text
        yearText.text = "Year: " + currentYear.ToString();

        // 2. Get the temperature data for this year
        float currentTemp = dataParser.GetAnnualAverage(currentYear);
        temperatureText.text = "Temp: +" + currentTemp.ToString("F2") + "°C"; // F2 formats to 2 decimal places

        // 3. Map the temperature to a color
        float normalizedTemp = Mathf.InverseLerp(-0.5f, 1.5f, currentTemp);

        Color targetColor = temperatureGradient.Evaluate(normalizedTemp);

        // 4. Apply the color to the water material
        if (waterRenderer != null)
        {
            waterRenderer.material.color = targetColor;
        }
        Debug.Log($"Year: {currentYear}, Temp: {currentTemp}, Color: {targetColor}");
    }
}