using System.Collections.Generic;
using UnityEngine;
using TMPro; // Required for TextMeshPro
using System.IO;

[System.Serializable]
public class SaveData
{
    public List<TextItemData> textItems = new List<TextItemData>();
}

[System.Serializable]
public class TextItemData
{
    public string uniqueID;
    public string text;
    public Vector3 position;
}

public class SaveSystem : MonoBehaviour
{
    private const string SaveFileName = "saveData.json";

    public void Save()
    {
        SaveData saveData = new SaveData();

        foreach (var item in GameObject.FindGameObjectsWithTag("Saveable"))
        {
            var uniqueID = item.GetComponent<UniqueID>();
            var tmpComponent = item.GetComponent<TextMeshProUGUI>();

            if (uniqueID != null && tmpComponent != null)
            {
                TextItemData data = new TextItemData
                {
                    uniqueID = uniqueID.ID,
                    text = tmpComponent.text,
                    position = item.transform.position
                };
                saveData.textItems.Add(data);
            }
        }

        string json = JsonUtility.ToJson(saveData, true);
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, SaveFileName), json);
        Debug.Log($"Game saved to {System.IO.Path.Combine(Application.persistentDataPath, SaveFileName)}");
    }

    public void Load()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, SaveFileName);
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            foreach (var itemData in saveData.textItems)
            {
                // Find object by unique ID
                foreach (var obj in GameObject.FindGameObjectsWithTag("Saveable"))
                {
                    var uniqueID = obj.GetComponent<UniqueID>();
                    if (uniqueID != null && uniqueID.ID == itemData.uniqueID)
                    {
                        var tmpComponent = obj.GetComponent<TextMeshProUGUI>();
                        if (tmpComponent != null)
                        {
                            tmpComponent.text = itemData.text;
                        }
                    }
                    else
                    {
                        Debug.Log("object not found");
                    }
                }
            }
            Debug.Log("Game loaded successfully.");
        }
        else
        {
            Debug.LogWarning("Save file not found!");
        }
    }


}

