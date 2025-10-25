using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataParse : MonoBehaviour
{
    // Public lists to hold the parsed data for other scripts to access
    public List<int> years = new List<int>();
    public List<float> annualAverages = new List<float>(); // The "J-D" values

    // If you want to keep the full data for potential monthly use
    public Dictionary<int, float[]> fullYearlyData = new Dictionary<int, float[]>();
    public string[] columnNames; 

    // Easy access to the range of your data
    public int minYear = int.MaxValue;
    public int maxYear = int.MinValue;

    void Start()
    {
        LoadTemperatureData();
    }

    public void LoadTemperatureData()
    {
        // Clear previous data
        years.Clear();
        annualAverages.Clear();
        fullYearlyData.Clear();

        // Load the CSV file from Resources
        TextAsset csvFile = Resources.Load<TextAsset>("GLB.Ts+dSST");

        // Split the file into lines
        string[] allLines = csvFile.text.Split('\n');

        // 1. GET THE COLUMN HEADERS
        // The 6th line (index 5) contains the column names
        string headerLine = allLines[5].Trim();
        columnNames = headerLine.Split(','); // Now we know what each column means
        Debug.Log("Found columns: " + string.Join(", ", columnNames));

        // 2. PARSE THE DATA ROWS
        for (int i = 6; i < allLines.Length; i++)
        {
            string line = allLines[i].Trim();

            // Skip empty lines and lines that don't start with a digit (a year)
            if (string.IsNullOrEmpty(line) || !char.IsDigit(line[0]))
                continue;

            // Split the current line into values
            string[] values = line.Split(',');

            // The first value is always the Year
            if (!int.TryParse(values[0], out int currentYear))
                continue; // Skip if we can't parse the year

            // Now try to parse the ANNUAL AVERAGE (the "J-D" value, which is the 14th column, index 13)
            if (values.Length > 13 && float.TryParse(values[13], out float annualAvg))
            {
                years.Add(currentYear);
                annualAverages.Add(annualAvg);

                // Update the year range
                if (currentYear < minYear) minYear = currentYear;
                if (currentYear > maxYear) maxYear = currentYear;
            }
        }

        // Final debug output
        Debug.Log($"Parsing complete! Loaded {years.Count} years of data.");
        Debug.Log($"Time range: {minYear} - {maxYear}");
    }

    // --- HELPER METHODS FOR OTHER SCRIPTS TO GET DATA ---

    // Get the annual average for a specific year
    public float GetAnnualAverage(int year)
    {
        int index = years.IndexOf(year);

        return annualAverages[index];
        
    }

    // Get the data for a specific month of a specific year
    public float GetMonthlyValue(int year, string monthName)
    {
        // Check if we have the full data and the requested year
        if (fullYearlyData.ContainsKey(year))
        {
            // Find the index of the requested month in the column names
            int monthIndex = System.Array.IndexOf(columnNames, monthName);
            if (monthIndex != -1)
            {
                // Return the value (the array index is one less than the column index)
                return fullYearlyData[year][monthIndex - 1];
            }
        }
        return float.NaN; // Return NaN if data not found
    }
}

