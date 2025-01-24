using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropdownHandler : MonoBehaviour
{
    public TMP_Dropdown saveDropdown; // Assign this in the Inspector

    private void Start()
    {
        PopulateDropdown();
    }

    private void PopulateDropdown()
    {
        List<string> saveFiles = GetSaveFileNames(); // Get save files from your save directory

        // Clear existing options
        saveDropdown.ClearOptions();

        // Add save file names to the dropdown
        saveDropdown.AddOptions(saveFiles);
    }

    private List<string> GetSaveFileNames()
    {
        string saveDirectory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        if (!System.IO.Directory.Exists(saveDirectory))
        {
            return new List<string>();
        }

        string[] saveFiles = System.IO.Directory.GetFiles(saveDirectory, "*.json");
        List<string> fileNames = new List<string>();

        foreach (string filePath in saveFiles)
        {
            fileNames.Add(System.IO.Path.GetFileNameWithoutExtension(filePath));
        }

        return fileNames;
    }


    public void OnDropdownValueChanged(int index)
    {
        List<string> saveFiles = GetSaveFileNames(); // Ensure this matches the dropdown's options
        if (index >= 0 && index < saveFiles.Count)
        {
            // Construct the full file path for the selected save
            string saveDirectory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
            string selectedFilePath = System.IO.Path.Combine(saveDirectory, saveFiles[index] + ".json");

            // Call the LoadFromFile method in MenuScript
            FindObjectOfType<MenuScript>().LoadFromFile(selectedFilePath);
        }
    }
}
