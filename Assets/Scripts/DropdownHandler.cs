using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropdownHandler : MonoBehaviour
{
    public TMP_Dropdown saveDropdown; // Assign this in the Inspector

    private List<string> saveFilePaths; // Store full paths for consistency

    private void Start()
    {
        PopulateDropdown();
    }

    public void PopulateDropdown()
    {
        saveFilePaths = GetSaveFilePaths(); // Get save files with full paths

        // Prepare list for dropdown options
        List<string> fileNames = new List<string>
        {
            "No Save" // Add "No Save" as the first option
        };

        foreach (string filePath in saveFilePaths)
        {
            fileNames.Add(System.IO.Path.GetFileNameWithoutExtension(filePath)); // Extract filenames for display
        }

        saveDropdown.ClearOptions(); // Clear any previous options
        saveDropdown.AddOptions(fileNames); // Add filenames to the dropdown
    }

    private List<string> GetSaveFilePaths()
    {
        string saveDirectory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        if (!System.IO.Directory.Exists(saveDirectory))
        {
            return new List<string>(); // Return empty list if the directory doesn't exist
        }

        return new List<string>(System.IO.Directory.GetFiles(saveDirectory, "*.json"));
    }

    public void OnDropdownValueChanged(int index)
    {
        if (index == 0)
        {
            Debug.Log("No Save selected. No file will be loaded.");
            return; // Do nothing for "No Save" option
        }

        if (saveFilePaths == null || saveFilePaths.Count == 0)
        {
            Debug.LogError("Save file paths are empty or null. PopulateDropdown might not have run correctly.");
            return;
        }

        int adjustedIndex = index - 1; // Adjust index because "No Save" is the first item

        if (adjustedIndex >= 0 && adjustedIndex < saveFilePaths.Count)
        {
            string selectedFilePath = saveFilePaths[adjustedIndex]; // Use the stored full path
            Debug.Log($"Dropdown Index: {index}, File Path: {selectedFilePath}");
            FindObjectOfType<MenuScript>().LoadFromFile(selectedFilePath); // Call LoadFromFile
        }
        else
        {
            Debug.LogError($"Invalid index selected in dropdown: {index}");
        }
    }
}
